using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllReservationsResponse : ResponseBase
    {
        public IEnumerable<ReservationWithParkingLotDTO>? Reservations { get; set; } = new List<ReservationWithParkingLotDTO>();
        public int NumPages { get; set; }
    }
}
