using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using NLog;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
        private readonly IGenericRepository<ApplicationRole> _roleRepository;
        private readonly RoleResponse _response;
        private readonly AllRolesResponse _getResponse;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public RoleService(IUnitOfWork<ParkingDbContextCustom> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _roleRepository = _unitOfWork.GetGenericRepository<ApplicationRole>();
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new RoleResponse();
            _getResponse = new AllRolesResponse();
        }
        public AllRolesResponse GetAllRoles()
        {
            try
            {
                var roles = _roleRepository.GetAll();
                if (roles.Count() == 0)
                {
                    _getResponse.StatusCode = HttpStatusCode.NoContent;
                    _getResponse.Message = "There aren't any roles.";
                    _getResponse.Roles = Enumerable.Empty<RoleDTO>();
                    return _getResponse;
                }

                List<RoleDTO> rolesDto = new List<RoleDTO>();

                foreach (var r in roles)
                {
                    rolesDto.Add(_mapper.Map<RoleDTO>(r));
                }

                _getResponse.Roles = rolesDto;
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "Roles returned successfully";
                return _getResponse;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting all Roles {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting all Roles");

            }
        }

        public RoleResponse GetRoleById(int id)
        {
            try
            {
                ApplicationRole role = CheckIfRoleExists(id);

                var roleDto = _mapper.Map<RoleDTO>(role);

                _response.Role = roleDto;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Role returned successfully";
                return _response;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for GetRoleById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for GetRoleById {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while getting Role by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while getting the Role by Id");
            }
        }

        public RoleResponse CreateRole(RoleRequest request)
        {
            try
            {
                if (_roleRepository.FindByPredicate(u => u.Name == request.Name))
                {
                    throw new BadRequestException("Role already exists");
                }

                var role = _mapper.Map<ApplicationRole>(request);
                role.TimeCreated = DateTime.Now;

                _roleRepository.Insert(role);
                _unitOfWork.Save();

                var roleDto = _mapper.Map<RoleDTO>(role);

                _response.Role = roleDto;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Role created successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CreateRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while creating the Role {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while creating the Role");
            }
        }

        public RoleResponse UpdateRole(int id, RoleRequest changes)
        {
            try
            {
                ApplicationRole role = CheckIfRoleExists(id);
                CheckRoleUpdateDetails(changes, role);

                role.Name = (role.Name == changes.Name) ? role.Name : changes.Name;
                role.NormalizedName = (role.NormalizedName == changes.Name.ToUpper()) ? role.NormalizedName : changes.Name.ToUpper();
                role.TimeModified = DateTime.Now;

                _roleRepository.Update(role);
                _unitOfWork.Save();

                var roleDto = _mapper.Map<RoleDTO>(role);
                _response.Role = roleDto;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Role updated successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for UpdateRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for UpdateRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while updating the Role {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while updating the Role");
            }
        }

        public RoleResponse DeleteRole(int id)
        {
            try
            {
                ApplicationRole role = CheckIfRoleExists(id);

                _roleRepository.Delete(role);
                _unitOfWork.Save();

                var roleDto = _mapper.Map<RoleDTO>(role);

                _response.Role = roleDto;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Role deleted successfully";

                return _response;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for DeleteRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for DeleteRole {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while deleting the Role {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while deleting the Role");
            }
        }

        private ApplicationRole CheckIfRoleExists(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException("Role Id is required");
                }

                ApplicationRole role = _roleRepository.GetById(id);
                if (role == null)
                {
                    throw new NotFoundException("Role not found");
                }

                return role;
            }
            catch (NotFoundException ex)
            {
                _logger.Error($"Not Found for CheckIfRoleExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckIfRoleExists {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if Role exists in CheckIfRoleExists method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if Role exists in CheckIfRoleExists method");
            }

        }

        private void CheckRoleUpdateDetails(RoleRequest changes, ApplicationRole role)
        {
            try
            {
                if (changes.Name == role.Name)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                if (changes.Name != role.Name)
                {
                    if (_roleRepository.FindByPredicate(u => u.Name == changes.Name))
                    {
                        throw new BadRequestException("Role with that name already exists");
                    }
                }
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckRoleUpdateDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking Role update details in CheckRoleUpdateDetails method" +
                    $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking Role update details in CheckRoleUpdateDetails method");
            }

        }
    }
}
