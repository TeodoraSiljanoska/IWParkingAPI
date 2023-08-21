using IWParkingAPI.CustomExceptions;
using Microsoft.IdentityModel.Tokens;
using NLog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IWParkingAPI.Utilities
{
    public class JWTDecode : IJWTDecode
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public JWTDecode(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        public List<Claim> ExtractClaims()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var context = _httpContextAccessor.HttpContext;

            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Split(" ").Last();
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

                    return principal.Claims.ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Unexpected error while decoding the token {Environment.NewLine}ErrorMessage: {ex.Message}", ex.StackTrace);
                    throw new InternalErrorException("Unexpected error while decoding the token");
                }
            }

            return null;
        }

        public string ExtractClaimByType(string claimType)
        {
            if (claimType == null || claimType.Length == 0 || claimType == "")
            {
                return null;
            }

            var claims = ExtractClaims();
            if (claims == null)
            {
                return "";
            }
            var claimByType = claims.Where(claim => claim.Type.Equals(claimType)).FirstOrDefault();

            if (claimByType != null)
            {
                return claimByType.Value;
            }
            return null;
        }
    }
}
