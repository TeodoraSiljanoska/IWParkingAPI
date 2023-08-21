using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Services.Interfaces;
using System.Net;
using IWParkingAPI.CustomExceptions;
using NLog;
using Microsoft.EntityFrameworkCore;
using IWParkingAPI.Mappers;
using AutoMapper;
using IWParkingAPI.Utilities;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Models.Responses;

public class UserService : IUserService
{
    private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
    private readonly IGenericRepository<AspNetUser> _userRepository;
    private readonly GetUsersDTOResponse _getResponse;
    private readonly UserDTOResponse _userDTOResponse;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IMapper _mapper;
    private readonly IJWTDecode _jWTDecode;

    public UserService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
        _getResponse = new GetUsersDTOResponse();
        _userDTOResponse = new UserDTOResponse();
        _mapper = MapperConfig.InitializeAutomapper();
        _jWTDecode = jWTDecode;
    }

    public GetUsersDTOResponse GetAllUsers()
    {
        try
        {
            var users = _userRepository.GetAsQueryable(null, null, x => x.Include(y => y.Roles)).ToList();

            if (!users.Any())
            {
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "There aren't any users";
                _getResponse.Users = Enumerable.Empty<UserDTO>();
                return _getResponse;
            }

            var UserDTOList = new List<UserDTO>();
            foreach (var user in users)
            {
                UserDTOList.Add(_mapper.Map<UserDTO>(user));
            }

            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Users returned successfully";
            _getResponse.Users = UserDTOList;
            return _getResponse;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while getting all Users {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while getting all Users");
        }
    }

    public UserDTOResponse GetUserById()
    {
        try
        {
            var id = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());

            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var userDto = _mapper.Map<UserDTO>(user);

            _userDTOResponse.User = userDto;
            _userDTOResponse.StatusCode = HttpStatusCode.OK;
            _userDTOResponse.Message = "User returned successfully";
            return _userDTOResponse;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for GetUserById {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for GetUserById {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while getting the User by Id {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while getting the User by Id");
        }
    }

    public UserDTOResponse UpdateUser(UpdateUserRequest changes)
    {
        try
        {
            var id = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();
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

            var userDto = _mapper.Map<UserDTO>(user);

            _userDTOResponse.User = userDto;
            _userDTOResponse.StatusCode = HttpStatusCode.OK;
            _userDTOResponse.Message = "User updated successfully";

            return _userDTOResponse;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for UpdateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for UpdateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (InternalErrorException ex)
        {
            _logger.Error($"Internal Error for UpdateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while updating the User {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while updating the User");
        }

    }

    public UserDTOResponse DeactivateUser()
    {
        try
        {
            var id = Convert.ToInt32(_jWTDecode.ExtractUserIdFromToken());
            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            if (user.IsDeactivated == true)
            {
                throw new BadRequestException("User is already deactivated");
            }

            user.IsDeactivated = true;
            user.TimeModified = DateTime.Now;
            _userRepository.Update(user);
            _unitOfWork.Save();

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.IsDeactivated = true;

            _userDTOResponse.User = userDto;
            _userDTOResponse.StatusCode = HttpStatusCode.OK;
            _userDTOResponse.Message = "User deactivated successfully";

            return _userDTOResponse;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for DeactivateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for DeactivateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while deactivating the User {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while deactivating the User");
        }
    }

    public UserDTOResponse DeactivateUserAdmin(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            if (user.IsDeactivated == true)
            {
                throw new BadRequestException("User is already deactivated");
            }

            user.IsDeactivated = true;
            user.TimeModified = DateTime.Now;
            _userRepository.Update(user);
            _unitOfWork.Save();

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.IsDeactivated = true;

            _userDTOResponse.User = userDto;
            _userDTOResponse.StatusCode = HttpStatusCode.OK;
            _userDTOResponse.Message = "User deactivated successfully";

            return _userDTOResponse;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for DeactivateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for DeactivateUser {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while deactivating the User {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while deactivating the User");
        }
    }
}

