﻿using IWParkingAPI.Fluent_Validations;
using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeCustom(UserRoles.SuperAdmin)]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;


        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("GetAll")]
        public AllRolesResponse GetAll()
        {
            return _roleService.GetAllRoles();
        }

        [HttpGet("Get/{id}")]
        public RoleResponse GetById(int id)
        {
            return _roleService.GetRoleById(id);
        }

        [HttpPost("Create")]
        [Validate]
        public RoleResponse Create(RoleRequest request)
        {
            return _roleService.CreateRole(request);
        }

        [HttpPut("Update/{id}")]
        [Validate]
        public RoleResponse Update(int id, RoleRequest changes)
        {
            return _roleService.UpdateRole(id, changes);
        }

        [HttpDelete("Delete/{id}")]
        public RoleResponse Delete(int id)
        {
            return _roleService.DeleteRole(id);
        }
    }
}
