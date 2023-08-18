namespace IWParkingAPI.Models.Responses
{
    public class VehicleWithoutUserDTO
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string Type { get; set; } = null!;
        public DateTime TimeCreated { get; set; }

        public DateTime? TimeModified { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
