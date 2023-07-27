using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<ApplicationUser> GetAllUsers();
        UserResponse GetUserById(int id);
        UserResponse CreateUser(UserRequest request);
        UserResponse UpdateUser(int id, UserRequest changes);
        UserResponse DeleteUser(int id);
    }
}
