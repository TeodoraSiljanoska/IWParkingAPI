using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly UserResponse _response;

    public UserService(IUnitOfWork<ParkingDbContextCustom> unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
        _mapper = MapperConfig.InitializeAutomapper();
        _response = new UserResponse();
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

    public UserResponse CreateUser(UserRequest request)
    {
        if (_userRepository.FindByPredicate(u => u.UserName == request.UserName))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Errors.Add("User already exists.");
            return _response;
        }

        var user = _mapper.Map<ApplicationUser>(request);
        user.TimeCreated = DateTime.Now;
        user.IsDeactivated = false;

        _userRepository.Insert(user);
        _unitOfWork.Save();

        _response.User = user;
        _response.StatusCode = HttpStatusCode.OK;

        return _response;
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