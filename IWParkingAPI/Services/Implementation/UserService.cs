using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Net;

public class UserService : IUserService
{
    private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly UserResponse _response;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IUnitOfWork<ParkingDbContextCustom> unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
        _response = new UserResponse();
        _userManager = userManager;
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
        _response.Message = "Users returned successfully";
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
        _response.Message = "User returned successfully";
        return _response;
    }

    public async Task<UserResponse> UpdateUser(int id, UserRequest changes)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Message = "User not found";
            return _response;
        }

        var userByUsername = await _userManager.FindByNameAsync(changes.UserName);
        if (userByUsername != null)
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
        _response.Message = "User updated successfully";

        return _response;
    }

    public UserResponse DeactivateUser(int id)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Message = "User not found";
            return _response;
        }

        user.IsDeactivated = true;
        _userRepository.Update(user);
        _unitOfWork.Save();

        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;
        _response.Message = "User deactivated successfully";

        return _response;
    }






}

