namespace DomainService.Interfaces.PermissionManagement;

public interface ISysUserRolesService
{
    Task<object> GetUserRoles(Guid currentUserId, string currentUserName, Guid userId);
    Task<object> CreateUserRoles(Guid currentUserId, string currentUserName, Guid userId, List<Guid> roleIds);
    Task<object> DeleteUserRoles(Guid currentUserId, string currentUserName, List<Guid> ids);
}
