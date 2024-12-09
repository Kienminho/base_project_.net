using Model.RequestModel.PermissionManagement;

namespace DomainService.Interfaces.PermissionManagement;

public interface ISysRoleActivitiesService
{
    Task<object> GetRoleActivities(Guid currentUserId, string currentUserName, Guid roleId);
    Task<object> CreateRoleActivities(Guid currentUserId, string currentUserName, Guid roleId, List<SysRoleActivityRequest> req);
    Task<object> UpdateRoleActivities(Guid currentUserId, string currentUserName, List<SysRoleActivityRequest> req);
    Task<object> DeleteRoleActivities(Guid currentUserId, string currentUserName, List<Guid> ids);
}
