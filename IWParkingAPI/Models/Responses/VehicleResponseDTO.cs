﻿using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class VehicleResponseDTO : ResponseBase
    {
        public VehicleDTO Vehicle { get; set; } = new VehicleDTO();

    }
}
