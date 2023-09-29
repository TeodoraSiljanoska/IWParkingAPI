using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class AllZonesResponse : ResponseBase
    {
        public IEnumerable<Zone>? Zones { get; set; } = new List<Zone>();

    }
}
