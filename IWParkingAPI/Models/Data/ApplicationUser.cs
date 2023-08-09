using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
            [JsonIgnore]
            public bool? IsDeactivated { get; set; }
            [JsonIgnore]
            public DateTime TimeCreated { get; set; }
            [JsonIgnore]
            public DateTime? TimeModified { get; set; }

    }
}

