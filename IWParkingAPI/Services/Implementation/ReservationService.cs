﻿using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
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
        private readonly AllReservationsResponse _allReservationsResponse;
        private readonly IEnumsExtension<Enums.VehicleTypes> _enumsExtensionVehicleTypes;
        private readonly ILocalTimeExtension _localTime;
        private const int PageSize = 5;
        private const int PageNumber = 1;

        public ReservationService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode,
            ICalculateCapacityExtension calculateCapacityExtension, IEnumsExtension<Enums.VehicleTypes> enumsExtension,
            ILocalTimeExtension localTime)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _unitOfWork = unitOfWork;
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
            _reservationRepository = _unitOfWork.GetGenericRepository<Reservation>();
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _jWTDecode = jWTDecode;
            _calculateCapacityExtension = calculateCapacityExtension;
            _reservationResponse = new ReservationResponse();
            _allReservationsResponse = new AllReservationsResponse();
            _enumsExtensionVehicleTypes = enumsExtension;
            _localTime = localTime;
        }

        public AllReservationsResponse GetUserReservations(int pageNumber, int pageSize)
        {
            try
            {
                var userId = _jWTDecode.ExtractClaimByType("Id");
                if (userId == null)
                {
                    throw new BadRequestException("Please login to make a reservation");
                }

                var reservations = _reservationRepository.GetAsQueryable(
                x => x.UserId == int.Parse(userId),
                orderBy: q => q.OrderBy(x => x),
                include: x => x.Include(y => y.ParkingLot).Include(y => y.Vehicle),
                orderProperty: x => x.TimeCreated,
                isDescending: true);

                int totalPages;
                List<Reservation> paginatedReservations;
                PaginateReservations(ref pageNumber, ref pageSize, reservations, out totalPages, out paginatedReservations);

                if (paginatedReservations.Count() == 0)
                {
                    _allReservationsResponse.StatusCode = HttpStatusCode.OK;
                    _allReservationsResponse.Message = "There aren't any reservations.";
                    _allReservationsResponse.Reservations = Enumerable.Empty<ReservationWithParkingLotDTO>();
                    return _allReservationsResponse;
                }

                List<ReservationWithParkingLotDTO> resDTOs = new List<ReservationWithParkingLotDTO>();
                foreach (var res in paginatedReservations)
                {
                    ReservationWithParkingLotDTO reservationDTO = _mapper.Map<ReservationWithParkingLotDTO>(res);
                    reservationDTO.ParkingLot = _mapper.Map<ParkingLotDTO>(res.ParkingLot);
                    reservationDTO.Vehicle = _mapper.Map<VehicleDTO>(res.Vehicle);
                    resDTOs.Add(reservationDTO);
                }

                _allReservationsResponse.Reservations = resDTOs;
                _allReservationsResponse.StatusCode = HttpStatusCode.OK;
                _allReservationsResponse.Message = "User reservations returned successfully";
                _allReservationsResponse.NumPages = totalPages;
                return _allReservationsResponse;
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

        public ReservationResponse MakeReservation(MakeReservationRequest request)
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                var user = _userRepository.GetAsQueryable(x => x.Id == userId,
                    null, x => x.Include(y => y.Vehicles)).FirstOrDefault();
                if (user == null || user.IsDeactivated == true)
                {
                    throw new BadRequestException("User not found");
                }

                Vehicle? selectedVehicle = CheckIfVehicleIsValid(request, user);
                ParkingLot? parkingLot = CheckIfParkingLotIsValid(request);

                TimeSpan reservationStartTime;
                TimeSpan reservationEndTime;
                TimeSpan.TryParse(request.StartTime, out reservationStartTime);
                TimeSpan.TryParse(request.EndTime, out reservationEndTime);

                DateTime date = _localTime.GetLocalTime();

                DateTime reservationStartDateTime = request.StartDate.Date.Add(reservationStartTime);
                DateTime reservationEndDateTime = request.EndDate.Date.Add(reservationEndTime);
                if (reservationStartDateTime <= date || reservationEndDateTime <= date ||
                   reservationEndDateTime <= reservationStartDateTime)
                {
                    throw new BadRequestException("Please enter valid date and time range to make a reservation");
                }

                ValidateDateTimeRange(parkingLot, reservationStartDateTime, reservationEndDateTime);

                CheckForExistingReservation(request, user, selectedVehicle, reservationStartDateTime, reservationEndDateTime);

                var madeReservations = _calculateCapacityExtension.AvailableCapacity(userId, selectedVehicle.Type, parkingLot.Id,
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
                        var madeReservationsWithCar = _calculateCapacityExtension.AvailableCapacity(userId,
                        Enums.VehicleTypes.Car.ToString(), parkingLot.Id,
                        request.StartDate, reservationStartTime, request.EndDate, reservationEndTime);

                        var availableCarCapacity = parkingLot.CapacityCar - madeReservationsWithCar;

                        if (availableCarCapacity == 0)
                        {
                            throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                        }
                    }
                }

                InsertReservation(request, userId.ToString(), selectedVehicle, parkingLot, reservationStartDateTime, reservationEndDateTime);
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
                && x.Type == Enums.ReservationTypes.Successful.ToString(), null, x => x.Include(y => y.Vehicle)).FirstOrDefault();
                if (reservation == null)
                {
                    throw new NotFoundException("Reservation doesn't exist");
                }
                var parkingLot = _parkingLotRepository.GetAsQueryable(x => x.Id == reservation.ParkingLotId &&
                x.IsDeactivated == false, null, null).FirstOrDefault();

                TimeSpan reservationExtendedEndTime;
                TimeSpan.TryParse(request.EndTime, out reservationExtendedEndTime);

                DateTime reservationStartDateTime = reservation.StartDate.Add(reservation.StartTime);
                DateTime reservationEndDateTime = reservation.EndDate.Add(reservation.EndTime);
                DateTime reservationExtendedEndDateTime = request.EndDate.Add(reservationExtendedEndTime);

                DateTime date = _localTime.GetLocalTime();

                if (reservationEndDateTime < date)
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
                    CheckForExistingReservationForExtend(reservation, reservationExtendedEndDateTime);
                }

                UpdateReservation(request, reservation, parkingLot, reservationExtendedEndTime, reservationStartDateTime);
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

        public ReservationResponse CancelReservation(int reservationId)
        {
            try
            {
                var userId = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));
                var reservation = _reservationRepository.GetAsQueryable(x => x.Id == reservationId && x.UserId == userId,
                    null, x => x.Include(y => y.ParkingLot).Include(y => y.Vehicle)).FirstOrDefault();
                if (reservation == null)
                {
                    throw new BadRequestException("Reservation doesn't exist");
                }

                if (reservation.Type.Equals(Enums.ReservationTypes.Cancelled.ToString()))
                {
                    throw new BadRequestException("Reservation is already cancelled");
                }

                DateTime date = _localTime.GetLocalTime();
                DateTime dateTimeNow = date;
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
                reservation.TimeModified = date;
                _reservationRepository.Update(reservation);
                _unitOfWork.Save();

                var reservationDTO = _mapper.Map<ReservationWithParkingLotDTO>(reservation);
                reservationDTO.Vehicle = _mapper.Map<VehicleDTO>(reservation.Vehicle);
                reservationDTO.ParkingLot = _mapper.Map<ParkingLotDTO>(reservation.ParkingLot);
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

        private ParkingLot CheckIfParkingLotIsValid(MakeReservationRequest request)
        {
            try
            {
                var parkingLot = _parkingLotRepository.GetAsQueryable(x => x.Id == request.ParkingLotId
                                     && x.IsDeactivated == false, null, null).FirstOrDefault();
                if (parkingLot == null)
                {
                    throw new BadRequestException("Please select a valid Parking Lot to make a reservation");
                }

                return parkingLot;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfParkingLotIsValid {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Parking Lot is valid in CheckIfParkingLotIsValid method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Parking Lot is valid in CheckIfParkingLotIsValid method");
            }


        }

        public static Vehicle CheckIfVehicleIsValid(MakeReservationRequest request, AspNetUser? user)
        {
            try
            {
                var selectedVehicle = user.Vehicles.Where(x => x.PlateNumber == request.PlateNumber).FirstOrDefault();
                if (selectedVehicle == null)
                {
                    throw new BadRequestException("Please select a valid vehicle to make a reservation");
                }

                return selectedVehicle;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfVehicleIsValid {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if the Vehicle is valid in CheckIfVehicleIsValid method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Vehicle is valid in CheckIfVehicleIsValid method");
            }

        }

        private static void PaginateReservations(ref int pageNumber, ref int pageSize, IQueryable<Reservation> reservations, out int totalPages, out List<Reservation> paginatedReservations)
        {
            if (pageNumber == 0)
            {
                pageNumber = PageNumber;
            }
            if (pageSize == 0)
            {
                pageSize = PageSize;
            }

            var totalCount = reservations.Count();
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            paginatedReservations = reservations.Skip((pageNumber - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .ToList();
        }

        private void UpdateReservation(ExtendReservationRequest request, Reservation? reservation, ParkingLot? parkingLot, TimeSpan reservationExtendedEndTime, DateTime reservationStartDateTime)
        {
            reservation.Type = Enums.ReservationTypes.Successful.ToString();
            reservation.EndTime = reservationExtendedEndTime;
            reservation.EndDate = request.EndDate.Date;
            double totalPrice = CalculatePrice(parkingLot, reservationStartDateTime, reservation.EndDate.Add(reservation.EndTime));
            totalPrice = Math.Ceiling(totalPrice);
            reservation.Amount = (int)totalPrice;

            DateTime date = _localTime.GetLocalTime();

            reservation.TimeModified = date;
            _reservationRepository.Update(reservation);
            _unitOfWork.Save();

            var reservationDTO = _mapper.Map<ReservationWithParkingLotDTO>(reservation);
            reservationDTO.StartDate = reservation.StartDate;
            reservationDTO.StartTime = reservation.StartTime;
            reservationDTO.ParkingLot = _mapper.Map<ParkingLotDTO>(parkingLot);
            reservationDTO.Vehicle = _mapper.Map<VehicleDTO>(reservation.Vehicle);

            _reservationResponse.StatusCode = HttpStatusCode.OK;
            _reservationResponse.Message = "Reservation extended successfully";
            _reservationResponse.Reservation = reservationDTO;
        }

        private void InsertReservation(MakeReservationRequest request, string userId, Vehicle selectedVehicle, ParkingLot parkingLot, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            var reservation = _mapper.Map<ReservationDTO>(request);
            reservation.StartDate = reservationStartDateTime.Date;
            reservation.IsPaid = true;
            reservation.Type = Enums.ReservationTypes.Successful.ToString();
            double totalPrice = CalculatePrice(parkingLot, reservationStartDateTime, reservationEndDateTime);
            totalPrice = Math.Ceiling(totalPrice);
            reservation.Amount = (int)totalPrice;
            reservation.UserId = int.Parse(userId);
            reservation.ParkingLotId = parkingLot.Id;
            reservation.VehicleId = selectedVehicle.Id;

            DateTime date = _localTime.GetLocalTime();

            reservation.TimeCreated = date;

            var reservationToInsert = _mapper.Map<Reservation>(reservation);
            _reservationRepository.Insert(reservationToInsert);
            _unitOfWork.Save();

            var resToReturn = _mapper.Map<ReservationWithParkingLotDTO>(reservationToInsert);
            resToReturn.Vehicle = _mapper.Map<VehicleDTO>(selectedVehicle);
            resToReturn.ParkingLot = _mapper.Map<ParkingLotDTO>(parkingLot);

            _reservationResponse.StatusCode = HttpStatusCode.OK;
            _reservationResponse.Message = "Reservation made successfully";
            _reservationResponse.Reservation = resToReturn;
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

        private void CheckForExistingReservationForExtend(Reservation reservation, DateTime reservationExtendedEndDateTime)
        {
            DateTime reservationStartDateTime = reservation.StartDate.Date.Add(reservation.StartTime);

            List<Reservation> existingReservation = _reservationRepository.GetAsQueryable(x =>
                        x.Type.Equals(Enums.ReservationTypes.Successful.ToString()) &&
                        x.UserId == reservation.UserId &&
                        x.VehicleId == reservation.VehicleId && x.Id != reservation.Id).ToList();
            foreach (var x in existingReservation)
            {
                DateTime start = x.StartDate.Add(x.StartTime);
                DateTime end = x.EndDate.Add(x.EndTime);

                var res = existingReservation.Where(r => (r.StartDate <= reservationExtendedEndDateTime.Date && // Check for date range overlap
                   r.EndDate >= reservation.StartDate &&
                   start < reservationExtendedEndDateTime && // Check for time range overlap
                   end > reservationStartDateTime) || (start == reservationStartDateTime && end == reservationExtendedEndDateTime))
                   .FirstOrDefault();

                if (res != null)
                {
                    throw new BadRequestException("A reservation with the same vehicle and overlapping time range already exists.");
                }
            }
        }

        private double CalculatePrice(ParkingLot parkingLot, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
        {
            bool isOvernight = false;
            TimeSpan parkingLotWorkingHoursStart = parkingLot.WorkingHourFrom;
            TimeSpan parkingLotWorkingHoursEnd = parkingLot.WorkingHourTo;
            DateTime reservationStartDate = reservationStartDateTime.Date;
            DateTime reservationEndDate = reservationEndDateTime.Date;
            TimeSpan reservationStartTime = reservationStartDateTime.TimeOfDay;
            TimeSpan reservationEndTime = reservationEndDateTime.TimeOfDay;
            DateTime currentDate = reservationStartDate;
            TimeSpan totalDurationWithinWorkingHours = TimeSpan.Zero;
            double priceForNonOvernight = 0;
            double priceForOvernight = 0;
            double priceToReturn = 0;

            if (parkingLotWorkingHoursEnd < parkingLotWorkingHoursStart)
            {
                isOvernight = true;
            }

            if (isOvernight)
            {
                TimeSpan startOfDay = new TimeSpan(0, 0, 0, 0);
                TimeSpan endOfDay = new TimeSpan(0, 23, 59, 59);
                DateTime currentDateTime = reservationStartDateTime;
                int totalNonWorkingHours = 0;
                double duration = (reservationEndDateTime - reservationStartDateTime).TotalHours;
                while (currentDateTime <= reservationEndDateTime)
                {
                    if (!((currentDateTime.TimeOfDay >= startOfDay && currentDateTime.TimeOfDay <= parkingLotWorkingHoursEnd) ||
                        (currentDateTime.TimeOfDay >= parkingLotWorkingHoursStart && currentDateTime.TimeOfDay <= endOfDay))
                        && currentDateTime.TimeOfDay != parkingLotWorkingHoursEnd)
                        totalNonWorkingHours++;
                    currentDateTime = currentDateTime.AddHours(1);
                }
                duration = duration - totalNonWorkingHours;
                priceForOvernight = duration * parkingLot.Price;
                priceToReturn = priceForOvernight;
            }
            else
            {
                while (currentDate <= reservationEndDate)
                {
                    TimeSpan reservationWorkingHoursStart =
                        (currentDate == reservationStartDate && reservationStartTime > parkingLotWorkingHoursStart) ?
                        reservationStartTime : parkingLotWorkingHoursStart;

                    TimeSpan reservationWorkingHoursEnd =
                        (currentDate == reservationEndDate && reservationEndTime < parkingLotWorkingHoursEnd) ?
                        reservationEndTime : parkingLotWorkingHoursEnd;

                    TimeSpan durationWithinWorkingHours = reservationWorkingHoursEnd - reservationWorkingHoursStart;

                    if (durationWithinWorkingHours > TimeSpan.Zero)
                    {
                        totalDurationWithinWorkingHours += durationWithinWorkingHours;
                    }
                    currentDate = currentDate.AddDays(1);
                }
                priceForNonOvernight = parkingLot.Price * totalDurationWithinWorkingHours.TotalHours;
                priceToReturn = priceForNonOvernight;
            }

            return priceToReturn;
        }

        public void ValidateDateTimeRange(ParkingLot parkingLot, DateTime reservationStartDateTime, DateTime reservationEndDateTime)
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
