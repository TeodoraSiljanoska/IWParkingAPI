using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface ICityService
    {
        public AllCitiesResponse GetAllCities();
        public CityResponse GetCityById(int id);
        public CityResponse CreateCity(CityRequest request);
        public CityResponse UpdateCity(int id, CityRequest changes);
        public CityResponse DeleteCity(int id);
    }
}
