using IWParkingAPI.CustomExceptions;
using Newtonsoft.Json;
using System.Net;

namespace IWParkingAPI.Middleware.Exceptions
{
    public class ExceptionMiddleware
    {
            private readonly RequestDelegate _next;
            private ILogger<ExceptionMiddleware> _logger;

            public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
            {
                this._next = next;
                this._logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(context, ex);
                }
            }

            public Task HandleExceptionAsync(HttpContext context, Exception ex)
            {
                context.Response.ContentType = "application/json";
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
                var errorDetails = new ResponseBase();

                switch (ex)
                {
                    case BadRequestException:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E400;
                        statusCode = HttpStatusCode.BadRequest;
                        break;
                    case InternalErrorException:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E500;
                        statusCode = HttpStatusCode.InternalServerError;
                        break;
                    case UnauthorizedException:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E401;
                        statusCode = HttpStatusCode.Unauthorized;
                        break;
                    case ForbiddenException:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E403;
                        statusCode = HttpStatusCode.Forbidden;
                        break;
                    case NotFoundException:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E404;
                        statusCode = HttpStatusCode.NotFound;
                        break;
                    default:
                        errorDetails.Errors.Add(ex.Message);
                        errorDetails.ErrorType = ErrorMapper.E500;
                        statusCode = HttpStatusCode.InternalServerError;
                        break;
                }
                errorDetails.StatusCode = statusCode;
                string response = JsonConvert.SerializeObject(errorDetails);
                context.Response.StatusCode = (int)statusCode;
                return context.Response.WriteAsync(response);
            }
    }
}
