using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetVehiclesResponse : ResponseBase
    {
        public IEnumerable<Vehicle>? Vehicles { get; set; } = new List<Vehicle>();

    }
}
