using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetAllParkingLotRequestsResponse : ResponseBase
    {
        public IEnumerable<GetAllRequestsDTO>? Requests { get; set; } = new List<GetAllRequestsDTO>();
    }
}
