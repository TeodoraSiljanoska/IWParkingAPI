using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllParkingLotsResponse : ResponseBase
    {
        public IEnumerable<ParkingLotWithFavouritesDTO>? ParkingLots { get; set; } = new List<ParkingLotWithFavouritesDTO>();
        public int NumPages { get; set; }
    }
}
