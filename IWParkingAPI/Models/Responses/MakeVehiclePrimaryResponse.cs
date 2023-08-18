namespace IWParkingAPI.Models.Responses
{
    public class MakeVehiclePrimaryResponse : ResponseBase
    {
        public VehicleWithoutUserDTO Vehicle { get; set; } = new VehicleWithoutUserDTO();

    }
}
