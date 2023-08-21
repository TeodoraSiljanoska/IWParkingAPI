using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Models.Responses.Dto
{
    public class GetParkingLotsDTOResponse : ResponseBase
    {
        public IEnumerable<ParkingLotDTO>? ParkingLots { get; set; } = new List<ParkingLotDTO>();
    }
}
