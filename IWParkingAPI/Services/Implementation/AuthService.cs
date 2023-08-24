using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtUtils _jwtUtils;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserResponse _userDTOResponse;
        private readonly ResponseBase _responseBase;

        public AuthService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IJwtUtils jwtUtils, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtUtils = jwtUtils;
            _userDTOResponse = new UserResponse();
            _responseBase = new ResponseBase();
        }

        public async Task<UserResponse> RegisterUser(UserRegisterRequest request)
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

                var userDto = _mapper.Map<UserDTO>(newUser);
                userDto.Role = request.Role;

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, request.Role);
                    _userDTOResponse.StatusCode = HttpStatusCode.OK;
                    _userDTOResponse.Message = "User created successfully";
                    _userDTOResponse.User = userDto;
                }
                else
                {
                    throw new BadRequestException("User creation failed! Please check User details and try again");
                }
                return _userDTOResponse;
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

        public async Task<ResponseBase> ChangePassword(UserResetPasswordRequest model)
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
                    _responseBase.Message = "User reset password successfully";
                    _responseBase.StatusCode = HttpStatusCode.OK;
                    return _responseBase;
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

        public async Task<ResponseBase> ChangeEmail(UserChangeEmailRequest model)
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
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _responseBase.Message = "Email changed successfully!";
                    _responseBase.StatusCode = HttpStatusCode.OK;
                    return _responseBase;
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
