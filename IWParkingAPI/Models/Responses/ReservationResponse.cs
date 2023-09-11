using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class ReservationResponse : ResponseBase
    {
        public ReservationDTO Reservation { get; set; } = new ReservationDTO();
    }
}
