using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class CityResponse : ResponseBase
    {
        public City City { get; set; } = new City();
    }
}
