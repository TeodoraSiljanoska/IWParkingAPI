using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Models.Responses
{
    public class GetUsersDTOResponse : ResponseBase
    {
        public IEnumerable<UserDTO>? Users { get; set; } = new List<UserDTO>();
    }
}
