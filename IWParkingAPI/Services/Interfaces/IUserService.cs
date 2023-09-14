using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        public AllUsersResponse GetAllUsers(int pageNumber, int pageSize);
        public UserResponse GetUserById();
        public UserResponse UpdateUser(UpdateUserRequest changes);
        public UserResponse DeactivateUser();
        public ResponseBase DeactivateUserAdmin(int id);
    }
}
