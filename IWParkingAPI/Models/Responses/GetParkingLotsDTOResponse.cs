using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class GetParkingLotsDTOResponse : ResponseBase
    {
        public IEnumerable<ParkingLotDTO>? ParkingLots { get; set; } = new List<ParkingLotDTO>();
    }
}
