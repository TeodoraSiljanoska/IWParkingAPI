using IWParkingAPI.DTOs;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IJwtUtils _jwtUtils;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        

        public UserController(IJwtUtils jwtUtils, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<UserController> logger, IUserService userService)
        {
            _jwtUtils = jwtUtils;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("GetAll")]
        public IEnumerable<ApplicationUser> GetUsers()
        {
            return _userService.GetAllUsers();
        }

        [HttpGet("Get/{id}")]
        public UserResponse GetUser(int id)
        {
            return _userService.GetUserById(id);
        }

        [HttpPost("Create")]
        public UserResponse Create(UserRequest request)
        {
            return _userService.CreateUser(request);
        }

        [HttpPut("Update/{id}")]
        public UserResponse Update(int id, UserRequest changes)
        {
            return _userService.UpdateUser(id, changes);
        }

        [HttpDelete("Delete/{id}")]
        public UserResponse Delete(int id)
        {
            return _userService.DeleteUser(id);
        }


        [HttpPost, Route("Login")]

        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDTO.UserName) || string.IsNullOrEmpty(loginDTO.Password))
                    return BadRequest("Username and/or Password not specified");


                var user = await _userManager.FindByEmailAsync(loginDTO.UserName);
                if (user == null) return NotFound();
                if (user != null)
                {
                        // Verify the user's password
                        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

                        var jwt = _jwtUtils.GenerateToken(user.UserName);
                    Response.Cookies.Append("jwt", jwt, new CookieOptions { HttpOnly = true });

                    return Ok(new { token = jwt });

                }
            }
            catch (Exception ex)
            {
                return null;
                //("An error occurred in generating the token");
            }
            return Unauthorized();
        }
    }
}