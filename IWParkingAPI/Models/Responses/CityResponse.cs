using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class CityResponse : ResponseBase
    {
        public CityDTO City { get; set; } = new CityDTO();
    }
}
