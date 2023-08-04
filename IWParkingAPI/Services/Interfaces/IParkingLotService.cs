using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IParkingLotService
    {
        public GetParkingLotsResponse GetAllParkingLots();
    }
}
