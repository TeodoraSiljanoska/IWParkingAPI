using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotsService;

        public ParkingLotController(IParkingLotService parkingLotsService)
        {
            _parkingLotsService = parkingLotsService;
        }

        [HttpGet("GetAll")]
        public GetParkingLotsResponse GetParkingLots()
        {
            return _parkingLotsService.GetAllParkingLots();
        }

        [HttpGet("Get/{id}")]
        public ParkingLotResponse GetParkingLotById(int id)
        {
            return _parkingLotsService.GetParkingLotById(id);
        }

        [HttpPost("Deactivate/{id}")]
        public ParkingLotResponse DeactivateParkingLot(int id)
        {
            return _parkingLotsService.DeactivateParkingLot(id)
;
        }
    }
}
