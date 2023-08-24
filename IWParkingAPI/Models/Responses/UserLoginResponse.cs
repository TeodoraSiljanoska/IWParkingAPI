namespace IWParkingAPI.Models.Responses
{
    public class UserLoginResponse : ResponseBase
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
