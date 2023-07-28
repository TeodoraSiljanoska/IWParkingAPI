using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class UserResponse : ResponseBase
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();    
    }
}

