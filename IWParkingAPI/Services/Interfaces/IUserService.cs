using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        public AllUsersResponse GetAllUsers();
        public UserResponse GetUserById();
        public UserResponse UpdateUser(UpdateUserRequest changes);
        public UserResponse DeactivateUser();
        public UserResponse DeactivateUserAdmin(int id);
    }
}
