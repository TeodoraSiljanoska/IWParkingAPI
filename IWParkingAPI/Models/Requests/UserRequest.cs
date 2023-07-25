namespace IWParkingAPI.Models.Requests
{
    public class UserRequest
    {
        public string? UserName { get; set; }

        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
