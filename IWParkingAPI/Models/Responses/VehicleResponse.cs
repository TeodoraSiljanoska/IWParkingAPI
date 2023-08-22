using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class VehicleResponse : ResponseBase
    {
        public VehicleDTO Vehicle { get; set; } = new VehicleDTO();

    }
}
