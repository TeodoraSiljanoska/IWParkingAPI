using System.ComponentModel.DataAnnotations;

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
    }
}
