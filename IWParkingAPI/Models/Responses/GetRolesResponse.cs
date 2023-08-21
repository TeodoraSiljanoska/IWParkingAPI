using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Models.Responses.Dto
{
    public class GetRolesResponse : ResponseBase
    {
        public IEnumerable<ApplicationRole>? Roles { get; set; } = new List<ApplicationRole>();
    }
}
