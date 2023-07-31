namespace IWParkingAPI.Models.Requests
{
    public class UserResetPasswordRequest
    {
        public string? Username { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}
