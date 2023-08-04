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
    private readonly GetUsersResponse _getResponse;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IUnitOfWork<ParkingDbContextCustom> unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
        _response = new UserResponse();
        _getResponse = new GetUsersResponse();
        _userManager = userManager;
    }

    public GetUsersResponse GetAllUsers()
    {
        var users = _userRepository.GetAll();
        if (users.Count() == 0)
        {
            _getResponse.StatusCode = HttpStatusCode.NoContent;
            _getResponse.Message = "There aren't any users.";
            _getResponse.Users = Enumerable.Empty<ApplicationUser>();
            return _getResponse;
        }
        _getResponse.StatusCode = HttpStatusCode.OK;
        _getResponse.Message = "Users returned successfully";
        _getResponse.Users = users;
        return _getResponse;
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

    public async Task<UserResponse> UpdateUser(int id, UpdateUserRequest changes)
    {
        ApplicationUser user = _userRepository.GetById(id);
        if (user == null || user.IsDeactivated == true)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.Message = "User not found";
            return _response;
        }

        if(user.Name == changes.Name && user.Surname == changes.Surname && user.Email == changes.Email && user.PhoneNumber == changes.PhoneNumber)
        {
            _response.User = user;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = "No updates were entered. Please enter the updates";
            return _response;
        }

        if (changes.Email != user.Email)
        {
            var userByUsername = await _userManager.FindByEmailAsync(changes.Email);
            if (userByUsername != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User with that email already exists.";
                return _response;
            }
        }

        user.Name = (user.Name == changes.Name) ? user.Name : changes.Name;
        user.Surname = (user.Surname == changes.Surname) ? user.Surname : changes.Surname;
        user.UserName = (user.UserName == changes.Email) ? user.UserName : changes.Email;
        user.NormalizedUserName = (user.NormalizedUserName == changes.Email.ToUpper()) ? user.NormalizedUserName : changes.Email.ToUpper();
        user.PhoneNumber = (user.PhoneNumber == changes.PhoneNumber) ? user.PhoneNumber : changes.PhoneNumber;
        user.Email = (user.Email == changes.Email) ? user.Email : changes.Email;
        user.NormalizedEmail = (user.NormalizedEmail == changes.Email.ToUpper()) ? user.NormalizedEmail : changes.Email.ToUpper();
        user.TimeModified = DateTime.Now;
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

