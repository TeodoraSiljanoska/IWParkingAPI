using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IWParkingAPI.Models.Data
{
    
        [Table("ApplicationUsers", Schema = "ParkingDB")]
        public class ApplicationUser : IdentityUser<int>
        {
            public ApplicationUser()
            {

            }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        public bool? IsDeactivated { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime? TimeModified { get; set; }
    }
    }

