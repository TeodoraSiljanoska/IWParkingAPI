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

                TimeSpan parsedStartTime;
                TimeSpan parsedEndTime;
                if (!TimeSpan.TryParse(request.StartTime, out parsedStartTime) ||
                    !TimeSpan.TryParse(request.EndTime, out parsedEndTime))
                {
                    throw new BadRequestException("Invalid start or end time format");
                }
                DateTime startDateTime = request.StartDate.Date.Add(parsedStartTime);
                DateTime endDateTime = request.EndDate.Date.Add(parsedEndTime);
                if (startDateTime >= endDateTime || startDateTime < DateTime.Now || endDateTime < DateTime.Now)
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
                }

                //if working hours are after midnight (ex. from 21 - 12)
                //   var totalWorkingHours = (parkingLot.WorkingHourTo - parkingLot.WorkingHourFrom).TotalHours;
                /*       if (totalWorkingHours < 0)
                       {
                           // if ((parsedEndTime - parsedStartTime).TotalHours < 0)
                           // {    //ex. reservation from 23h - 01h the next day

                           //pst=18:30 pl.whf=21 18>=21 false && 02:30<=12 true => false || 18<=21 true && 02<=12 true true
                           //13:30 - 14:30
                           //  if ((parsedEndTime - parsedStartTime).TotalHours > 0)
                           //21-14
                           if ((parsedEndTime - parsedStartTime).TotalHours > 0)
                           {
                               if (!(parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                               {
                                   throw new BadRequestException("Parking Lot isn't available during those hours");
                               }
                           }
                           else if ((parsedEndTime - parsedStartTime).TotalHours <0)
                           {
                               if (((parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo) ||
                                    parsedStartTime <= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                               {
                                   throw new BadRequestException("Parking Lot isn't available during those hours");
                               }
                               else if  
                                    (!((parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo) ||
                                    parsedStartTime <= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                                   {
                                       throw new BadRequestException("Parking Lot isn't available during those hours");
                                   }
                               }

                           }
                            //}


                        /*   if (!((parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= TimeSpan.FromHours(24)) ||
                (parsedStartTime >= TimeSpan.Zero && parsedEndTime <= parkingLot.WorkingHourTo)))
                           {
                               throw new BadRequestException("Parking Lot isn't available during those hours");
                           }*/
            
                
                //LAST COMMENT

                /*
               var totalWorkingHours = (parkingLot.WorkingHourTo - parkingLot.WorkingHourFrom).TotalHours;
                if (request.StartDate < request.EndDate)
                {
                    // Check if the reservation's start time is after working hours on the start date
                    if (parsedStartTime < parkingLot.WorkingHourFrom)
                    {
                        throw new BadRequestException("Parking Lot isn't available during those hours");
                    }

                    // Check if the reservation's end time is after working hours on the end date
                    if (parsedEndTime > parkingLot.WorkingHourTo)
                    {
                        throw new BadRequestException("Parking Lot isn't available during those hours");
                    }
                }
                else
                {
                    // Reservation within the same day (no crossing midnight)
                    if (totalWorkingHours < 0)
                    {
                        // Calculate the effective end time for the parking lot
                        var effectiveWorkingHourTo = parkingLot.WorkingHourTo.Add(TimeSpan.FromDays(1)); // Add one day to represent crossing midnight

                        if (!(parsedStartTime >= parkingLot.WorkingHourFrom || parsedEndTime <= effectiveWorkingHourTo))
                        {
                            throw new BadRequestException("Parking Lot isn't available during those hours");
                        }
                    }
                    else
                    {
                        if (!(parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                        {
                            throw new BadRequestException("Parking Lot isn't available during those hours");
                        }
                    }
                }
                
             /* if (totalWorkingHours < 0)
                { 
                    var effectiveWorkingHours = totalWorkingHours + 24;
                  

                    if (!(parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                    {
                        throw new BadRequestException("Parking Lot isn't available during those hours");
                    }
                }  */
                /*    else
                    {
                        if (!(parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                        {
                            throw new BadRequestException("Parking Lot isn't available during those hours");
                        }
                    }*/ 

                if (/*totalWorkingHours >0 && */(parsedStartTime < parkingLot.WorkingHourFrom || parsedEndTime > parkingLot.WorkingHourTo))
            {
                throw new BadRequestException("Parking Lot isn't available during those hours");
            }

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
                   start <= endDateTime && // Check for time range overlap
                   end >= startDateTime) || (start == startDateTime && end == endDateTime))
                   .FirstOrDefault();

                if (res != null)
                {
                    throw new BadRequestException("A reservation with the same vehicle and overlapping time range already exists.");
                }

            }

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

            var reservation = _mapper.Map<ReservationDTO>(request);
            reservation.StartDate = startDateTime.Date;
            reservation.IsPaid = true;
            reservation.Type = Enums.ReservationTypes.Successful.ToString();
            double totalPrice = CalculateReservationPrice(startDateTime, endDateTime, parkingLot.Price);
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
