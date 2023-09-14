using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Services.Interfaces
{
    public interface ICalculateCapacityExtension
    {
        public int AvailableCapacity(int? id, string? vehicleType, int parkingLotId,
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime);
        public List<Reservation> CountReservations(string? vehicleType, int parkingLotId, DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime, double overlap);
    }
}
