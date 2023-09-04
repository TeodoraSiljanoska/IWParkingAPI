using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllParkingLotResponse : ResponseBase
    {
        public IEnumerable<ParkingLotWithAvailableCapacityDTO>? ParkingLots { get; set; } = new List<ParkingLotWithAvailableCapacityDTO>();
        public int NumPages { get; set; }
    }
}
