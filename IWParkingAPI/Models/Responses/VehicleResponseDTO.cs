using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Models.Responses.Dto
{
    public class VehicleResponseDTO : ResponseBase
    {
        public VehicleDTO Vehicle { get; set; } = new VehicleDTO();

    }
}
