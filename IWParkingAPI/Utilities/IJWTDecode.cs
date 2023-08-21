namespace IWParkingAPI.Utilities
{
    public interface IJWTDecode
    {
        public string ExtractUserIdFromToken();
        public string ExtractRoleFromToken();
    }
}
