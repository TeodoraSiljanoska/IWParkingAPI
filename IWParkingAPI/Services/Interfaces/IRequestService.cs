using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IRequestService
    {
        public GetAllParkingLotRequestsResponse GetAllRequests();
        public RequestResponse ModifyRequest(int id, RequestRequest request);
    }
}
