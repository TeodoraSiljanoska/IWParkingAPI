namespace IWParkingAPI.CustomExceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message)
        {

        }
    }
}

