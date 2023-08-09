using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
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
        [HttpPost("Create")]
        public ParkingLotResponse CreateParkingLot(ParkingLotReq request)
        {
            return _parkingLotService.CreateParkingLot(request);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]

        [HttpPost("Deactivate/{id}")]
        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            return _parkingLotService.DeactivateParkingLot(id)
;
        }

        [HttpPost("MakeParkingLotFavourite/{userId,parkingLotId}")]
        [AuthorizeCustom(UserRoles.Owner,UserRoles.User)]
        public Task <ParkingLotResponse> MakeParkingLotFavoriteAsync(int userId, int parkingLotId)
        {
            return _parkingLotService.MakeParkingLotFavoriteAsync(userId, parkingLotId);
        }
    }
}
