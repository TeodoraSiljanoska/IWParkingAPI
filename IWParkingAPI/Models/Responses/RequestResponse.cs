﻿namespace IWParkingAPI.Models.Responses
{
    public class RequestResponse : ResponseBase
    {
        public RequestDTO Request { get; set; } = new RequestDTO();
    }
}
