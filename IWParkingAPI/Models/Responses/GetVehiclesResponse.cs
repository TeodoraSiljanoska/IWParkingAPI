using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class GetVehiclesResponse : ResponseBase
    {
        public IEnumerable<VehicleDTO>? Vehicles { get; set; } = new List<VehicleDTO>();

    }
}
