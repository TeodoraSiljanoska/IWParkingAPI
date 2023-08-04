using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IRoleService
    {
      public  GetRolesResponse GetAllRoles();
      public  RoleResponse GetRoleById(int id);
      public  RoleResponse CreateRole(RoleRequest request);
      public  RoleResponse UpdateRole(int id, RoleRequest changes);
      public  RoleResponse DeleteRole(int id);
    }
}
