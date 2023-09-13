﻿using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Enums;
using IWParkingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace IWParkingAPI.Services.Implementation
{
    public class CalculateCapacityExtension : ICalculateCapacityExtension
    {
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IGenericRepository<ParkingLot> _parkingLotRepository;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public CalculateCapacityExtension(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _reservationRepository = _unitOfWork.GetGenericRepository<Reservation>();
            _parkingLotRepository = _unitOfWork.GetGenericRepository<ParkingLot>();
        }
        public int AvailableCapacity(int? id, string? vehicleType, int parkingLotId,
                   DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
        {
            try
            {
                List<Reservation> Reservations = new List<Reservation>();
                double overlap = (endTime - startTime).TotalHours;

                Reservations = CountReservations(vehicleType, parkingLotId, startDate, startTime, endDate, endTime, overlap);
                return Reservations.Count();

            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for AvailableCapacity {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for AvailableCapacity {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting the Available Capacity {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Available Capacity");
            }
        }

        public List<Reservation> CountReservations(string? vehicleType, int parkingLotId, DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, double overlap)
        {
            List<Reservation> Reservations;
            if (overlap < 0)
            {
                Reservations = _reservationRepository.GetAsQueryable(
                x => x.Type.Equals(Enums.ReservationTypes.Successful.ToString()) &&
                x.ParkingLotId == parkingLotId &&
                (
                   (x.StartDate == startDate && x.StartTime < endTime && x.EndTime > startTime) ||
                   (x.StartDate == endDate && x.EndTime > startTime) ||
                   (x.EndDate == startDate && x.StartTime < endTime) ||
                   (x.StartDate < endDate && x.EndDate > startDate)
               ),
               null, x => x.Include(y => y.Vehicle)).Where(x => x.Vehicle.Type.Equals(vehicleType)).ToList();
            }
            else
            {
                ParkingLot parkingLot = _parkingLotRepository.GetAsQueryable(p => p.Id == parkingLotId, null, null).FirstOrDefault();
                if ((parkingLot.WorkingHourTo - parkingLot.WorkingHourFrom).TotalHours < 0)
                {
                    Reservations = _reservationRepository.GetAsQueryable(
                    x => x.Type.Equals(Enums.ReservationTypes.Successful.ToString()) &&
                    x.ParkingLotId == parkingLotId &&
                    (
                      (x.StartDate == startDate && x.StartTime < endTime && x.EndTime > startTime) ||
                      (x.StartDate == endDate && x.EndTime > startTime) ||
                      (x.EndDate == startDate && x.StartTime < endTime) ||
                      (x.StartDate < endDate && x.EndDate > startDate)
                    ),
                    null, x => x.Include(y => y.Vehicle)).Where(x => x.Vehicle.Type.Equals(Enums.VehicleTypes.Car.ToString())).ToList();
                }
                else
                {
                    Reservations = _reservationRepository.GetAsQueryable(
                    x => (x.Type.Equals(Enums.ReservationTypes.Successful.ToString())) &&
                    (x.ParkingLotId == parkingLotId) &&
                    ((x.StartDate.Date <= endDate && x.EndDate.Date >= startDate &&
                    x.StartTime < endTime && x.EndTime > startTime) ||
                    (startTime >= x.StartTime && x.StartTime < endTime && endTime <= x.EndTime && x.EndTime > startTime)))
                    .Where(reservation => reservation.Vehicle.Type.Equals(vehicleType)).ToList();
                }
            }

            return Reservations;
        }

    }
}
