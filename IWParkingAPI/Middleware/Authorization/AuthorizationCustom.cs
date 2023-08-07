using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace IWParkingAPI.Middleware.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeCustomAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _claimValues;
        private const string ROLE = "Role";

        public AuthorizeCustomAttribute(params string[] claimValues)
        {
            _claimValues = claimValues;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                context.Result = new CustomUnauthorizedResult(HttpStatusCode.BadRequest, "Token validation failed.");
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Check if the token contains the required claim
                var userClaims = jwtToken.Claims.Where(claim => claim.Type == ROLE && _claimValues.Contains(claim.Value)).ToList();

                if (userClaims.Count > 0)
                {
                    // User has one of the required claims values
                    return;
                }
                else
                {
                    context.Result = new CustomUnauthorizedResult(HttpStatusCode.Forbidden, "You do not have permission to view this action");
                    return;
                }
            }
            catch (Exception)
            {
                // Token validation failed
                context.Result = new CustomUnauthorizedResult(HttpStatusCode.Unauthorized, "Token authorization validation failed");
                return;
            }
        }
    }
}
