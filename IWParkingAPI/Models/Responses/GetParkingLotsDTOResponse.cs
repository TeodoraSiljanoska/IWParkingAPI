using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetParkingLotsDTOResponse : ResponseBase
    {
        public IEnumerable<ParkingLotDTO>? ParkingLots { get; set; } = new List<ParkingLotDTO>();
    }
}
