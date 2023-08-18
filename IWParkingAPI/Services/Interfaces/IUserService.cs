using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        public GetUsersDTOResponse GetAllUsers();
        public UserDTOResponse GetUserById();
        public UserDTOResponse UpdateUser(UpdateUserRequest changes);
        public UserDTOResponse DeactivateUser();
        public UserDTOResponse DeactivateUserAdmin(int id);
    }
}
