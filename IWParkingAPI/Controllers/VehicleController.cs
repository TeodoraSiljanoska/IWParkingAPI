using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost("Create")]
        public VehicleResponse Create(VehicleRequest request)
        {
            return _vehicleService.AddNewVehicle(request);
        }

        [HttpPut("Update/{id}")]
        public VehicleResponse Update(int id, VehicleRequest changes)
        {
            return _vehicleService.UpdateVehicle(id, changes);
        }

        [HttpDelete("Delete/{id}")]
        public VehicleResponse Delete(int id)
        {
            return _vehicleService.DeleteVehicle(id);
        }
    }
}

