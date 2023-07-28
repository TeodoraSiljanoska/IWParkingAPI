using AutoMapper;
using Azure.Core;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
        private readonly IGenericRepository<ApplicationRole> _roleRepository;
        private readonly RoleResponse _response;

        public RoleService(IUnitOfWork<ParkingDbContextCustom> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = _unitOfWork.GetGenericRepository<ApplicationRole>();
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new RoleResponse();
        }
        public IEnumerable<ApplicationRole> GetAllRoles()
        {
            var roles = _roleRepository.GetAll();
            if (roles.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.Message = "There aren't any roles.";
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Roles returned successfully";
            return roles;
        }
        public RoleResponse GetRoleById(int id)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Role not found";
                return _response;
            }
            _response.Role = role;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Role returned successfully";
            return _response;
        }

        public RoleResponse CreateRole(RoleRequest request)
        {
            if (_roleRepository.FindByPredicate(u => u.Name == request.Name))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Role already exists.";
                return _response;
            }

            var role = _mapper.Map<ApplicationRole>(request);
            role.TimeCreated = DateTime.Now;

            _roleRepository.Insert(role);
            _unitOfWork.Save();

            _response.Role = role;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Role created successfully";

            return _response;
        }
        public RoleResponse UpdateRole(int id, RoleRequest changes)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Role not found";
                return _response;
            }

            if (_roleRepository.FindByPredicate(u => u.Name == changes.Name))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Role with that name already exists.";
                return _response;
            }

            role.Name = changes.Name;
            role.NormalizedName = role.Name.ToUpper();
            role.TimeModified = DateTime.Now;

            _roleRepository.Update(role);
            _unitOfWork.Save();

            _response.Role = role;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Role updated successfully";

            return _response;
        }

        public RoleResponse DeleteRole(int id)
        {
            ApplicationRole role = _roleRepository.GetById(id);
            if (role == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "Role not found";
                return _response;
            }

            _roleRepository.Delete(role);
            _unitOfWork.Save();

            _response.Role = role;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Role deleted successfully";

            return _response;
        }
    }
}
