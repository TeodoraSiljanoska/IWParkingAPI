using IWParkingAPI.Utilities;
using System.Net;

namespace IWParkingAPI.Middleware.Authentication
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            var allowedUrlsList = new List<string> { RouteEndpoints.Login, RouteEndpoints.Register, RouteEndpoints.PasswordReset };

            // if the path is one of these defined paths,
            // the token doesn't need to be validated
            if (context.Request.Path.HasValue && allowedUrlsList.Contains(context.Request.Path.Value))
            {
                await _next(context);
                return;
            }
            // else, the token needs to be validated
            else
            {
                // get the token from the Request Header
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                // if the token is null, return unauthorized
                if (token == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                // else, if token is not null, validate it
                bool response = jwtUtils.ValidateToken(token);

                // return the validation response
                if (response)
                {
                    await _next(context);
                }
            }
        }
    }
}
