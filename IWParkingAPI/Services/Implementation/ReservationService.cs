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
        private readonly ReservationResponse _reservationResponse;
        private readonly IEnumsExtension<Enums.VehicleTypes> _enumsExtensionVehicleTypes;

        public ReservationService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode,
            ICalculateCapacityExtension calculateCapacityExtension, IEnumsExtension<Enums.VehicleTypes> enumsExtension)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _reservationRepository = _unitOfWork.GetGenericRepository<Reservation>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _jWTDecode = jWTDecode;
            _calculateCapacityExtension = calculateCapacityExtension;
            _reservationResponse = new ReservationResponse();
            _enumsExtensionVehicleTypes = enumsExtension;
        }
        public ReservationResponse MakeReservation(MakeReservationRequest request)
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
                TimeSpan reservationStartTime;
                TimeSpan reservationEndTime;
                if (!TimeSpan.TryParse(request.StartTime, out reservationStartTime) ||
                    !TimeSpan.TryParse(request.EndTime, out reservationEndTime))
                {
                    throw new BadRequestException("Invalid start or end time format");
                }

                //DateTime for reservation start and end
                DateTime reservationStartDateTime = request.StartDate.Date.Add(reservationStartTime);
                DateTime reservationEndDateTime = request.EndDate.Date.Add(reservationEndTime);
                if (reservationStartDateTime <= DateTime.Now || reservationEndDateTime <= DateTime.Now ||
                    reservationEndDateTime <= reservationStartDateTime)
                {
                    throw new BadRequestException("Please enter valid date and time range to make a reservation");
                }



                ValidateDateTimeRange(parkingLot, reservationStartDateTime, reservationEndDateTime);

                CheckForExistingReservation(request, user, selectedVehicle, reservationStartDateTime, reservationEndDateTime);

                var madeReservations = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId), selectedVehicle.Type, parkingLot.Id,
                     request.StartDate.Date, reservationStartTime, request.EndDate.Date, reservationEndTime);

                // if users vehicle type is Car, check available car capacity
                if (selectedVehicle.Type.Equals(_enumsExtensionVehicleTypes.GetDisplayName(Enums.VehicleTypes.Car)))
                {
                    var availableCapacity = parkingLot.CapacityCar - madeReservations;
                    if (availableCapacity == 0)
                    {
                        throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                    }
                }

                // if users vehicle type is Adapted Car
                if (selectedVehicle.Type.Equals(_enumsExtensionVehicleTypes.GetDisplayName(Enums.VehicleTypes.AdaptedCar)))
                {
                    var availableAdaptedCapacity = parkingLot.CapacityAdaptedCar - madeReservations;
                    if (availableAdaptedCapacity == 0)
                    {
                        var madeReservationsWithCar = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId),
                        Enums.VehicleTypes.Car.ToString(), parkingLot.Id,
                        request.StartDate, reservationStartTime, request.EndDate, reservationEndTime);

                        var availableCarCapacity = parkingLot.CapacityCar - madeReservationsWithCar;

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

        private void CheckForExistingReservation(MakeReservationRequest request, AspNetUser user,
            Vehicle selectedVehicle, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            List<Reservation> existingReservation = _reservationRepository.GetAsQueryable(x =>
                        x.Type.Equals(Enums.ReservationTypes.Successful.ToString()) &&
                        x.UserId == user.Id &&
                        x.VehicleId == selectedVehicle.Id).ToList();
            foreach (var x in existingReservation)
            {
                DateTime start = x.StartDate.Add(x.StartTime);
                DateTime end = x.EndDate.Add(x.EndTime);

                var res = existingReservation.Where(r => (r.StartDate <= request.EndDate && // Check for date range overlap
                   r.EndDate >= request.StartDate &&
                   start < reservationEndDateTime && // Check for time range overlap
                   end > reservationStartDateTime) || (start == reservationStartDateTime && end == reservationEndDateTime))
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

        private void ValidateDateTimeRange(ParkingLot parkingLot, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            try
            {
                bool isOvernight = false;

                if (parkingLot.WorkingHourTo <= parkingLot.WorkingHourFrom)
                {
                    isOvernight = true;
                }
                //Reservation
                var reservationStartTime = reservationStartDateTime.TimeOfDay;
                var reservationEndTime = reservationEndDateTime.TimeOfDay;

                if (isOvernight)
                {
                    var parkingLotNonWorkingStart = parkingLot.WorkingHourTo;
                    var parkingLotNonWorkingEnd = parkingLot.WorkingHourFrom;

                    // out of valid time range
                    if ((reservationStartTime >= parkingLotNonWorkingStart && reservationStartTime < parkingLotNonWorkingEnd) ||
                        (reservationEndTime > parkingLotNonWorkingStart && reservationEndTime <= parkingLotNonWorkingEnd))
                    {
                        throw new BadRequestException("Non working hours");
                    }
                }
                else
                {
                    var parkingLotWorkingStart = parkingLot.WorkingHourFrom;
                    var parkingLotWorkingEnd = parkingLot.WorkingHourTo;

                    // valid time range
                    if ((reservationStartTime < parkingLotWorkingStart || reservationStartTime >= parkingLotWorkingEnd) ||
                      (reservationEndTime <= parkingLotWorkingStart || reservationEndTime > parkingLotWorkingEnd))
                    {
                        throw new BadRequestException("Non working hours");
                    }
                }
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

        public ReservationResponse CancelReservation(int reservationId)
        {
            try
            {
                var reservation = _reservationRepository.GetAsQueryable(x => x.Id == reservationId).FirstOrDefault();
                if (reservation == null)
                {
                    throw new BadRequestException("Reservation doesn't exist");
                }

                if (reservation.Type.Equals(Enums.ReservationTypes.Cancelled.ToString()))
                {
                    throw new BadRequestException("Reservation is already cancelled");
                }

                DateTime dateTimeNow = DateTime.Now;
                DateTime reservationStartDateTime = reservation.StartDate.Add(reservation.StartTime);
                DateTime reservationEndDateTime = reservation.EndDate.Add(reservation.EndTime);
                TimeSpan timeNow = dateTimeNow.TimeOfDay;
                if (dateTimeNow > reservationEndDateTime)
                {
                    throw new BadRequestException("Can't cancel this reservation, because it has already finished");
                }
                if ((reservationEndDateTime.Date == dateTimeNow.Date && timeNow >= reservation.StartTime) 
                    || (dateTimeNow > reservationStartDateTime))
                {
                    throw new BadRequestException("Can't cancel this reservation, because it has already started");
                }
                reservation.Type = Enums.ReservationTypes.Cancelled.ToString();
                reservation.TimeModified = DateTime.Now;
                _reservationRepository.Update(reservation);
                _unitOfWork.Save();

                var reservationDTO = _mapper.Map<ReservationDTO>(reservation);

                _reservationResponse.StatusCode = HttpStatusCode.OK;
                _reservationResponse.Message = "Reservation cancelled successfully";
                _reservationResponse.Reservation = reservationDTO;
                return _reservationResponse;

            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CancelReservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while cancelling the Reservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while cancelling the Reservation");
            }
        }
    }
}
