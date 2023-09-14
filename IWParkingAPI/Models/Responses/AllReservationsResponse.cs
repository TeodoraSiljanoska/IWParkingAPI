using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllReservationsResponse : ResponseBase
    {
        public IEnumerable<ReservationDTO>? Reservations { get; set; } = new List<ReservationDTO>();
        public int NumPages { get; set; }
    }
}
