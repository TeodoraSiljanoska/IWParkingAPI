using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.AspNetCore.Identity;
using NLog;
using System.Net;

namespace IWParkingAPI.Services.Implementation
{
    public class AuthService : IAuthService
    {

        private readonly IMapper _mapper;
        private readonly UserResponse _response;
        private readonly UserLoginResponse _loginResponse;
        private readonly UserRegisterResponse _registerResponse;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtUtils _jwtUtils;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserDTOResponse _userDTOResponse;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;
        private readonly IGenericRepository<AspNetUser> _userRepository;

        public AuthService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IJwtUtils jwtUtils, SignInManager<ApplicationUser> signInManager, IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new UserResponse();
            _registerResponse = new UserRegisterResponse();
            _loginResponse = new UserLoginResponse();
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtUtils = jwtUtils;
            _userDTOResponse = new UserDTOResponse();
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
        }

        public async Task<UserRegisterResponse> RegisterUser(UserRegisterRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);
                if (user != null)
                {
                    throw new BadRequestException("User already exists");
                }

                var role = await _roleManager.FindByNameAsync(request.Role);
                if (role == null)
                {
                    throw new BadRequestException("Role with that name doesn't exist");
                }

                var newUser = _mapper.Map<ApplicationUser>(request);
                newUser.TimeCreated = DateTime.Now;
                newUser.IsDeactivated = false;

                var result = await _userManager.CreateAsync(newUser, request.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, request.Role);
                    _registerResponse.StatusCode = HttpStatusCode.OK;
                    _registerResponse.Message = "User created successfully";
                    _registerResponse.User = newUser;
                }
                else
                {
                    throw new BadRequestException("User creation failed! Please check User details and try again");
                }
                return _registerResponse;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for RegisterUser {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while User registration {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while User registration");
            }
        }

        public async Task<UserLoginResponse> LoginUser(UserLoginRequest model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    throw new BadRequestException("User with that email doesn't exist");
                }

                if (user.IsDeactivated == true)
                {
                    throw new BadRequestException("User is deactivated");
                }

                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    throw new UnauthorizedException("Password isn't correct");
                }

                // authentication successful so generate jwt token
                return await _jwtUtils.GenerateToken(user);
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for LoginUser {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (UnauthorizedException ex)
            {
                _logger.Error($"Unauthorized for LoginUser {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while User login {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while User login");
            }
        }

        public async Task<UserResponse> ChangePassword(UserResetPasswordRequest model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);
               // var userAspNet = _userRepository.GetAsQueryable(x => x.Email.Equals(model.Email), null, x => x.Include(y => y.Roles)).FirstOrDefault();
                if (user == null)
                {
                    throw new BadRequestException("User with that email doesn't exist");
                }

                var checkOldPass = await _signInManager.PasswordSignInAsync(user.Email, model.OldPassword, false, false);
                if (!checkOldPass.Succeeded)
                {
                    throw new BadRequestException("The old password isn't correct");
                }

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(resetToken))
                {
                    throw new InternalErrorException("Unexpected error while generating reset token");
                }

                if (model.NewPassword == model.OldPassword || model.ConfirmNewPassword == model.OldPassword)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (result.Succeeded)
                {
                   //userAspNet.TimeModified = DateTime.Now;
                    //_userRepository.Update(userAspNet);
                    //_unitOfWork.Save();
                    //var userDto = _mapper.Map<UserDTO>(userAspNet);
                    //_response.User = user;
                    _response.Message = "User reset password successfully";
                    _response.StatusCode = HttpStatusCode.OK;
                    return _response;
                }
                else
                {
                    throw new InternalErrorException("Unexpected error while password reset");
                }
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for ChangePassword {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for ChangePassword {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while password reset {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while password reset");
            }
        }

        public async Task<UserResponse> ChangeEmail(UserChangeEmailRequest model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.OldEmail);

                if (user == null)
                {
                    throw new BadRequestException("User with that email doesn't exist");
                }

                if (user.Email != model.OldEmail)
                {
                    throw new BadRequestException("The old email is incorrect");
                }

                var userWithThatUsername = await _userManager.FindByNameAsync(model.NewEmail);
                if (userWithThatUsername != null)
                {
                    throw new BadRequestException("Email is already taken");
                }

                user.Email = model.NewEmail;
                user.NormalizedEmail = model.NewEmail.ToUpper();
                user.UserName = model.NewEmail;
                user.NormalizedUserName = model.NewEmail.ToUpper();
                user.TimeModified = DateTime.Now;
                // user.SecurityStamp = await _userManager.UpdateSecurityStampAsync(user);
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _response.Message = "Email changed successfully!";
                    _response.StatusCode = HttpStatusCode.OK;
                    return _response;
                }
                else
                {
                    throw new InternalErrorException("Unexpected error while changing the email");
                }
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for ChangeEmail {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for ChangeEmail {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while changing the email {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while changing the email");
            }
        }
    }
}
