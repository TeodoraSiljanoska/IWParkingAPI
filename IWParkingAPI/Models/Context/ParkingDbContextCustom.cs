using IWParkingAPI.Models.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IWParkingAPI.Models.Context
{
    
        public class ParkingDbContextCustom : IdentityDbContext<ApplicationUser, ApplicationRole, int>
        {

            public ParkingDbContextCustom(DbContextOptions options) : base(options)
            {
            }

            protected ParkingDbContextCustom()
            {
            }
            public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
            public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }

    }
}

