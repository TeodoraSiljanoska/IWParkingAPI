using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWParkingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
        private IGenericRepository<ApplicationRole> _roleRepository;
        private RoleResponse response;
        private readonly IMapper _mapper;


        public RoleController(IUnitOfWork<ParkingDbContextCustom> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = _unitOfWork.GetGenericRepository<ApplicationRole>();
            response = new RoleResponse();
            _mapper = MapperConfig.InitializeAutomapper();
        }

        [HttpGet("GetAll")]
        public IEnumerable<ApplicationRole> GetAll()
        {
            if (_roleRepository.GetAll().Count() == 0)
            {
                response.StatusCode = HttpStatusCode.NoContent;
                response.Errors.Add("Role Repository is empty.");
            }
            return _roleRepository.GetAll();
        }

        [HttpGet("Get/{id}")]
        public RoleResponse GetById(int id)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Role not found");
                return response;
            }
            response.Role = role;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [HttpPost("Create")]
        public RoleResponse Create(RoleRequest request)
        {
           if (_roleRepository.FindByPredicate(role => role.Name == request.Name))
           {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Role with that name already exists.");
                return response;
            }

            ApplicationRole role = _mapper.Map<ApplicationRole>(request);
            role.TimeCreated = DateTime.Now;

            _roleRepository.Insert(role);
            _unitOfWork.Save();

            response.Role = role;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        [HttpPut("Update/{id}")]
        public RoleResponse Update(int id, RoleRequest changes)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Role not found");
                return response;
            }

            role.Name = changes.Name;
            role.NormalizedName = role.Name.ToUpper();
            role.TimeModified = DateTime.Now;

            _roleRepository.Update(role);
            _unitOfWork.Save();

            response.Role = role;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        [HttpDelete("Delete/{id}")]
        public RoleResponse Delete(int id)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("Role not found");
                return response;
            }

            _roleRepository.Delete(role);
            _unitOfWork.Save();

            response.Role = role;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }
    }
}
