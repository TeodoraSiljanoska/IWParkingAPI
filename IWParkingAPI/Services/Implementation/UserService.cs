using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Net;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IGenericRepository<ApplicationRole> _roleRepository;
    private readonly UserResponse _response;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IUnitOfWork<ParkingDbContextCustom> unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
        _roleRepository = _unitOfWork.GetGenericRepository<ApplicationRole>();
        _mapper = MapperConfig.InitializeAutomapper();
        _response = new UserResponse();
        _userManager = userManager;
    }

    public IEnumerable<ApplicationUser> GetAllUsers()
    {
        var users = _userRepository.GetAll();
        if (users.Count() == 0)
        {
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.Errors.Add("There aren't any users.");
        }
        _response.StatusCode = HttpStatusCode.OK;
        return users;
    }

    public UserResponse GetUserById(int id)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Errors.Add("User not found");
            return _response;
        }
        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;
        return _response;
    }

    public async Task<UserResponse> CreateUser(UserRequest request, string roleName)
    {
        try
        {
            if (_userRepository.FindByPredicate(u => u.UserName == request.UserName))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("User already exists.");
                return _response;
            }

            if (!_roleRepository.FindByPredicate(r => r.Name == roleName))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("Role with that name doesn't exists.");
                return _response;
            }

            var user = _mapper.Map<ApplicationUser>(request);
            user.TimeCreated = DateTime.Now;
            user.IsDeactivated = false;

            var result = await _userManager.CreateAsync(user, user.PasswordHash);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);
                _response.StatusCode = HttpStatusCode.OK;
                _response.User = user;
            }
            else
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add("User registration failed.");
            }
            return _response;
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("An error occurred during user registration.");
            return _response;
        }
    }

    public UserResponse UpdateUser(int id, UserRequest changes)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Errors.Add("User not found");
            return _response;
        }

        if (_userRepository.FindByPredicate(u => u.UserName == changes.UserName))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("User with that username already exists.");
            return _response;
        }

        user.Name = changes.Name;
        user.Surname = changes.Surname;
        user.UserName = changes.UserName;
        user.NormalizedUserName = changes.UserName.ToUpper();
        user.PhoneNumber = changes.PhoneNumber;
        user.Email = changes.Email;
        user.NormalizedEmail = changes.Email.ToUpper();

        _userRepository.Update(user);
        _unitOfWork.Save();

        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;

        return _response;
    }

    public UserResponse DeleteUser(int id)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Errors.Add("User not found");
            return _response;
        }

        _userRepository.Delete(user);
        _unitOfWork.Save();

        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;

        return _response;
    }

    




}

