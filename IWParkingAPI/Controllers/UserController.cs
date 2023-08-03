
using IWParkingAPI.Middleware;
using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
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

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpGet("GetAll")]
        public IEnumerable<ApplicationUser> GetUsers()
        {
            return _userService.GetAllUsers();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin, UserRoles.User)]
        [HttpGet("Get/{id}")]
        public UserResponse GetUser(int id)
        {
            return _userService.GetUserById(id);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpPut("Update/{id}")]
        public Task<UserResponse> Update(int id, UserRequest changes)
        {
            return _userService.UpdateUser(id, changes);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpDelete("Deactivate/{id}")]
        public UserResponse Deactivate(int id)
        {
            return _userService.DeactivateUser(id);
        }
    }
}