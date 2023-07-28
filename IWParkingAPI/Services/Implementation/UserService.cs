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
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IGenericRepository<ApplicationRole> _roleRepository;
    private readonly UserResponse _response;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(IUnitOfWork<ParkingDbContextCustom> unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
        _roleRepository = _unitOfWork.GetGenericRepository<ApplicationRole>();
        _mapper = MapperConfig.InitializeAutomapper();
        _response = new UserResponse();
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IEnumerable<ApplicationUser> GetAllUsers()
    {
        var users = _userRepository.GetAll();
        if (users.Count() == 0)
        {
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.Message = "There aren't any users.";
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
            _response.Message = "User not found";
            return _response;
        }
        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;
        return _response;
    }

    public async Task<UserResponse> RegisterUser([FromBody] UserRegisterRequest request)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User already exists.";
                return _response;
            }

            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Role with that name doesn't exists.";
                return _response;
            }

            var newUser = _mapper.Map<ApplicationUser>(request);
            newUser.TimeCreated = DateTime.Now;
            newUser.IsDeactivated = false;

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, request.RoleName);
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "User created successfully";
                _response.User = newUser;
            }
            else
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User creation failed! Please check user details and try again.";
            }
            return _response;
        }
        catch (Exception ex)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = "An error occurred during user registration.";
            return _response;
        }
    }

    public UserResponse UpdateUser(int id, UserRequest changes)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Message = "User not found";
            return _response;
        }

        if (_userRepository.FindByPredicate(u => u.UserName == changes.UserName))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = "User with that username already exists.";
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
            _response.Message = "User not found";
            return _response;
        }

        _userRepository.Delete(user);
        _unitOfWork.Save();

        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;

        return _response;
    }

    




}

