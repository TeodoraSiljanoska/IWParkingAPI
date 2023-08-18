﻿using IWParkingAPI.Fluent_Validations;
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
        public VehicleResponseDTO Create(VehicleRequest request)
        {
            return _vehicleService.AddNewVehicle(request);
        }

        [AuthorizeCustom(UserRoles.User)]
        [Validate]
        [HttpPut("Update/{id}")]
        public VehicleResponseDTO Update(int id, UpdateVehicleRequest changes)
        {
            return _vehicleService.UpdateVehicle(id, changes);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpDelete("Delete/{id}")]
        public VehicleResponseDTO Delete(int id)
        {
            return _vehicleService.DeleteVehicle(id);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpGet("Get/{id}")]
        public VehicleResponseDTO GetVehicleById(int id)
        {
            return _vehicleService.GetVehicleById(id);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpPost("MakePrimary/{userId},{vehicleId}")]
        public MakeVehiclePrimaryResponse MakePrimary(int userId, int vehicleId)
        {
            return _vehicleService.MakeVehiclePrimary(userId, vehicleId);
        }

        [AuthorizeCustom(UserRoles.User)]
        [HttpGet("GetByUserId")]
        public GetAllVehiclesByUserIdResponse GetVehiclesByUserId()
        {
            return _vehicleService.GetVehiclesByUserId();
        }
    }
}

