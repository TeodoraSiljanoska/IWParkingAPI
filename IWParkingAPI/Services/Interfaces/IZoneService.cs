using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IZoneService
    {
            public AllZonesResponse GetAllZones();
            public ZoneResponse GetZoneById(int id);
            public ZoneResponse CreateZone(ZoneRequest request);
            public ZoneResponse UpdateZone(int id, ZoneRequest changes);
            public ZoneResponse DeleteZone(int id);
    }
}
