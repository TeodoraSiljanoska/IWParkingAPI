using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllParkingLotsResponse : ResponseBase
    {
        public IEnumerable<ParkingLotDTO>? ParkingLots { get; set; } = new List<ParkingLotDTO>();
    }
}
