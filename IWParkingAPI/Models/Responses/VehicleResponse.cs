using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class VehicleResponse : ResponseBase
    {
        public Vehicle Vehicle { get; set; } = new Vehicle();
    }
}
