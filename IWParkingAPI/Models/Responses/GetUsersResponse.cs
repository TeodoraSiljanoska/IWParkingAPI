using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetUsersResponse : ResponseBase
    {
        public IEnumerable<AspNetUser>? Users { get; set; } = new List<AspNetUser>();

    }
}
