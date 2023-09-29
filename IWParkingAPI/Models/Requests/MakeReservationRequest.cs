using System.Text.Json.Serialization;

namespace IWParkingAPI.Models.Requests
{
    public class MakeReservationRequest
    {
        public DateTime StartDate { get; set; }

        public string StartTime { get; set; }

        public DateTime EndDate { get; set; }

        public string EndTime { get; set; }

        public int ParkingLotId { get; set; }
        public string PlateNumber { get; set; }
    }
}
