using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWParkingAPI.Middleware.Authorization
{
    public class CustomUnauthorizedResult : JsonResult
    {
        public CustomUnauthorizedResult(HttpStatusCode statusCode, string message) : base(new ErrorResponse(statusCode, message))
        {
            StatusCode = (int)statusCode;
        }
    }

    public class ErrorResponse
    {
        public string Message { get; }
        public HttpStatusCode StatusCode { get; }

        public ErrorResponse(HttpStatusCode statusCode ,string message)
        {
            Message = message;
            StatusCode = statusCode;
        }
    }

}
