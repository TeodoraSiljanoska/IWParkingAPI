using IWParkingAPI.Fluent_Validations;
using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotService;
        public ParkingLotController(IParkingLotService parkingLotService)
        {
            _parkingLotService = parkingLotService;
        }

        [HttpPost("GetAll")]
        public AllParkingLotsResponse GetParkingLots(int pageNumber, int pageSize, FilterParkingLotRequest request)
        {
            return _parkingLotService.GetAllParkingLots(pageNumber, pageSize, request);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]

        [HttpGet("Get/{id}")]
        public ParkingLotResponse GetParkingLotById(int id)
        {
            return _parkingLotService.GetParkingLotById(id);
        }

        [AuthorizeCustom(UserRoles.Owner)]
        [Validate]
        [HttpPost("Create")]
        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            return _parkingLotService.CreateParkingLot(request);
        }

        [AuthorizeCustom(UserRoles.Owner)]
        [Validate]
        [HttpPut("Update/{id}")]
        public ParkingLotResponse UpdateParkingLot(int id, UpdateParkingLotRequest request)
        {
            return _parkingLotService.UpdateParkingLot(id,request);
        }
        [AuthorizeCustom(UserRoles.SuperAdmin)]

        [AuthorizeCustom(UserRoles.Owner)]
        [HttpDelete("Deactivate/{id}")]
        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            return _parkingLotService.DeactivateParkingLot(id);
        }

        [HttpDelete("RemoveParkingLotFavourite/{parkingLotId}")]
        [AuthorizeCustom(UserRoles.User)]
        public ParkingLotResponse RemoveParkingLotFavourite(int parkingLotId)
        {
            return _parkingLotService.RemoveParkingLotFavourite(parkingLotId);
        }

        [HttpPost("MakeParkingLotFavourite/{parkingLotId}")]
        [AuthorizeCustom(UserRoles.User)]
        public ParkingLotResponse MakeParkingLotFavorite(int parkingLotId)
        {
            return _parkingLotService.MakeParkingLotFavorite(parkingLotId);
        }

        [HttpGet("GetUserFavouriteParkingLots")]
        [AuthorizeCustom(UserRoles.User)]
        public AllParkingLotsResponse GetUsereFavouriteParkingLots()
        {
            return _parkingLotService.GetUserFavouriteParkingLots();
        }

      /*  [HttpGet("FilterParkingLots")]
        public AllParkingLotsResponse FilterParkingLots(FilterParkingLotRequest request)
        {
            return _parkingLotService.FilterParkingLots(request);
        } */
    }
}
