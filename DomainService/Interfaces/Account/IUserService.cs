using Model.RequestModel.PermissionManagement;

namespace DomainService.Interfaces.Account
{
    public interface IUserService
    {
        Task<object> GetList(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize);
        Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id);
        Task<object> Create(Guid currentUserId, string currentUserName, SysAccountRequest req);
        Task<object> Update(Guid currentUserId, string currentUserName, Guid accountId, SysAccountRequest req);
        Task<object> Delete(Guid currentUserId, string currentUserName, Guid id);
        Task<object> GetInfoMine(Guid currentUserId, string currentUserName);
    }
}
