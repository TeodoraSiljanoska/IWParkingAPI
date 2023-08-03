namespace IWParkingAPI.Models.Requests
{
    public class UpdateVehicleRequest
    {
        public string PlateNumber { get; set; } = null!;

        public string Type { get; set; } = null!;
    }
}
