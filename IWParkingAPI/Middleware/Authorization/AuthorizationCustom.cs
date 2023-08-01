using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace IWParkingAPI.Middleware.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeCustomAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _claimValues;
        private const string ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public AuthorizeCustomAttribute(params string[] claimValues)
        {
            _claimValues = claimValues;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                context.Result = new UnauthorizedResult();
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
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception)
            {
                // Token validation failed
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
}
