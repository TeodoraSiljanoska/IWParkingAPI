using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllVehiclesWithUserResponse : ResponseBase
    {
        public IEnumerable<VehicleWithUserDTO>? Vehicles { get; set; } = new List<VehicleWithUserDTO>();
    }
}
