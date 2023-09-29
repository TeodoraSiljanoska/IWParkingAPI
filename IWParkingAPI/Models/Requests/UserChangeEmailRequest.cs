namespace IWParkingAPI.Models.Requests
{
    public class UserChangeEmailRequest
    {
        public string? OldEmail { get; set; }
        public string? NewEmail { get; set; }
    }
}
