using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class ZoneResponse : ResponseBase
    {
        public Zone Zone { get; set; } = new Zone();
    }
}
