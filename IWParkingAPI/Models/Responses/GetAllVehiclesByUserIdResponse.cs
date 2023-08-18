namespace IWParkingAPI.Models.Responses
{
    public class GetAllVehiclesByUserIdResponse : ResponseBase
    {
        public IEnumerable<VehicleWithoutUserDTO>? Vehicles { get; set; } = new List<VehicleWithoutUserDTO>();

    }
}
