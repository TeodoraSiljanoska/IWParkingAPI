using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class RoleResponse : ResponseBase
    {
        public RoleDTO Role { get; set; } = new RoleDTO();
    }
}
