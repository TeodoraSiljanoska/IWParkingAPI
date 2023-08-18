
using IWParkingAPI.Fluent_Validations;
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
        public GetUsersDTOResponse GetUsers()
        {
            return _userService.GetAllUsers();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin, UserRoles.User)]
        [HttpGet("Get")]
        public UserDTOResponse GetUser()
        {
            return _userService.GetUserById();
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpPut("Update")]
        [Validate]
        public UserDTOResponse Update(UpdateUserRequest changes)
        {
            return _userService.UpdateUser(changes);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpDelete("Deactivate")]
        public UserDTOResponse Deactivate()
        {
            return _userService.DeactivateUser();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpDelete("Deactivate/{id}")]
        public UserDTOResponse DeactivateUserAdmin(int id)
        {
            return _userService.DeactivateUserAdmin(id);
        }
    }
}