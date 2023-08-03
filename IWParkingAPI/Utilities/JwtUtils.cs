using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserLoginResponse response;
        private readonly string secretKey;
        public JwtUtils(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _config = configuration;
            _userManager = userManager;
            response = new UserLoginResponse();
            secretKey = _config["Jwt:Key"];
    }
        public async Task<UserLoginResponse> GenerateToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            IdentityOptions _options = new IdentityOptions();
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),

                //new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
                //new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                authClaims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            var usertoken = new JwtSecurityTokenHandler().WriteToken(token);

            response.StatusCode = HttpStatusCode.OK;
            response.Message = "User logged in successfully";
            response.Token = usertoken;
            return response;

        }


        public bool ValidateToken(string token)
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

                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the validation exception as needed
                return false;
            }
        }
    }
}
