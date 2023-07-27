using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class UserResponse : ResponseBase
    {
        //public AspNetUser User { get; set; } = new AspNetUser();
        public ApplicationUser User { get; set; } = new ApplicationUser();
        
}
}

