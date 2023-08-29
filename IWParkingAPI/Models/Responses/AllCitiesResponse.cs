using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class AllCitiesResponse : ResponseBase
    {
        public IEnumerable<City>? Cities { get; set; } = new List<City>();
    }
}
