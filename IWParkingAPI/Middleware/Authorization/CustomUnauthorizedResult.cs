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
        public string message { get; }
        public HttpStatusCode statusCode { get; }

        public ErrorResponse(HttpStatusCode status, string mes)
        {
            message = mes;
            statusCode = status;
        }
    }

}
