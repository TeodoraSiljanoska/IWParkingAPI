using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetUsersDTOResponse : ResponseBase
    {
        public IEnumerable<UserDTO>? Users { get; set; } = new List<UserDTO>();
    }
}
