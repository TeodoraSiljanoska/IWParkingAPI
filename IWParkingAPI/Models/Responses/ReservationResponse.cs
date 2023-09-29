using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class ReservationResponse : ResponseBase
    {
        public ReservationWithParkingLotDTO Reservation { get; set; } = new ReservationWithParkingLotDTO();
    }
}
