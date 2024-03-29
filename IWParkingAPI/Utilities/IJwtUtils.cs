﻿using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Utilities
{
    public interface IJwtUtils
    {
        Task<UserLoginResponse> GenerateToken(ApplicationUser model);
        public TokenValidationResponse ValidateToken(string token);
    }
}
