using AutoMapper;
using IWParkingAPI.CustomExceptions;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using IWParkingAPI.Utilities;
using Microsoft.AspNetCore.Identity;
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
        public AuthService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IJwtUtils jwtUtils, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new UserResponse();
            _registerResponse = new UserRegisterResponse();
            _loginResponse = new UserLoginResponse();
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtUtils = jwtUtils;
        }

        public async Task<UserRegisterResponse> RegisterUser(UserRegisterRequest request)
        {
            try
            {
                if (request == null || request.Name.Length == 0 || request.Surname.Length == 0 || request.Email.Length == 0 || request.Password.Length ==0
                    || request.ConfirmPassword.Length == 0 || request.Phone.Length == 0 || request.Role.Length == 0 )
                {
                    throw new BadRequestException("Name, Surname, Email, Password, Confirm Password, Phone and Role are required");
                }
                var user = await _userManager.FindByNameAsync(request.Email);
                if (user != null)
                {
                    throw new BadRequestException("User already exists");
                }

                if (request.Password != request.ConfirmPassword)
                {
                    throw new BadRequestException("Passwords do not match");
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
            catch(BadRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while User registration");
            }
        }

        public async Task<UserLoginResponse> LoginUser(UserLoginRequest model)
        {
            try
            {
                if (model == null || model.Email.Length == 0 || model.Password.Length == 0)
                {
                    throw new BadRequestException("Email and Password are required");
                }
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
            catch(BadRequestException ex)
            {
                throw;
            }
            catch(UnauthorizedException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new InternalErrorException("Unexpected error while User login");
            }
        }

        public async Task<UserResponse> ChangePassword(UserResetPasswordRequest model)
        {
            try
            {
                if (model == null || model.Email.Length == 0 || model.OldPassword.Length == 0 || model.NewPassword.Length == 0 || model.ConfirmNewPassword.Length == 0 )
                {
                    throw new BadRequestException("Email, Old Password, New Password and Confirm New Password are required");
                }
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

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(resetToken))
                {
                    throw new InternalErrorException("Unexpected error while generating reset token");
                }

                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    throw new BadRequestException("New passwords don't match");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (result.Succeeded)
                {
                    _response.Message = "User reset password successfully";
                    _response.StatusCode = HttpStatusCode.OK;
                    return _response;
                }
                else
                {
                    throw new InternalErrorException("Unexpected error while password reset");
                }
            }
            catch(BadRequestException ex)
            {
                throw;
            }
            catch(InternalErrorException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InternalErrorException("Unexpected error while password reset");
            }
        }

        public async Task<UserResponse> ChangeEmail(UserChangeEmailRequest model)
        {
            try
            {
                if (model == null || model.OldEmail.Length == 0 || model.NewEmail.Length == 0)
                {
                    throw new BadRequestException("Old Email and New Email are required");
                }

                var user = await _userManager.FindByNameAsync(model.OldEmail);

                if (user == null)
                {
                    throw new BadRequestException("User with that email doesn't exist");
                }

                if (user.Email != model.OldEmail)
                {
                    throw new BadRequestException("The old email is incorrect");
                }

                if (model.OldEmail == model.NewEmail)
                {
                    throw new BadRequestException("No updates were entered. Please enter the updates");
                }

                var userwiththatusername = await _userManager.FindByNameAsync(model.NewEmail);
                if (userwiththatusername != null)
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
            catch(BadRequestException ex)
            {
                throw;
            }
            catch(InternalErrorException ex)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new InternalErrorException("Unexpected error while changing the email");
            }

        }

    }
}
