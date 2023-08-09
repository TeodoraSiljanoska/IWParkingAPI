using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IParkingLotService
    {
        public GetParkingLotsResponse GetAllParkingLots();
        public ParkingLotResponse GetParkingLotById(int id);
        public ParkingLotResponse DeactivateParkingLot(int id);
        public ParkingLotResponse CreateParkingLot(ParkingLotReq request);
        public Task<ParkingLotResponse> MakeParkingLotFavoriteAsync(int userId, int parkingLotId);

    }
}
