namespace IWParkingAPI.Models.Requests
{
    public class FilterParkingLotRequest
    {
        public string? Name { get; set; } 

        public string? City { get; set; } 

        public string? Zone { get; set; } 

        public string? Address { get; set; } 

        public int? CapacityCar { get; set; }

        public int? CapacityAdaptedCar { get; set; }
        public string? Status { get; set; }

    }
}
