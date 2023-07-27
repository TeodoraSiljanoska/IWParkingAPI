using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IWParkingAPI.Models.Data
{
    [Table("ApplicationRoles", Schema = "ParkingDB")]
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() 
        {

        }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeModified { get; set; }
    }
}
