using IWParkingAPI.Services.Interfaces;

namespace IWParkingAPI.Services.Implementation
{
    public class LocalTimeExtension : ILocalTimeExtension
    {
        public DateTime GetLocalTime()
        {
            TimeZoneInfo cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            DateTime serverTime = DateTime.UtcNow;
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(serverTime, cetTimeZone);
            return localTime;
        }
    }
}
