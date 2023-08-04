namespace IWParkingAPI.Models.Responses
{
    public class ParkingLotResponse : ResponseBase
    {
        public ParkingLot ParkingLot { get; set; } = new ParkingLot();
    }
}
