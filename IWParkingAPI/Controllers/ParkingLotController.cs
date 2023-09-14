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
        public AllParkingLotResponse GetParkingLots(int pageNumber, int pageSize, FilterParkingLotRequest request)
        {
            return _parkingLotService.GetAllParkingLots(pageNumber, pageSize, request);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin, UserRoles.Owner, UserRoles.User)]
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

        [AuthorizeCustom(UserRoles.Owner, UserRoles.SuperAdmin)]
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
        public AllFavouriteParkingLotsResponse GetUsereFavouriteParkingLots(int pageNumber, int pageSize)
        {
            return _parkingLotService.GetUserFavouriteParkingLots(pageNumber, pageSize);
        }
    }
}
