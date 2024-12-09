using Common.Authorization.Utils;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.PermissionManagement;
using Entity.Entities.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.ResponseModel.PermissionManagement;

namespace Infrastructure.Implements.PermissionManagement
{
    public class SysUserRolesService(IUnitOfWork unitOfWork, IMemoryCache memoryCache) : BaseService(unitOfWork, memoryCache), ISysUserRolesService
    {
        public async Task<object> CreateUserRoles(Guid currentUserId, string currentUserName, Guid userId, List<Guid> roleIds)
        {
            var roles = await _unitOfWork.Repository<SysRole>()
                                              .Where(a => a.IsDeleted != true && roleIds.Contains(a.Id))
                                              .Select(a => a.Id)
                                              .ToListAsync();

            var roleNotExists = roleIds.Except(roles).ToList();
            if (roleNotExists.Any())
                throw new KeyNotFoundException($"Activity with id {string.Join(", ", roleNotExists)} doesn't exists");

            var existUserRoles = await _unitOfWork.Repository<SysUserRole>()
                                                  .Where(ur => ur.IsDeleted != true && ur.UserId == userId && roleIds.Contains(ur.RoleId)).Select(ur => ur.RoleId)
                                                  .ToListAsync();

            if (existUserRoles.Count > 0)
                throw new KeyExistsException($"User with id {userId} already has role with id {string.Join(", ", existUserRoles)}");

            var newUserRoles = roleIds.Select(roleId => new SysUserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId,
                CreatedName = currentUserName
            }).ToList();

            _unitOfWork.Repository<SysUserRole>().AddRange(newUserRoles);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> DeleteUserRoles(Guid currentUserId, string currentUserName, List<Guid> ids)
        {
            var userRoles = await _unitOfWork.Repository<SysUserRole>().Where(ur => ur.IsDeleted != true && ids.Contains(ur.Id)).ToListAsync();

            if (userRoles.Count == 0)
                throw new KeyNotFoundException($"User role with id {string.Join(", ", ids)} doesn't exists");

            userRoles.ForEach(ur =>
            {
                ur.IsDeleted = true;
                ur.UpdatedDate = DateTime.Now;
                ur.UpdatedById = currentUserId;
                ur.Updater = currentUserName;
            });

            _unitOfWork.Repository<SysUserRole>().UpdateRange(userRoles);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> GetUserRoles(Guid currentUserId, string currentUserName, Guid userId)
        {
            var userRoles = await _unitOfWork.Repository<SysUserRole>().Where(ur => ur.IsDeleted != true && ur.UserId == userId)
                                              .Join(_unitOfWork.Repository<SysRole>(), ur => ur.RoleId, r => r.Id,
                                                   (ur, r) => new SysRoleResponse
                                                   {
                                                       Id = ur.Id,
                                                       RoleId = r.Id,
                                                       Name = r.Name,
                                                       Description = r.Description,
                                                       CreatedDate = r.CreatedDate,
                                                       Creator = r.CreatedName
                                                   })
                                              .OrderByDescending(r => r.CreatedDate)
                                              .ToListAsync();

            return Utils.CreateResponseModel(userRoles, userRoles.Count);
        }
    }
}
