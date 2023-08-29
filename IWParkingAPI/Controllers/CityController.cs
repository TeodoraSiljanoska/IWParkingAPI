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
    [AuthorizeCustom(UserRoles.SuperAdmin)]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet("GetAll")]
        public AllCitiesResponse GetAll()
        {
            return _cityService.GetAllCities();
        }

        [HttpGet("Get/{id}")]
        public CityResponse GetById(int id)
        {
            return _cityService.GetCityById(id);
        }

        [HttpPost("Create")]
        [Validate]
        public CityResponse CreateCity(CityRequest request)
        {
            return _cityService.CreateCity(request);
        }

        [HttpPut("Update/{id}")]
        [Validate]
        public CityResponse Update(int id, CityRequest changes)
        {
            return _cityService.UpdateCity(id, changes);
        }

        [HttpDelete("Delete/{id}")]
        public CityResponse Delete(int id)
        {
            return _cityService.DeleteCity(id);
        }
    }
}
