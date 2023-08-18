namespace IWParkingAPI.Models.Responses
{
    public class UserDataDTO
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
