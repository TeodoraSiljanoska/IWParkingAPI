using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterUser(UserRegisterRequest request);
        Task<UserLoginResponse> LoginUser(UserLoginRequest model);
        Task<ResponseBase> ChangePassword(UserResetPasswordRequest request);
        Task<ResponseBase> ChangeEmail(UserChangeEmailRequest request);
    }
}
