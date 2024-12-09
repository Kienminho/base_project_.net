using Model.RequestModel.PermissionManagement;

namespace DomainService.Interfaces.PermissionManagement;

public interface ISysUserActivitiesService
{
    Task<object> GetUserActivities(Guid currentUserId, string currentUserName, Guid userId);
    Task<object> CreateUserActivities(Guid currentUserId, string currentUserName, Guid userId, List<SysRoleActivityRequest> req);
    Task<object> UpdateUserActivities(Guid currentUserId, string currentUserName, List<SysRoleActivityRequest> req);
    Task<object> DeleteUserActivities(Guid currentUserId, string currentUserName, List<Guid> ids);
    Task<List<UserPermissionBaseResponse>> GetPermissionOfUser(Guid currentUserId);
}
