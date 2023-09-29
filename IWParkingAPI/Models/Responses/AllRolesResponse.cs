using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class AllRolesResponse : ResponseBase
    {
        public IEnumerable<RoleDTO>? Roles { get; set; } = new List<RoleDTO>();
    }
}
