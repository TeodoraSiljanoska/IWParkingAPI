using IWParkingAPI.Utilities;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace IWParkingAPI.Middleware.Authentication
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string contentType = "application/json";

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            var allowedUrlsList = new List<string> { RouteEndpoints.Login, RouteEndpoints.Register, RouteEndpoints.ParkingLots };

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
                    context.Response.ContentType = contentType;

                    var responseMessage = new
                    {
                        message = "Token for authentication was not found",
                        statusCode = context.Response.StatusCode
                    };

                    var responseBody = JsonConvert.SerializeObject(responseMessage);
                    await context.Response.WriteAsync(responseBody, Encoding.UTF8);
                    return;
                }

                // else, if token is not null, validate it
                var validation = jwtUtils.ValidateToken(token);

                // return the validation response
                if (validation.IsValid)
                {
                    await _next(context);
                }
                else
                {
                    context.Response.StatusCode = (int)validation.StatusCode;
                    context.Response.ContentType = contentType;

                    var responseMessage = new
                    {
                        message = validation.Message,
                        statusCode = (int)validation.StatusCode
                    };

                    var responseBody = JsonConvert.SerializeObject(responseMessage);
                    await context.Response.WriteAsync(responseBody, Encoding.UTF8);
                }
            }
        }
    }
}

