using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWParkingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
        private readonly IRoleService _roleService;


        public RoleController(IUnitOfWork<ParkingDbContextCustom> unitOfWork, IRoleService roleService)
        {
            _unitOfWork = unitOfWork;
            _roleService = roleService;
        }

        [HttpGet("GetAll")]
        public IEnumerable<ApplicationRole> GetAll()
        {
            return _roleService.GetAllRoles();
        }

        [HttpGet("Get/{id}")]
        public RoleResponse GetById(int id)
        {
            return _roleService.GetRoleById(id);
        }

        [HttpPost("Create")]
        public RoleResponse Create(RoleRequest request)
        {
            return _roleService.CreateRole(request);
        }

        [HttpPut("Update/{id}")]
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
