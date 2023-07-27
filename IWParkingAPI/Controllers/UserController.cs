using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
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

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger, IUserService userService)
        {
            _userManager = userManager;
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
    }
}