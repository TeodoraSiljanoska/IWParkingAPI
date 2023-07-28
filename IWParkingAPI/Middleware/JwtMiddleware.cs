using IWParkingAPI.Utilities;

namespace IWParkingAPI.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            if (context.Request.Path == "/api/User/Login" || context.Request.Path == "/api/User/Register")
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            bool response = jwtUtils.ValidateToken(token);

            if (response)
            {
                await _next(context);
            }
        }
    }
}
