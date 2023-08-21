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
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpGet("GetAll")]
        public AllRequestsResponse GetRequests()
        {
            return _requestService.GetAllRequests();
        }

        [AuthorizeCustom(UserRoles.SuperAdmin)]
        [HttpPut("Modify/{id}")]
        [Validate]
        public RequestResponse ModifyRequest(int id, RequestRequest request)
        {
            return _requestService.ModifyRequest(id, request);
        }

    }
}
