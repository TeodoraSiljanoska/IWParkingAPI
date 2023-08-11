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
                    _registerResponse.StatusCode = HttpStatusCode.BadRequest;
                    _registerResponse.Message = "User creation failed! Please check user details and try again.";
                }
                return _registerResponse;
            }
            catch (Exception ex)
            {
                _registerResponse.StatusCode = HttpStatusCode.BadRequest;
                _registerResponse.Message = "An error occurred during user registration.";
                return _registerResponse;
            }
        }

        public async Task<UserLoginResponse> LoginUser(UserLoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                _loginResponse.Message = "User with that email doesn't exist";
                _loginResponse.StatusCode = HttpStatusCode.BadRequest;
                return _loginResponse;
            }
            if(user.IsDeactivated == true)
            {
                _loginResponse.Message = "User is deactivated.";
                _loginResponse.StatusCode = HttpStatusCode.BadRequest;
                return _loginResponse;
            }

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _loginResponse.Message = "Password isn't correct";
                _loginResponse.StatusCode = HttpStatusCode.Unauthorized;
                return _loginResponse;
            }

            // authentication successful so generate jwt token
            return await _jwtUtils.GenerateToken(user);
        }

        public async Task<UserResponse> ChangePassword(UserResetPasswordRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                _response.Message = "User with that email doesn't exist";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            var checkOldPass = await _signInManager.PasswordSignInAsync(user.Email, model.OldPassword, false, false);
            if (!checkOldPass.Succeeded)
            {
                _response.Message = "The old password isn't correct";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(resetToken))
            {
                _response.Message = "Error while generating reset token";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                _response.Message = "The new passwords don't match";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
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
                _response.Message = "User didn't reset password successfully";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        public async Task<UserResponse> ChangeEmail(UserChangeEmailRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.OldEmail);

            if (user == null)
            {
                _response.Message = "User with that email doesn't exist";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            if(user.Email != model.OldEmail)
            {
                _response.Message = "Incorrect old email";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            if (model.OldEmail == model.NewEmail)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "No updates were entered. Please enter the updates";
                return _response;
            }    

            var userwiththatusername = await _userManager.FindByNameAsync(model.NewEmail);
            if (userwiththatusername != null)
            {
                _response.Message = "Email is already taken";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
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
                _response.Message = "There was a problem changing the email!";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

        }

    }
}
