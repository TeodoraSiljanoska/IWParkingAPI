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
    [AuthorizeCustom(UserRoles.SuperAdmin)]
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

        [HttpGet("Get/{id}")]
        public ZoneResponse GetById(int id)
        {
            return _zoneService.GetZoneById(id);
        }

        [HttpPost("Create")]
        [Validate]
        public ZoneResponse Create(ZoneRequest request)
        {
            return _zoneService.CreateZone(request);
        }

        [HttpPut("Update/{id}")]
        [Validate]
        public ZoneResponse Update(int id, ZoneRequest changes)
        {
            return _zoneService.UpdateZone(id, changes);
        }

        [HttpDelete("Delete/{id}")]
        public ZoneResponse Delete(int id)
        {
            return _zoneService.DeleteZone(id);
        }
    }
}
