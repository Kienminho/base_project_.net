using Model.RequestModel.PermissionManagement;

namespace DomainService.Interfaces.PermissionManagement;

public interface ISysActivityService
{
    Task<object> GetActivities(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize);
    Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id);
    Task<object> Create(Guid currentUserId, string currentUserName, SysActivityRequest req);
    Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysActivityRequest req);
    Task<object> Delete(Guid currentUserId, string currentUserName, Guid id);
    Task<bool> ValidateActivities(List<Guid> activityIds);
}
