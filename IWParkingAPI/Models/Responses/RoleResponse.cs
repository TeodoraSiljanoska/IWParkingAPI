namespace IWParkingAPI.Models.Responses
{
    public class RoleResponse : ResponseBase
    {
        public AspNetRole Role { get; set; } = new AspNetRole();
    }
}
