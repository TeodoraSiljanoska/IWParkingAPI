using IWParkingAPI.Fluent_Validations;
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
        [Validate]
        public Task<UserRegisterResponse> Register([FromBody] UserRegisterRequest request)
        {
            return _authService.RegisterUser(request);
        }

        [HttpPost("Login")]
        [Validate]
        public Task<UserLoginResponse> Login([FromBody] UserLoginRequest request)
        {
            return _authService.LoginUser(request);
        }

        [HttpPost("ChangePassword")]
        [Validate]
        public Task<ResponseBase> ChangePassword([FromBody] UserResetPasswordRequest request)
        {
            return _authService.ChangePassword(request);
        }

        [HttpPost("ChangeEmail")]
        [Validate]
        public Task<ResponseBase> ChangeEmail([FromBody] UserChangeEmailRequest request)
        {
            return _authService.ChangeEmail(request);
        }

    }
}
