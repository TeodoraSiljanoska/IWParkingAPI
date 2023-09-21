using IWParkingAPI.Fluent_Validations;
using IWParkingAPI.Middleware.Authorization;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;
using IWParkingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IWParkingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ZoneController : Controller
    {
        private readonly IZoneService _zoneService;

        public ZoneController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        [HttpGet("GetAll")]
        public AllZonesResponse GetAll()
        {
            return _zoneService.GetAllZones();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpGet("Get/{id}")]
        public ZoneResponse GetById(int id)
        {
            return _zoneService.GetZoneById(id);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpPost("Create")]
        [Validate]
        public ZoneResponse Create(ZoneRequest request)
        {
            return _zoneService.CreateZone(request);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpPut("Update/{id}")]
        [Validate]
        public ZoneResponse Update(int id, ZoneRequest changes)
        {
            return _zoneService.UpdateZone(id, changes);
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpDelete("Delete/{id}")]
        public ZoneResponse Delete(int id)
        {
            return _zoneService.DeleteZone(id);
        }
    }
}
