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
            _reservationResponse = new ReservationResponse();
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

                var parkingLot = _parkingLotRepository.GetAsQueryable(x => x.Id == request.ParkingLotId && 
                x.IsDeactivated == false, null, null).FirstOrDefault();

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
                        request.StartDate, reservationStartTime, request.EndDate, reservationEndTime);

                        var availableCarCapacity = parkingLot.CapacityCar - madeReservations;

                        if (availableCarCapacity == 0)
                        {
                            throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                        }
                    }
                }

                InsertReservation(request, userId, selectedVehicle, parkingLot, reservationStartDateTime, reservationEndDateTime);
                return _reservationResponse;
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

        public ReservationResponse ExtendReservation(int reservationId, ExtendReservationRequest request)
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                var reservation = _reservationRepository.GetAsQueryable(x => x.Id == reservationId && x.UserId == userId
                && x.Type == Enums.ReservationTypes.Successful.ToString()).FirstOrDefault();
                if (reservation == null)
                {
                    throw new NotFoundException("Reservation doesn't exist");
                }
                var parkingLot = _parkingLotRepository.GetAsQueryable(x => x.Id == reservation.ParkingLotId &&
                x.IsDeactivated == false, null, null).FirstOrDefault();

                TimeSpan reservationExtendedEndTime;
                if (!TimeSpan.TryParse(request.EndTime, out reservationExtendedEndTime))
                {
                    throw new BadRequestException("Invalid start or end time format");
                }

                DateTime reservationStartDateTime = reservation.StartDate.Add(reservation.StartTime);
                DateTime reservationEndDateTime = reservation.EndDate.Add(reservation.EndTime);
                DateTime reservationExtendedEndDateTime = request.EndDate.Add(reservationExtendedEndTime);

                if (reservationEndDateTime < DateTime.Now)
                {
                    throw new BadRequestException("Can't extend this reservation, because it has already finished");
                }
                else
                {
                    if (reservationExtendedEndDateTime <= reservationStartDateTime || 
                        reservationExtendedEndDateTime == reservationEndDateTime || reservationExtendedEndDateTime < reservationEndDateTime)
                    {
                        throw new BadRequestException("Please enter valid date and time range to extend the reservation");
                    }

                    ValidateDateTimeRange(parkingLot, reservationStartDateTime, reservationExtendedEndDateTime);
                }

                reservation.Type = Enums.ReservationTypes.Successful.ToString();
                reservation.EndTime = reservationExtendedEndTime;
                reservation.EndDate = request.EndDate.Date;
                reservation.TimeModified = DateTime.Now;
                _reservationRepository.Update(reservation);
                _unitOfWork.Save();

                var reservationDTO = _mapper.Map<ReservationDTO>(reservation);
                reservationDTO.StartDate = reservation.StartDate;
                reservationDTO.StartTime = reservation.StartTime;

                _reservationResponse.StatusCode = HttpStatusCode.OK;
                _reservationResponse.Message = "Reservation extended successfully";
                _reservationResponse.Reservation = reservationDTO;
                return _reservationResponse;

            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for ExtendReservation {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for ExtendReservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while extending the Reservation {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while extending the Reservation");
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

            _reservationResponse.StatusCode = HttpStatusCode.OK;
            _reservationResponse.Message = "Reservation made successfully";
            _reservationResponse.Reservation = _mapper.Map<ReservationDTO>(reservationToInsert);
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
                _logger.Error($"Bad Request for ValidateDateTimeRange {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while validating the DateTime range {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while validating the DateTime range");
            }

        }
    }
}
