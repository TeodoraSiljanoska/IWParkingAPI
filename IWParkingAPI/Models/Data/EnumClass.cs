using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IWParkingAPI.Models.Data
{
    public class EnumClass
    {
        public enum StatusEnum
        {
            [Display(Name = "Pending")]
            Pending = 1,
            [Display(Name = "Approved")]
            Approved = 2,
            [Display(Name = "Declined")]
            Declined = 3
        }
    }
}
