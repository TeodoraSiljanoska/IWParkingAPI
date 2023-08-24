namespace IWParkingAPI.Models.Requests
{
    public class FilterParkingLotRequest
    {
        public string? Name { get; set; } 

        public string? City { get; set; } 

        public string? Zone { get; set; } 

        public string? Address { get; set; } 

        public string? WorkingHourFrom { get; set; }

        public string? WorkingHourTo { get; set; }

        public int? CapacityCar { get; set; }

        public int? CapacityAdaptedCar { get; set; }

        public int? Price { get; set; }
    }
}
