using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetUsersDTOResponse : ResponseBase
    {
        public IEnumerable<UserDataDTO>? Users { get; set; } = new List<UserDataDTO>();
    }
}
