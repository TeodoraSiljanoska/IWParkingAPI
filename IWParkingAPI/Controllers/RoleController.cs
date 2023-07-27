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
        private IUnitOfWork<ParkingDbContext> _unitOfWork;
        private IGenericRepository<AspNetRole> _roleRepository;
        private RoleResponse response;
        private readonly IMapper _mapper;


        public RoleController(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = _unitOfWork.GetGenericRepository<AspNetRole>();
            response = new RoleResponse();
            _mapper = MapperConfig.InitializeAutomapper();
        }

        [HttpGet("GetAll")]
        public IEnumerable<AspNetRole> GetAll()
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
            AspNetRole role = _roleRepository.GetById(id);
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

            AspNetRole role = _mapper.Map<AspNetRole>(request);
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
            AspNetRole role = _roleRepository.GetById(id);
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
            AspNetRole role = _roleRepository.GetById(id);
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
