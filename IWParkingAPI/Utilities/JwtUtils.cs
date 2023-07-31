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

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
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


        /*public string GenerateToken(string name)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var header = new JwtHeader(credentials);

            var payload = new JwtPayload(name.ToString(), null, null, null, DateTime.Today.AddDays(1));
            var securityToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(securityToken);

        }*/

        /*public bool ValidateToken(string token)
        {
            if (token == null)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(token);
            var readToken = (JwtSecurityToken)(new JwtSecurityTokenHandler().ReadToken(token));

            //return readToken.Claims.First(c => c.Type == "iss").Value.Equals("string");

            var claims = _httpContextAccessor.HttpContext.User.Claims;

            // Check if the user has the specified role
            //bool isInRole = claims.Any(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "User");
            bool isInRole = readToken.Claims.First(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value.Equals("User");

            // Get the current user's nameidentifier claim
            *//*var nameIdentifierClaim = _httpContextAccessor.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            var userId = nameIdentifierClaim?.Value;*//*
            bool name = readToken.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value.Equals("user");

            // You can also check for additional claims if needed
            //return isInRole && !string.IsNullOrEmpty(userId);
            return isInRole && name;

        }*/
    }
}
