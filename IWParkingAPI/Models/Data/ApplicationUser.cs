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
        }
    }

