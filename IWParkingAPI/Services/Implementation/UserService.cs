using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
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
using IWParkingAPI.Models;

public class UserService : IUserService
{
    private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
    private readonly IGenericRepository<AspNetUser> _userRepository;
    private readonly AllUsersResponse _getResponse;
    private readonly UserResponse _userDTOResponse;
    private readonly ResponseBase _response;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IMapper _mapper;
    private readonly IJWTDecode _jWTDecode;
    private const int PageSize = 5;
    private const int PageNumber = 1;

    public UserService(IUnitOfWork<ParkingDbContext> unitOfWork, IJWTDecode jWTDecode)
    {
        _unitOfWork = unitOfWork;
        _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
        _getResponse = new AllUsersResponse();
        _userDTOResponse = new UserResponse();
        _response = new ResponseBase();
        _mapper = MapperConfig.InitializeAutomapper();
        _jWTDecode = jWTDecode;
    }

    public AllUsersResponse GetAllUsers(int pageNumber, int pageSize)
    {
        try
        {
            var users = _userRepository.GetAsQueryable(null, null, x => x.Include(y => y.Roles)).ToList();
            int totalPages;
            List<AspNetUser> paginatedUsers;

            PaginateUsers(ref pageNumber, ref pageSize, users, out totalPages, out paginatedUsers);

            if (!paginatedUsers.Any())
            {
                _getResponse.StatusCode = HttpStatusCode.OK;
                _getResponse.Message = "There aren't any users";
                _getResponse.Users = Enumerable.Empty<UserDTO>();
                return _getResponse;
            }

            var UserDTOList = new List<UserDTO>();
            foreach (var user in paginatedUsers)
            {
                UserDTOList.Add(_mapper.Map<UserDTO>(user));
            }

            _getResponse.StatusCode = HttpStatusCode.OK;
            _getResponse.Message = "Users returned successfully";
            _getResponse.Users = UserDTOList;
            _getResponse.NumPages = totalPages;
            return _getResponse;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while getting all Users {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while getting all Users");
        }
    }

    public UserResponse GetUserById()
    {
        try
        {
            AspNetUser? user = CheckIfUserExists();

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

    public UserResponse UpdateUser(UpdateUserRequest changes)
    {
        try
        {
            AspNetUser? user = CheckIfUserExists();

            CheckUserUpdateDetails(changes, user);

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

    public ResponseBase DeactivateUser()
    {
        try
        {
            AspNetUser? user = CheckIfUserExists();

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

    public UserResponse DeactivateUserAdmin(int userId)
    {
        try
        {
            AspNetUser? user = CheckIfUserExistsAdmin(userId);

            user.IsDeactivated = true;
            user.TimeModified = DateTime.Now;
            _userRepository.Update(user);
            _unitOfWork.Save();

            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "Profile successfully deactivated";

            return _response;
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

    private static void PaginateUsers(ref int pageNumber, ref int pageSize, List<AspNetUser> users,
        out int totalPages, out List<AspNetUser> paginatedUsers)
    {
        if (pageNumber == 0)
        {
            pageNumber = PageNumber;
        }
        if (pageSize == 0)
        {
            pageSize = PageSize;
        }

        var totalCount = users.Count();
        totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        paginatedUsers = users.Skip((pageNumber - 1) * pageSize)
                                             .Take(pageSize)
                                             .ToList();
    }

    private AspNetUser CheckIfUserExists()
    {
        try
        {
            var id = Convert.ToInt32(_jWTDecode.ExtractClaimByType("Id"));

            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();

            if (user == null || user.IsDeactivated == true)
            {
                throw new NotFoundException("User not found");
            }

            return user;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for CheckIfUserExists {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for CheckIfUserExists {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while checking if User exists in CheckIfUserExists method" +
                $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while checking if User exists in CheckIfUserExists method");
        }
    }

    private AspNetUser CheckIfUserExistsAdmin(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new BadRequestException("User Id is required");
            }

            var user = _userRepository.GetAsQueryable(u => u.Id == id, null, x => x.Include(y => y.Roles)).FirstOrDefault();

            if (user == null || user.IsDeactivated == true)
            {
                throw new NotFoundException("User not found");
            }

            return user;
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for CheckIfUserExistsAdmin {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.Error($"Not Found for CheckIfUserExistsAdmin {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while checking if User exists from Admin in CheckIfUserExistsAdmin method" +
                $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while checking if User exists from Admin in CheckIfUserExistsAdmin method");
        }

    }

    private void CheckUserUpdateDetails(UpdateUserRequest changes, AspNetUser user)
    {
        try
        {
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
        }
        catch (BadRequestException ex)
        {
            _logger.Error($"Bad Request for CheckUserUpdateDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while checking User update details in CheckUserUpdateDetails method" +
                $" {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
            throw new InternalErrorException("Unexpected error while checking User update details in CheckUserUpdateDetails method");
        }

    }
}

