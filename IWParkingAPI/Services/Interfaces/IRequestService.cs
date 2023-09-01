using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IRequestService
    {
        public AllRequestsResponse GetAllRequests(int pageNumber, int pageSize);
        public RequestResponse ModifyRequest(int id, RequestRequest request);
    }
}
