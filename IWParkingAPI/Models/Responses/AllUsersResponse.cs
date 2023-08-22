using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllUsersResponse : ResponseBase
    {
        public IEnumerable<UserDTO>? Users { get; set; } = new List<UserDTO>();
    }
}
