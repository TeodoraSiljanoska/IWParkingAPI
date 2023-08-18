﻿using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.Models.Enums
{
    public class Enums
    {
        public enum Status
        {
            [Display(Name ="Pending")]
            Pending = 1,

            [Display(Name = "Approved")]
            Approved = 2,

            [Display(Name = "Declined")]
            Declined = 3,
        }

        public enum RequestType
        {
            [Display(Name = "Activate")]
            Activate = 1,

            [Display(Name = "Deactivate")]
            Deactivate = 2
        }
    }
}
