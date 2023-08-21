﻿using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class GetAllParkingLotRequestsResponse : ResponseBase
    {
        public IEnumerable<RequestDTO>? Requests { get; set; } = new List<RequestDTO>();
    }
}
