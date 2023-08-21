using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class VehicleDTOResponse : ResponseBase
    {
        public VehicleDTO Vehicle { get; set; } = new VehicleDTO();
    }
}
