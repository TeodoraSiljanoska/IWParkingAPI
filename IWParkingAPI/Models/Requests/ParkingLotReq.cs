namespace IWParkingAPI.Models.Requests
{
    public class ParkingLotReq
    {
        public string Name { get; set; } = null!;

        public string City { get; set; } = null!;

        public string Zone { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string WorkingHourFrom { get; set; }

        public string WorkingHourTo { get; set; }

        public int CapacityCar { get; set; }

        public int CapacityAdaptedCar { get; set; }

        public int Price { get; set; }

    }
}
