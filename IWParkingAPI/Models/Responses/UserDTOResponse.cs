using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class UserDTOResponse : ResponseBase
    {
        public UserDTO User { get; set; }
    }
}
