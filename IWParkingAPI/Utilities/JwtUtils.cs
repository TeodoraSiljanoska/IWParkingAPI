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
        private readonly UserLoginResponse userLoginResponse;
        private readonly TokenValidationResponse tokenValidationResponse;
        private readonly string secretKey;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtUtils() { }
        public JwtUtils(IConfiguration configuration, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _config = configuration;
            _userManager = userManager;
            userLoginResponse = new UserLoginResponse();
            tokenValidationResponse = new TokenValidationResponse();
            secretKey = _config["Jwt:Key"];
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<UserLoginResponse> GenerateToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                //new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Id", user.Id.ToString()),
                new Claim("Name", user.Name),
                new Claim("Surname", user.Surname),
                new Claim("Email", user.Email),
                new Claim("Phone", user.PhoneNumber)
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
            userLoginResponse.Message = "User logged in successfully";
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

        public string ExtractUserIdFromToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", ""); // Extract the token from "Bearer <token>"

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    SecurityToken validatedToken;
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                    var userIdClaim = principal.FindFirst("Id"); // The claim that holds the user ID
                    if (userIdClaim != null)
                    {
                        return userIdClaim.Value;
                    }
                }
                catch (Exception)
                {
                    // Token validation or decoding failed
                }
            }

            return null;
        }
    }
}
