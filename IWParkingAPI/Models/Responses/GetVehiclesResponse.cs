using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetVehiclesResponse : ResponseBase
    {
        public IEnumerable<VehicleDTO>? Vehicles { get; set; } = new List<VehicleDTO>();

    }
}
