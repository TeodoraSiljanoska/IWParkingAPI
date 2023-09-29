namespace IWParkingAPI.Models.Requests
{
    public class UserResetPasswordRequest
    {
        public string? Email { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}
