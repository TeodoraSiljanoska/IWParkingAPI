namespace IWParkingAPI.Models.Responses.Dto
{
    public class ReservationWithParkingLotDTO
    {
        public string Type { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan EndTime { get; set; }

        public int Amount { get; set; }

        public bool IsPaid { get; set; }

        public int UserId { get; set; }

        public int ParkingLotId { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime? TimeModified { get; set; }

        public int VehicleId { get; set; }
        public ParkingLotDTO ParkingLot { get; set; }
        public VehicleDTO Vehicle { get; set; }
    }
}
