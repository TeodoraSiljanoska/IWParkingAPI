using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class GetRolesResponse : ResponseBase
    {
        public IEnumerable<ApplicationRole>? Roles { get; set; } = new List<ApplicationRole>();
    }
}
