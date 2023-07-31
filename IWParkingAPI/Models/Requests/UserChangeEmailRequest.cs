namespace IWParkingAPI.Models.Requests
{
    public class UserChangeEmailRequest
    {
        public string? OldUsername { get; set; }
        public string? NewUsername { get; set; }
        
    }
}
