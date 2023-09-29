namespace IWParkingAPI.Models.Requests
{
    public class UpdateUserRequest
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }
}
