﻿using AutoMapper;
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

                TimeSpan parsedStartTime;
                TimeSpan parsedEndTime;
                if (!TimeSpan.TryParse(request.StartTime, out parsedStartTime) ||
                    !TimeSpan.TryParse(request.EndTime, out parsedEndTime))
                {
                    throw new BadRequestException("Invalid start or end time format");
                }
                DateTime startDateTime = request.StartDate.Add(parsedStartTime);
                DateTime endDateTime = request.EndDate.Add(parsedEndTime);
                //check the condition
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
                var totalWorkingHours = (parkingLot.WorkingHourTo - parkingLot.WorkingHourFrom).TotalHours;
                if (totalWorkingHours < 0)
                {
                    //ex. reservation from 23h - 01h the next day
                    if (!((parsedStartTime >= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo) ||
                        parsedStartTime <= parkingLot.WorkingHourFrom && parsedEndTime <= parkingLot.WorkingHourTo))
                    {
                        throw new BadRequestException("Parking Lot isn't available during those hours");
                    }
                }



                if (parsedStartTime < parkingLot.WorkingHourFrom || parsedEndTime > parkingLot.WorkingHourTo)
                {
                    throw new BadRequestException("Parking Lot isn't available during those hours");
                }

                var madeReservations = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId), selectedVehicle.Type, parkingLot.Id,
                     request.StartDate.Date, parsedStartTime, request.EndDate.Date, parsedEndTime);

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
                    // first check available adapted car capacity
                    var availableAdaptedCapacity = parkingLot.CapacityAdaptedCar - madeReservations;
                    if (availableAdaptedCapacity == 0)
                    {
                        var madeReservationsWithCar = _calculateCapacityExtension.AvailableCapacity(int.Parse(userId),
                                    Enums.VehicleTypes.Car.ToString(), parkingLot.Id,
                                    request.StartDate, parsedStartTime, request.EndDate, parsedEndTime);

                        var availableCarCapacity = parkingLot.CapacityCar - madeReservationsWithCar;

                        // then check available car capacity
                        if (availableCarCapacity == 0)
                        {
                            throw new BadRequestException("Sorry. There aren't any free Parking Spaces on this Parking Lot");
                        }
                    }
                }

                var reservation = _mapper.Map<ReservationDTO>(request);
                reservation.IsPaid = true;
                reservation.Type = Enums.ReservationTypes.Successful.ToString();

                // double totalPriceForHours;
                double totalPrice;
                double totalHours;
                if (request.EndDate != request.StartDate)
                {
                    int totalDays = (request.EndDate - request.StartDate).Days;
                    totalHours = (parsedEndTime - parsedStartTime).TotalHours;
                    if (totalHours < 0)
                    {
                        // Calculate hours until midnight
                        double hoursUntilMidnight = (TimeSpan.FromHours(24) - parsedStartTime).TotalHours;

                        // Calculate hours from midnight to end time
                        double hoursFromMidnight = parsedEndTime.TotalHours;

                        // Calculate the total price for overnight parking
                        double hours = hoursUntilMidnight + hoursFromMidnight; // * parkingLot.Price);
                        totalPrice = hours * totalDays * parkingLot.Price;
                    }
                    else
                    {
                        totalPrice = totalHours * totalDays * parkingLot.Price;
                    }

                    totalPrice = Math.Ceiling(totalPrice);
                    reservation.Amount = (int)totalPrice;
                }
                if (request.EndDate == request.StartDate)
                {
                    totalHours = (parsedEndTime - parsedStartTime).TotalHours;
                    if (totalHours < 0)
                    {
                        // Calculate hours until midnight
                        double hoursUntilMidnight = (TimeSpan.FromHours(24) - parsedStartTime).TotalHours;

                        // Calculate hours from midnight to end time
                        double hoursFromMidnight = parsedEndTime.TotalHours;

                        // Calculate the total price for overnight parking
                        double hours = hoursUntilMidnight + hoursFromMidnight; // * parkingLot.Price);
                        totalPrice = hours * parkingLot.Price;
                    }
                    else
                    {
                        totalPrice = totalHours * parkingLot.Price;
                    }

                    totalPrice = Math.Ceiling(totalPrice);
                    reservation.Amount = (int)totalPrice;
                }

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
