namespace IWParkingAPI.Models.Requests
{
    public class UserRegisterRequest
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; } 
        public string? Phone { get; set; }
        public string? Role { get; set;}
    }
}
