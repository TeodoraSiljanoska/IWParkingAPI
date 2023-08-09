using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class ParkingLotResponse : ResponseBase
    {
        public ParkingLotDTO ParkingLot { get; set; } = new ParkingLotDTO();
    }
}
