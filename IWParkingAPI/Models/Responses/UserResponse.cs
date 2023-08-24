using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class UserResponse : ResponseBase
    {
        public UserDTO User { get; set; } = null!;
    }
}
