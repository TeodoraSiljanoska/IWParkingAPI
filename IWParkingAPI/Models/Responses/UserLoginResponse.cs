using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class UserLoginResponse : ResponseBase
    {
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
