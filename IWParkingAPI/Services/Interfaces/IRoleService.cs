using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Services.Interfaces
{
    public interface IRoleService
    {
        IEnumerable<ApplicationRole> GetAllRoles();
        RoleResponse GetRoleById(int id);
        RoleResponse CreateRole(RoleRequest request);
        RoleResponse UpdateRole(int id, RoleRequest changes);
        RoleResponse DeleteRole(int id);
    }
}
