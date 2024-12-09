using Model.RequestModel.PermissionManagement;

namespace DomainService.Interfaces.PermissionManagement;

public interface ISysRoleService
{
    Task<object> GetRoles(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize);
    Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id);
    Task<object> Create(Guid currentUserId, string currentUserName, SysRoleRequest req);
    Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysRoleRequest req);
    Task<object> Delete(Guid currentUserId, string currentUserName, Guid id);
}
