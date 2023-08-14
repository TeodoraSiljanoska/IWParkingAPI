using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using System.Net;
using IWParkingAPI.CustomExceptions;

public class UserService : IUserService
{
    private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
    private readonly IGenericRepository<AspNetUser> _userRepository;
    private readonly UserResponse _response;
    private readonly GetUsersResponse _getResponse;

    public UserService(IUnitOfWork<ParkingDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
        _response = new UserResponse();
        _getResponse = new GetUsersResponse();
    }

    public GetUsersResponse GetAllUsers()
    {
        try
        {
            var users = _userRepository.GetAll();

            if (!users.Any())
            {
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "There aren't any users";
                _getResponse.Users = Enumerable.Empty<AspNetUser>();
                return _getResponse;
            }

            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Users returned successfully";
            _getResponse.Users = users;
            return _getResponse;
        }
        catch (Exception)
        {
            throw new InternalErrorException("Unexpected error while getting all Users");
        }

    }

    public UserResponse GetUserById(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            AspNetUser user = _userRepository.GetById(id);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            _response.User = user;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "User returned successfully";
            return _response;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InternalErrorException("Unexpected error while getting the User by Id");
        }
    }

    public UserResponse UpdateUser(int id, UpdateUserRequest changes)
    {
        try
        {
            if (id <= 0 || changes.Name == null || changes.Name.Length == 0 || changes.Surname == null || changes.Surname.Length == 0 ||
                changes.Email == null || changes.Email.Length == 0 || changes.PhoneNumber == null || changes.PhoneNumber.Length == 0)
            {
                throw new BadRequestException("User Id, Name, Surname, Email and Phone number are required");
            }

            var user = _userRepository.GetById(id);
            if (user == null || user.IsDeactivated == true)
            {
                throw new NotFoundException("User not found");
            }

            if (user.Name == changes.Name && user.Surname == changes.Surname && user.Email == changes.Email && user.PhoneNumber == changes.PhoneNumber)
            {
                throw new BadRequestException("No updates were entered. Please enter the updates");
            }

            if (changes.Email != user.Email)
            {
                var userByUsername = _userRepository.GetAsQueryable(p => p.Email == changes.Email || p.UserName == changes.Email, null, null).FirstOrDefault();
                if (userByUsername != null)
                {
                    throw new BadRequestException("User with that email already exists");
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
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (InternalErrorException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InternalErrorException("Unexpected error while updating the User");
        }

    }

    public UserResponse DeactivateUser(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            var user = _userRepository.GetById(id);
            if (user == null || user.IsDeactivated == true)
            {
                throw new NotFoundException("User not found");
            }

            user.IsDeactivated = true;
            user.TimeModified = DateTime.Now;
            _userRepository.Update(user);
            _unitOfWork.Save();

            _response.User = user;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "User deactivated successfully";

            return _response;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InternalErrorException("Unexpected error while deactivating the User");
        }
    }
}

