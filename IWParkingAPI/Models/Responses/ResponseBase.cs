using System.Net;

namespace IWParkingAPI.Models.Responses
{
    public class ResponseBase
    {
        public string? Message { get; set; }
        public HttpStatusCode? StatusCode { get; set; } 
    }
}
