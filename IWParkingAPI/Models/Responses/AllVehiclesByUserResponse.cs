using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllVehiclesByUserResponse : ResponseBase
    {
        public IEnumerable<VehicleWithoutUserDTO>? Vehicles { get; set; } = new List<VehicleWithoutUserDTO>();

    }
}
