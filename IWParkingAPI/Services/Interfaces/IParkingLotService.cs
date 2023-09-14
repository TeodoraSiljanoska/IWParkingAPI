using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IParkingLotService
    {
        public AllParkingLotResponse GetAllParkingLots(int pageNumber, int pageSize, FilterParkingLotRequest request);
        public ParkingLotResponse GetParkingLotById(int id);
        public ResponseBase DeactivateParkingLot(int id);
        public ResponseBase CreateParkingLot(ParkingLotReq request);
        public ResponseBase UpdateParkingLot(int id, UpdateParkingLotRequest changes);
        public ResponseBase MakeParkingLotFavorite(int parkingLotId);
        public ResponseBase RemoveParkingLotFavourite(int parkingLotId);
        public AllFavouriteParkingLotsResponse GetUserFavouriteParkingLots(int pageNumber, int pageSize);
    }
}
