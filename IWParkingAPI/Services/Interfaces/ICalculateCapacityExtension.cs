namespace IWParkingAPI.Services.Interfaces
{
    public interface ICalculateCapacityExtension
    {
        public int AvailableCapacity(int? id, string? vehicleType, int parkingLotId,
            DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime);
    }
}
