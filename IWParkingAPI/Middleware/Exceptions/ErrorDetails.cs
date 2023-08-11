namespace IWParkingAPI.Middleware.Exceptions
{
    public class ErrorDetails
    {
        public int HttpStatus { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
        public string StatusCode { get; set; }
    }
}
