using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class UserLoginResponse : ResponseBase
    {
        public string Token { get; set; }
    }
}
