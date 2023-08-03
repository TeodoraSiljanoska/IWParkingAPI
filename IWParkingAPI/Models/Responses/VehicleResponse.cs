using IWParkingAPI.Models.Data;
using System.Text.Json.Serialization;

namespace IWParkingAPI.Models.Responses
{
    public class VehicleResponse : ResponseBase
    {
        public Vehicle Vehicle { get; set; } = new Vehicle();

    }
}
