namespace IWParkingAPI.Models.Responses
{
    public class VehicleTypesResponse : ResponseBase
    {
        public IEnumerable<string>? VehicleTypes { get; set; } = new List<string>();
    }
}
