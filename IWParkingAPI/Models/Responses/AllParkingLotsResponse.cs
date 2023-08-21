using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class AllParkingLotsResponse : ResponseBase
    {
        public IEnumerable<ParkingLot>? ParkingLots { get; set; } = new List<ParkingLot>();
    }
}
