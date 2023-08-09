using IWParkingAPI.Models.Data;

namespace IWParkingAPI.Models.Responses
{
    public class UserRegisterResponse : ResponseBase
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();

    }
}
