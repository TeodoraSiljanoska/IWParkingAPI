using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IWParkingAPI.Utilities
{
    public class JwtUtils : IJwtUtils
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserLoginResponse userLoginResponse;
        private readonly TokenValidationResponse tokenValidationResponse;
        private readonly string secretKey;
        private readonly IJWTDecode _jwtDecode;
        private readonly IGenericRepository<AspNetUser> _userRepository;
        private readonly IGenericRepository<AspNetRole> _roleRepository;
        private readonly IUnitOfWork<ParkingDbContext> _unitOfWork;


        public JwtUtils(IConfiguration configuration, UserManager<ApplicationUser> userManager,
            IJWTDecode jwtDecode, IUnitOfWork<ParkingDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetGenericRepository<AspNetUser>();
            _roleRepository = _unitOfWork.GetGenericRepository<AspNetRole>();
            _config = configuration;
            _userManager = userManager;
            userLoginResponse = new UserLoginResponse();
            tokenValidationResponse = new TokenValidationResponse();
            secretKey = _config["Jwt:Key"];
            _jwtDecode = jwtDecode;
        }
        public async Task<UserLoginResponse> GenerateToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                /// Not in use
                //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                //new Claim(ClaimTypes.Name, user.UserName),
                //new Claim("Name", user.Name),
                //new Claim("Surname", user.Surname),
                //new Claim("Email", user.Email),
                //new Claim("Phone", user.PhoneNumber)

                new Claim("Id", user.Id.ToString())
            };

            foreach (var userRole in userRoles)
            {
                //authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                authClaims.Add(new Claim("Role", userRole));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                authClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            var usertoken = new JwtSecurityTokenHandler().WriteToken(token);

            userLoginResponse.StatusCode = HttpStatusCode.OK;
            userLoginResponse.Message = "Successfully logged in";
            userLoginResponse.Token = usertoken;
            userLoginResponse.Role = userRoles.FirstOrDefault();
            return userLoginResponse;

        }


        public TokenValidationResponse ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero // Optional: Adjust the tolerance for expired tokens
                }, out SecurityToken validatedToken);


                var userIdFromClaims = _jwtDecode.ExtractClaimByType("Id");
                var roleFromClaims = _jwtDecode.ExtractClaimByType("Role");

                var applicationUser = _userRepository.GetAsQueryable(x => x.Id == int.Parse(userIdFromClaims),
                    null, x => x.Include(y => y.Roles)).FirstOrDefault();

                var role = _roleRepository.GetAsQueryable(x => x.Name == roleFromClaims.ToString()).FirstOrDefault();
                if (applicationUser == null || role == null || !applicationUser.Roles.Contains(role))

                {
                    tokenValidationResponse.StatusCode = HttpStatusCode.BadRequest;
                    tokenValidationResponse.Message = "Token authentication validation failed";
                    tokenValidationResponse.IsValid = false;
                    return tokenValidationResponse;
                }

                tokenValidationResponse.StatusCode = HttpStatusCode.OK;
                tokenValidationResponse.Message = "Token authentication validation is successful";
                tokenValidationResponse.IsValid = true;
                return tokenValidationResponse;
            }
            catch (Exception ex)
            {
                tokenValidationResponse.StatusCode = HttpStatusCode.BadRequest;
                tokenValidationResponse.Message = "Token authentication validation failed";
                tokenValidationResponse.IsValid = false;
                return tokenValidationResponse;
            }
        }
    }
}

