using System.Net;

namespace IWParkingAPI.Models.Responses
{
    public class ResponseBase
    {
            public List<string> Errors { get; set; } = new List<string>();
            public HttpStatusCode StatusCode { get; set; }
        
    }
}
