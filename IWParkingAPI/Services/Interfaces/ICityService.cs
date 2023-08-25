using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface ICityService
    {
        public CityResponse CreateCity(CityRequest request);
    }
}
