using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public Task<UserResponse> Register([FromBody] UserRegisterRequest request)
        {
            return _authService.RegisterUser(request);
        }

        [HttpPost("Login")]
        public Task<UserLoginResponse> Login([FromBody] UserLoginRequest request)
        {
            return _authService.LoginUser(request);
        }

        [HttpPost("Reset-Password")]
        public Task<UserResponse> ResetPassword([FromBody] UserResetPasswordRequest request)
        {
            return _authService.ResetPassword(request);
        }

        [HttpPost("Change-Username")]
        public Task<UserResponse> ChangeUsername([FromBody] UserChangeEmailRequest request)
        {
            return _authService.ChangeUsername(request);
        }

    }
}
