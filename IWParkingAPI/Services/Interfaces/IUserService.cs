using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        public GetUsersDTOResponse GetAllUsers();
        public UserDTOResponse GetUserById(int id);
        public UserDTOResponse UpdateUser(int id, UpdateUserRequest changes);
        public UserDTOResponse DeactivateUser(int id);
    }
}
