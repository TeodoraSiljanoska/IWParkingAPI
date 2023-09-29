using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models;
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
        public List<Reservation> CountReservations(string? vehicleType, int parkingLotId,
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, double overlap)
        {
            List<Reservation> Reservations = new List<Reservation>();
            var existingReservations = _reservationRepository.GetAsQueryable(
            x => (x.Type.Equals(Enums.ReservationTypes.Successful.ToString())) &&
            (x.ParkingLotId == parkingLotId),
            null, x => x.Include(y => y.Vehicle))
            .Where(x => x.Vehicle.Type.Equals(vehicleType)).ToList();
            DateTime startDateTime = startDate.Date.Add(startTime);
            DateTime endDateTime = endDate.Date.Add(endTime);

            foreach (var x in existingReservations)
            {
                DateTime reservationStartDateTime = x.StartDate.Add(x.StartTime);
                DateTime reservationEndDateTime = x.EndDate.Add(x.EndTime);

                var res = existingReservations.Where(r => (r.StartDate <= endDate &&
                   r.EndDate >= startDate &&
                   reservationStartDateTime < endDateTime &&
                   reservationEndDateTime > startDateTime) ||
                   (reservationStartDateTime == startDateTime && reservationEndDateTime == endDateTime)).FirstOrDefault();
                if (res != null)
                    Reservations.Add(res);
            }
            return Reservations;
        }
    }
}
