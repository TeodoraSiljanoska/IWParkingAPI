using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Models.Responses.Dto
{
    public class GetUsersDTOResponse : ResponseBase
    {
        public IEnumerable<UserDTO>? Users { get; set; } = new List<UserDTO>();
    }
}
