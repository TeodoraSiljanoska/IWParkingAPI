using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        public GetUsersResponse GetAllUsers();
        public UserResponse GetUserById(int id);
        public UserResponse UpdateUser(int id, UpdateUserRequest changes);
        public UserResponse DeactivateUser(int id);
    }
}
