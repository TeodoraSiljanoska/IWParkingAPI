using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

    }
}
