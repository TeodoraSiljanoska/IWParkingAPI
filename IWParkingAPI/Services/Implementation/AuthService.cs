using AutoMapper;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtUtils _jwtUtils;
        public AuthService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
            IJwtUtils jwtUtils, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = MapperConfig.InitializeAutomapper();
            _response = new UserResponse();
            _loginResponse = new UserLoginResponse();
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtUtils = jwtUtils;
        }

        public async Task<UserResponse> RegisterUser(UserRegisterRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user != null)
                {
                    _response.Message = "User already exists.";
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                if (request.Password != request.ConfirmPassword)
                {
                    _response.Message = "Passwords do not match";
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }

                var role = await _roleManager.FindByNameAsync(request.Role);
                if (role == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Role with that name doesn't exists.";
                    return _response;
                }

                var newUser = _mapper.Map<ApplicationUser>(request);
                newUser.TimeCreated = DateTime.Now;
                newUser.IsDeactivated = false;

                var result = await _userManager.CreateAsync(newUser, request.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, request.Role);
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Message = "User created successfully";
                    _response.User = newUser;
                }
                else
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "User creation failed! Please check user details and try again.";
                }
                return _response;
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "An error occurred during user registration.";
                return _response;
            }
        }

        public async Task<UserLoginResponse> LoginUser(UserLoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                _loginResponse.Message = "User with that username doesn't exist";
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

        public async Task<UserResponse> ResetPassword(UserResetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                _response.Message = "User with that username doesn't exist";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            var checkOldPass = await _signInManager.PasswordSignInAsync(user.UserName, model.OldPassword, false, false);
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

        public async Task<UserResponse> ChangeUsername(UserChangeEmailRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.OldUsername);

            if (user == null)
            {
                _response.Message = "User with that username doesn't exist";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
          
                var userwiththatusername = await _userManager.FindByNameAsync(model.NewUsername);
                if (userwiththatusername != null)
                {
                    _response.Message = "Username is already taken";
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return _response;
                }
                

                
            user.UserName = model.NewUsername;
            user.NormalizedUserName = model.NewUsername.ToUpper();
            user.TimeModified = DateTime.Now;
           // user.SecurityStamp = await _userManager.UpdateSecurityStampAsync(user);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _response.Message = "Username changed successfully!";
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            else
            {
                _response.Message = "There was a problem updating the username!";
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }



        }
    }
}
