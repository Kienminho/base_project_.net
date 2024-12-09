using Common.UnitOfWork.UnitOfWorkPattern;
using DomainService.Interfaces.Account;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.PermissionManagement;

namespace Infrastructure.Implements.Account
{
    public class UserService(IUnitOfWork unitOfWork, IMemoryCache memoryCache) : BaseService(unitOfWork, memoryCache), IUserService
    {
        public Task<object> Create(Guid currentUserId, string currentUserName, SysAccountRequest req)
        {
            throw new NotImplementedException();
        }

        public Task<object> Delete(Guid currentUserId, string currentUserName, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetInfoMine(Guid currentUserId, string currentUserName)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetList(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<object> Update(Guid currentUserId, string currentUserName, Guid accountId, SysAccountRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
