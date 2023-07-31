using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;


        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
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

        [HttpPut("Update/{id}")]
        public Task<UserResponse> Update(int id, UserRequest changes)
        {
            return _userService.UpdateUser(id, changes);
        }

        [HttpDelete("Deactivate/{id}")]
        public UserResponse Deactivate(int id)
        {
            return _userService.DeactivateUser(id);
        }
    }
}