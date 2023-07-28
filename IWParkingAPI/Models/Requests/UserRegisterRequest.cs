namespace IWParkingAPI.Models.Requests
{
    public class UserRegisterRequest
    {
            public string? UserName { get; set; }
            public string Name { get; set; } = null!;
            public string Surname { get; set; } = null!;
            public string? Email { get; set; }
            public string? Password { get; set; }
            public string? PhoneNumber { get; set; }
            public string? RoleName { get; set; }
        
    }
}
