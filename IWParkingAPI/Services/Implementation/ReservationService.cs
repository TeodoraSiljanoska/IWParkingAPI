using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Enums;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IJWTDecode _jWTDecode;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICalculateCapacityExtension _calculateCapacityExtension;
        private readonly MakeReservationResponse _makeReservationResponse;




        public ReservationService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode,
            ICalculateCapacityExtension calculateCapacityExtension)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _reservationRepository = _unitOfWork.GetGenericRepository<Reservation>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _jWTDecode = jWTDecode;
            _calculateCapacityExtension = calculateCapacityExtension;
            _makeReservationResponse = new MakeReservationResponse();
        }
        public MakeReservationResponse MakeReservation(MakeReservationRequest request)
        {
            try
            {
                var userId = _jWTDecode.ExtractClaimByType("Id");
                if (userId == null)
                {
                    throw new BadRequestException("Please login to make a reservation");
                }

                var user = _userRepository.GetAsQueryable(x => x.Id == Convert.ToInt32(userId), null, x => x.Include(y => y.Vehicles)).FirstOrDefault();
                var selectedVehicle = user.Vehicles.Where(x => x.PlateNumber == request.PlateNumber).FirstOrDefault();

                if (selectedVehicle == null)
                {
                    throw new BadRequestException("Please select a valid vehicle to make a reservation");
                }

                var parkingLot = _parkingLotRepository.GetAsQueryable(x => x.Id == request.ParkingLotId, null, null).FirstOrDefault();

                if (parkingLot == null)
                {
                    throw new BadRequestException("Please select a valid Parking Lot to make a reservation");
                }

                //convert from request - from string to TimeSpan
                TimeSpan parsedStartTime;
                TimeSpan parsedEndTime;
                if (!TimeSpan.TryParse(request.StartTime, out parsedStartTime) ||
                    !TimeSpan.TryParse(request.EndTime, out parsedEndTime))
                {
                    throw new BadRequestException("Invalid start or end time format");
                }

                //DateTime for reservation start and end
                DateTime reservationStartDateTime = request.StartDate.Date.Add(parsedStartTime);
                DateTime reservationEndDateTime = request.EndDate.Date.Add(parsedEndTime);

                if(reservationStartDateTime < DateTime.Now || reservationEndDateTime < DateTime.Now)
                {
                    throw new BadRequestException("Please enter valid date and time range to make a reservation");
                }
                //if the working hours of the parking lot are overnight
                bool workingHourisOvernight = false;
                //workinghoursfrom
                DateTime parkingLotWorkingStart = DateTime.Now.Date.Add(parkingLot.WorkingHourFrom);
                DateTime parkingLotWorkingEnd;
                DateTime fromPreviosWorkingDayEnd = DateTime.MinValue;
                DateTime fromPreviousWorkingDayStart = DateTime.MaxValue;

                //check if the reservation date and time range is valid
                if (reservationEndDateTime <= reservationStartDateTime)
                {
                    throw new BadRequestException("Please enter valid date and time range to make a reservation");
                }

                //if the workingHourTo <= workingHourFrom, then the workingHours of the Parking Lot are overnight
                if (parkingLot.WorkingHourTo <= parkingLot.WorkingHourFrom)
                {
                    workingHourisOvernight = true;
                }

                //if the workingHours are overnight, then one day is added to calculate the workingHourTo
                if (workingHourisOvernight == true)
                {
                    fromPreviosWorkingDayEnd = DateTime.Now.Date.Add(parkingLot.WorkingHourTo);
                    fromPreviousWorkingDayStart = parkingLotWorkingStart.AddDays(-1);
                    parkingLotWorkingEnd = (DateTime.Now.Date.AddDays(1)).Date.Add(parkingLot.WorkingHourTo);
                }
                else
                {
                    parkingLotWorkingEnd = DateTime.Now.Date.Add(parkingLot.WorkingHourTo);
                }

                /* if(fromPreviosWorkingDayToEnd != DateTime.MinValue && (reservationStartDateTime < parkingLotWorkingStart.AddDays(-1)
                     || reservationEndDateTime > fromPreviosWorkingDayToEnd || reservationEndDateTime > parkingLotWorkingEnd))*/
                TimeSpan tempEndOfDay = new TimeSpan(0, 23, 59, 59, 59);
                DateTime endOfPreviousDay = fromPreviousWorkingDayStart.Date.Add(tempEndOfDay);
                TimeSpan tempEndOfWorkingHours = new TimeSpan(0, 23, 59, 59, 59);
                if (workingHourisOvernight == true)
                {
                    if(!((reservationStartDateTime >= fromPreviousWorkingDayStart && reservationStartDateTime >= endOfPreviousDay 
                        && reservationEndDateTime <= fromPreviosWorkingDayEnd) ||
                        (reservationStartDateTime >= parkingLotWorkingStart && reservationEndDateTime <= parkingLotWorkingEnd)))

                  /*  if (reservationStartDateTime < parkingLotWorkingStart || reservationStartDateTime.Hour < fromPreviousWorkingDayStart.Hour
                        || reservationStartDateTime > fromPreviosWorkingDayEnd
                        || reservationEndDateTime.Hour > fromPreviosWorkingDayEnd.Hour || reservationEndDateTime > parkingLotWorkingEnd)*/
                        throw new BadRequestException("Please enter valid date and time range to make a reservation");
                }
                else
                {
                    if (reservationStartDateTime < parkingLotWorkingStart || reservationEndDateTime > parkingLotWorkingEnd)
                    {
                        throw new BadRequestException("Please enter valid date and time range to make a reservation");
                    }
                }


                //OLD VALIDATIONS
                /* 
                 if (reservationStartDateTime >= reservationEndDateTime || reservationStartDateTime < DateTime.Now || reservationEndDateTime < DateTime.Now)
                 {
                     throw new BadRequestException("Please enter valid date and time range to make a reservation");
                 }

                 if ((parsedEndTime - parsedStartTime).TotalHours == 0 &&
                     (request.EndDate == request.StartDate || request.EndDate < request.StartDate))
                 {
                     throw new BadRequestException("Please enter valid date and time range to make a reservation");
                 }
                 if ((parsedEndTime - parsedStartTime).TotalHours > 0)
                 {
                     if (request.EndDate == request.StartDate && parsedEndTime < parsedStartTime)
                     {
                         throw new BadRequestException("Please enter valid date and time range to make a reservation");
                     }
                     if (parsedEndTime < parsedStartTime || request.EndDate < request.StartDate)
                     {
                         throw new BadRequestException("Please enter valid date and time range to make a reservation");
                     }
                 }
                 if ((parsedEndTime - parsedStartTime).TotalHours < 0)
                 {
                     if (request.EndDate <= request.StartDate)
                     {
                         throw new BadRequestException("Please enter valid date and time range to make a reservation");
                     }
                 }*/





                //  if (/*totalWorkingHours >0 && */(parsedStartTime < parkingLot.WorkingHourFrom || parsedEndTime > parkingLot.WorkingHourTo))
                /* {
                     throw new BadRequestException("Parking Lot isn't available during those hours");
                 } */


                CheckForExistingReservation(request, user, selectedVehicle, reservationStartDateTime, reservationEndDateTime);

                var madeReservations = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId), selectedVehicle.Type, parkingLot.Id,
                     request.StartDate.Date, parsedStartTime, request.EndDate.Date, parsedEndTime);

                if (selectedVehicle.Type.Equals(Enums.VehicleTypes.Car.ToString()))
                {
                    var availableCapacity = parkingLot.CapacityCar - madeReservations;
                    if (availableCapacity == 0)
                    {
                        throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                    }
                }

                if (selectedVehicle.Type.Equals(Enums.VehicleTypes.AdaptedCar.ToString()))
                {
                    var availableAdaptedCapacity = parkingLot.CapacityAdaptedCar - madeReservations;
                    if (availableAdaptedCapacity == 0)
                    {
                        var madeReservationsWithCar = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId),
                        Enums.VehicleTypes.Car.ToString(), parkingLot.Id,
                        request.StartDate, parsedStartTime, request.EndDate, parsedEndTime);

                        var availableCarCapacity = parkingLot.CapacityCar - madeReservations;

                        if (availableCarCapacity == 0)
                        {
                            throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                        }
                    }
                }

                InsertReservation(request, userId, selectedVehicle, parkingLot, reservationStartDateTime, reservationEndDateTime);
                return _makeReservationResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for MakeReservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while making the Reservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while making the Reservation");
            }
        }

        private void InsertReservation(MakeReservationRequest request, string userId, Vehicle selectedVehicle, ParkingLot parkingLot, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            var reservation = _mapper.Map<ReservationDTO>(request);
            reservation.StartDate = reservationStartDateTime.Date;
            reservation.IsPaid = true;
            reservation.Type = Enums.ReservationTypes.Successful.ToString();
            double totalPrice = CalculateReservationPrice(reservationStartDateTime, reservationEndDateTime, parkingLot.Price);
            totalPrice = Math.Ceiling(totalPrice);
            reservation.Amount = (int)totalPrice;
            reservation.UserId = int.Parse(userId);
            reservation.ParkingLotId = parkingLot.Id;
            reservation.VehicleId = selectedVehicle.Id;
            reservation.TimeCreated = DateTime.Now;

            var reservationToInsert = _mapper.Map<Reservation>(reservation);
            _reservationRepository.Insert(reservationToInsert);
            _unitOfWork.Save();

            _makeReservationResponse.StatusCode = HttpStatusCode.OK;
            _makeReservationResponse.Message = "Reservation made successfully";
            _makeReservationResponse.Reservation = _mapper.Map<ReservationDTO>(reservationToInsert);
        }

        private void CheckForExistingReservation(MakeReservationRequest request, AspNetUser? user, Vehicle? selectedVehicle, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            List<Reservation> existingReservation = _reservationRepository.GetAsQueryable(x =>
                        x.UserId == user.Id &&
                        x.VehicleId == selectedVehicle.Id).ToList();
            foreach (var x in existingReservation)
            {
                DateTime start = x.StartDate.Add(x.StartTime);
                DateTime end = x.EndDate.Add(x.EndTime);
                // }
                var res = existingReservation.Where(x => (x.StartDate <= request.EndDate && // Check for date range overlap
                   x.EndDate >= request.StartDate &&
                   start <= reservationEndDateTime && // Check for time range overlap
                   end >= reservationStartDateTime) || (start == reservationStartDateTime && end == reservationEndDateTime))
                   .FirstOrDefault();

                if (res != null)
                {
                    throw new BadRequestException("A reservation with the same vehicle and overlapping time range already exists.");
                }

            }
        }

        private double CalculateReservationPrice(DateTime startDateTime, DateTime endDateTime, double hourlyRate)
        {
            // Calculate the total duration of the reservation in hours
            TimeSpan reservationDuration = endDateTime - startDateTime;

            // Calculate the total price based on the hourly rate
            double totalPrice = reservationDuration.TotalHours * hourlyRate;

            return totalPrice;
        }
    }
}
