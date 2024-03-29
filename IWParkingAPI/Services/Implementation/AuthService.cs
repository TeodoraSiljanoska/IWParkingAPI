﻿using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Mappers;
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
        private readonly ResponseBase _responseBase;

        public AuthService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IJwtUtils jwtUtils, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtUtils = jwtUtils;
            _responseBase = new ResponseBase();
        }

        public async Task<ResponseBase> RegisterUser(UserRegisterRequest request)
        {
            try
            {
                await CheckUserRegisterDetails(request);

                var newUser = _mapper.Map<ApplicationUser>(request);
                newUser.TimeCreated = DateTime.Now;
                newUser.IsDeactivated = false;

                var result = await _userManager.CreateAsync(newUser, request.Password);

                var userDto = _mapper.Map<UserDTO>(newUser);
                userDto.Role = request.Role;

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, request.Role);
                    _responseBase.StatusCode = HttpStatusCode.OK;
                    _responseBase.Message = "Successfully signed up";
                }
                else
                {
                    throw new BadRequestException("User creation failed! Please check User details and try again");
                }
                return _responseBase;
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
                ApplicationUser user = await CheckUserLoginDetails(model);

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
                ApplicationUser user = await CheckChangePasswordDetails(model);

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(resetToken))
                {
                    throw new InternalErrorException("Unexpected error while generating reset token");
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
                ApplicationUser user = await CheckChangeEmailDetails(model);

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

        private async Task CheckUserRegisterDetails(UserRegisterRequest request)
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
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckUserRegisterDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking if the User and Role exist in the RegisterUser method " +
                    $"{Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking if the User and Role exist in the RegisterUser method");
            }
        }

        private async Task<ApplicationUser> CheckUserLoginDetails(UserLoginRequest model)
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

                return user;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckUserLoginDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (UnauthorizedException ex)
            {
                _logger.Error($"Unauthorized for CheckUserLoginDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking User Login Details {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking User Login Details");
            }
        }

        private async Task<ApplicationUser> CheckChangePasswordDetails(UserResetPasswordRequest model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    throw new BadRequestException("User with that email doesn't exist");
                }

                var checkOldPass = await _signInManager.PasswordSignInAsync(user.Email, model.OldPassword, false, false);
                if (!checkOldPass.Succeeded)
                {
                    throw new BadRequestException("The old password isn't correct");
                }

                if (model.NewPassword == model.OldPassword || model.ConfirmNewPassword == model.OldPassword)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                return user;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckChangePasswordDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for CheckChangePasswordDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking Change Password Details {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking Change Password Details");
            }

        }

        private async Task<ApplicationUser> CheckChangeEmailDetails(UserChangeEmailRequest model)
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

                return user;
            }
            catch (BadRequestException ex)
            {
                _logger.Error($"Bad Request for CheckChangeEmailDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (InternalErrorException ex)
            {
                _logger.Error($"Internal Error for CheckChangeEmailDetails {Environment.NewLine}ErrorMessage: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error while checking Change Email details {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                throw new InternalErrorException("Unexpected error while checking Change Email details");
            }

        }
    }
}
