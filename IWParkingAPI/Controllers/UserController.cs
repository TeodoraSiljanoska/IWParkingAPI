using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly UserResponse response;

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger, IUserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _userService = userService;
            response = new UserResponse();
        }

        [HttpGet("GetAll")]
        public IEnumerable<ApplicationUser> GetUsers()
        {
            var users = _userService.GetAllUsers();
            return users;
        }

        [HttpGet("Get/{id}")]
        public UserResponse GetUser(int id)
        {
            var userResponse = _userService.GetUserById(id);
            
            return userResponse;
        }

        [HttpPost("Create")]
        public UserResponse Create(UserRequest request)
        {
            var userResponse = _userService.CreateUser(request);
            return userResponse;
        }

        [HttpPut("Update/{id}")]
        public UserResponse Update(int id, UserRequest changes)
        {
            var userResponse = _userService.UpdateUser(id, changes);
            return userResponse;
        }

        [HttpDelete("Delete/{id}")]
        public UserResponse Delete(int id)
        {
            var userResponse = _userService.DeleteUser(id);
            
            return userResponse;
        }
    }
}