﻿namespace IWParkingAPI.Models.Responses.Dto
{
    public class VehicleWithUserDTO
    {
        public int Id { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string Type { get; set; } = null!;

        public int UserId { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime? TimeModified { get; set; }

        public bool? IsPrimary { get; set; }

        public virtual UserDTO User { get; set; } = null!;
    }
}
