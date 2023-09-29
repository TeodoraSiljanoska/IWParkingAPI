using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllVehiclesResponse : ResponseBase
    {
        public IEnumerable<VehicleDTO>? Vehicles { get; set; } = new List<VehicleDTO>();
    }
}
