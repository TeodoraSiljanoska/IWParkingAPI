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
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpGet("GetAll")]
        public GetVehiclesResponse GetVehicles()
        {
            return _vehicleService.GetAllVehicles();
        }

        [AuthorizeCustom(UserRoles.User)]
        [Validate]
        [HttpPost("Create")]
        public VehicleDTOResponse Create(VehicleRequest request)
        {
            return _vehicleService.AddNewVehicle(request);
        }

        [AuthorizeCustom(UserRoles.User)]
        [Validate]
        [HttpPut("Update/{id}")]
        public VehicleDTOResponse Update(int id, UpdateVehicleRequest changes)
        {
            return _vehicleService.UpdateVehicle(id, changes);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpDelete("Delete/{id}")]
        public VehicleDTOResponse Delete(int id)
        {
            return _vehicleService.DeleteVehicle(id);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpGet("Get/{id}")]
        public VehicleDTOResponse GetVehicleById(int id)
        {
            return _vehicleService.GetVehicleById(id);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpPost("MakePrimary/{vehicleId}")]
        public MakeVehiclePrimaryResponse MakePrimary(int vehicleId)
        {
            return _vehicleService.MakeVehiclePrimary(vehicleId);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpGet("GetByUserId")]
        public AllVehiclesByUserResponse GetVehiclesByUserId()
        {
            return _vehicleService.GetVehiclesByUserId();
        }
    }
}

