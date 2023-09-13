using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost("Make")]
        [AuthorizeCustom(UserRoles.User)]
        public ReservationResponse MakeReservation(MakeReservationRequest request)
        {
           return _reservationService.MakeReservation(request); 
        }

        [HttpGet("Cancel/{id}")]
        [AuthorizeCustom(UserRoles.User)]
        public ReservationResponse CancelReservation(int id)
        {
            return _reservationService.CancelReservation(id);
        }

        [HttpPut("ExtendReservation/{id}")]
        [AuthorizeCustom(UserRoles.User)]
        public ReservationResponse ExtendReservation(int id, ExtendReservationRequest request)
        {
            return _reservationService.ExtendReservation(id, request);
        }

    }
}
