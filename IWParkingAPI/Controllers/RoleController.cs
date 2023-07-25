using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
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


        public RoleController(IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = _unitOfWork.GetGenericRepository<AspNetRole>();
            response = new RoleResponse();
        }

        [HttpGet("getAll")]
        public IEnumerable<AspNetRole> Index()
        {
            if (_roleRepository.GetAll().Count() == 0)
            {
                response.StatusCode = HttpStatusCode.NoContent;
                response.Errors.Add("Role Repository is empty.");
            }
            return _roleRepository.GetAll();
        }

        [HttpGet("get/{id}")]
        public RoleResponse GetRole(int id)
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

        [HttpPost("create")]
        public RoleResponse Create(RoleRequest request)
        {
            var mapper = MapperConfig.InitializeAutomapper();
            var role = mapper.Map<AspNetRole>(request);

            _roleRepository.Insert(role);
            _unitOfWork.Save();

            response.Role = role;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        [HttpDelete("delete/{id}")]
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
