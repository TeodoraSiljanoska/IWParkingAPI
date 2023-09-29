using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllRequestsResponse : ResponseBase
    {
        public IEnumerable<RequestDTO>? Requests { get; set; } = new List<RequestDTO>();
        public int NumPages { get; set; }
    }
}
