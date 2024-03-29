﻿
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
        public AllUsersResponse GetUsers(int pageNumber, int pageSize)
        {
            return _userService.GetAllUsers(pageNumber, pageSize);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin, UserRoles.User, UserRoles.Owner)]
        [HttpGet("Get")]
        public UserResponse GetUser()
        {
            return _userService.GetUserById();
        }

        [AuthorizeCustom(UserRoles.User, UserRoles.SuperAdmin)]
        [HttpPut("Update")]
        [Validate]
        public UserResponse Update(UpdateUserRequest changes)
        {
            return _userService.UpdateUser(changes);
        }

        [AuthorizeCustom(UserRoles.User, UserRoles.Owner)]
        [HttpDelete("Deactivate")]
        public ResponseBase Deactivate()
        {
            return _userService.DeactivateUser();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpDelete("Deactivate/{id}")]
        public ResponseBase DeactivateUserAdmin(int id)
        {
            return _userService.DeactivateUserAdmin(id);
        }
    }
}