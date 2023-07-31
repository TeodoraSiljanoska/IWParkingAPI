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
            if (context.Request.Path == "/api/Auth/Login" || context.Request.Path == "/api/Auth/Register" || context.Request.Path == "/api/Auth/Reset-Password")
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                await _next(context);
                return;
            }

            bool response = jwtUtils.ValidateToken(token);

            if (response)
            {
                await _next(context);
                return;
            }
        }
    }
}
