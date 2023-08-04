using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetUsersResponse : ResponseBase
    {
        public IEnumerable<ApplicationUser>? Users { get; set; } = new List<ApplicationUser>();

    }
}
