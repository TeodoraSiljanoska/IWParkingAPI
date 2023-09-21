using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class ParkingLotResponse : ResponseBase
    {
        public ParkingLotWithAvailableCapacityDTO ParkingLot { get; set; } = new ParkingLotWithAvailableCapacityDTO();
    }
}
