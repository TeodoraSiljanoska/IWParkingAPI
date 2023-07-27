using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class RoleResponse : ResponseBase
    {
        public ApplicationRole Role { get; set; } = new ApplicationRole();
    }
}
