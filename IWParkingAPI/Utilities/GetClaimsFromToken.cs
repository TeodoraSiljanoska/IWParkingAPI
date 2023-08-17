using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IWParkingAPI.Utilities
{
    public class GetClaimsFromToken
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetClaimsFromToken() { }

     /*   public GetClaimsFromToken(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _config = configuration;
            _httpContextAccessor = httpContextAccessor;
        }  */
        public string ExtractUserIdFromToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", ""); // Extract the token from "Bearer <token>"

             /*   var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }; */

                try
                {
                    //SecurityToken validatedToken;
               var principal =     tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero // Optional: Adjust the tolerance for expired tokens
                    }, out SecurityToken validatedToken);
                   // var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

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
