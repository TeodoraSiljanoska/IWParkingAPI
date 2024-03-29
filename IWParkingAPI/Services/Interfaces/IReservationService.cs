﻿using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        public ReservationResponse MakeReservation(MakeReservationRequest request);
        public ReservationResponse CancelReservation(int  reservationId);
        public ReservationResponse ExtendReservation(int reservationId, ExtendReservationRequest request);
        public AllReservationsResponse GetUserReservations(int pageNumber, int pageSize);
    }
}
