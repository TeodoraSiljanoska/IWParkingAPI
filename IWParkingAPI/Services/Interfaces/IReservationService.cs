using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        public MakeReservationResponse MakeReservation(MakeReservationRequest request);
    }
}
