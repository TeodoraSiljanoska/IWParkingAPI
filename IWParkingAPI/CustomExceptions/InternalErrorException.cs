namespace IWParkingAPI.CustomExceptions
{
    public class InternalErrorException : ApplicationException
    {
        public InternalErrorException(string message) : base(message)
        {

        }
    }
}

