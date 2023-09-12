using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class MakeReservationResponse : ResponseBase
    {
        public ReservationDTO Reservation { get; set; } = new ReservationDTO();
    }
}
