using System.Text.Json.Serialization;

namespace IWParkingAPI.Models.Requests
{
    public class ParkingLotReq
    {
        public string Name { get; set; } = null!;

        public string City { get; set; } = null!;

        public string Zone { get; set; } = null!;

        public string Address { get; set; } = null!;

        public TimeSpan WorkingHourFrom { get; set; }

        public TimeSpan WorkingHourTo { get; set; }

        public int CapacityCar { get; set; }

        public int CapacityAdaptedCar { get; set; }

        public int Price { get; set; }
        public int UserId { get; set; }

    }
}
