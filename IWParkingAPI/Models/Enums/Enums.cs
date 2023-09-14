using System.ComponentModel;
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

        public enum RequestType
        {
            [Display(Name = "Activate")]
            Activate = 1,

            [Display(Name = "Deactivate")]
            Deactivate = 2,

            [Display(Name = "Update")]
            Update = 3
        }

        public enum ParkingLotStatus
        {
            [Display(Name = "Activated")]
            Activated = 1,

            [Display(Name = "Deactivated")]
            Deactivated = 2
        }

        /*[Display(Name = "Deactivated")]
            Deactivated = 1,

            [Display(Name = "Activated")]
            Activated = 2,
            [Display(Name = "Updated")]
            Updated = 3 */

        public enum VehicleTypes
        {
            [Display(Name = "Car")]
            Car = 1,

            [Display(Name = "Adapted Car")]
            AdaptedCar = 2,

            [Display(Name = "Truck")]
            Truck = 3
        }

        public enum ReservationTypes
        {
            [Display(Name = "Successful")]
            Successful = 1,

            [Display(Name = "Cancelled")]
            Cancelled = 2
        }
    }
}
