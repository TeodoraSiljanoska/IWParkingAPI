using IWParkingAPI.Fluent_Validations;
using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Models.Responses.Dto;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("GetAll")]
        public GetParkingLotsResponse GetParkingLots()
        {
            return _parkingLotService.GetAllParkingLots();
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

        [HttpDelete("Deactivate/{id}")]
        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            return _parkingLotService.DeactivateParkingLot(id);
        }

        [HttpDelete("RemoveParkingLotFavourite/{userId},{parkingLotId}")]
        [AuthorizeCustom(UserRoles.User)]
        public ParkingLotResponse RemoveParkingLotFavourite(int userId, int parkingLotId)
        {
            return _parkingLotService.RemoveParkingLotFavourite(userId, parkingLotId);
        }

        [HttpPost("MakeParkingLotFavourite/{userId},{parkingLotId}")]
        [AuthorizeCustom(UserRoles.User)]
        public ParkingLotResponse MakeParkingLotFavorite(int userId, int parkingLotId)
        {
            return _parkingLotService.MakeParkingLotFavorite(userId, parkingLotId);
        }

        [HttpGet("GetUserFavouriteParkingLots/{userId}")]
        [AuthorizeCustom(UserRoles.User)]
        public GetParkingLotsDTOResponse GetUsereFavouriteParkingLots(int userId)
        {
            return _parkingLotService.GetUserFavouriteParkingLots(userId);
        }
    }
}
