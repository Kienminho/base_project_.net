using Common.Authorization.Utils;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.PermissionManagement;
using Entity.Entities.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.PermissionManagement;
using Model.ResponseModel.PermissionManagement;

namespace Infrastructure.Implements.PermissionManagement
{
    public class SysRoleActivitiesService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, ISysActivityService _sysActivityService) : BaseService(unitOfWork, memoryCache), ISysRoleActivitiesService
    {
        public async Task<object> CreateRoleActivities(Guid currentUserId, string currentUserName, Guid roleId, List<SysRoleActivityRequest> req)
        {
            var activityIds = req.Select(r => r.Id).ToList(); // Get all activity ids from request
            _ = await _sysActivityService.ValidateActivities(activityIds); // Validate all activity ids

            var existRoleActivities = await _unitOfWork.Repository<SysRoleActivity>()
                                                        .Where(ra => ra.IsDeleted != true && ra.RoleId == roleId &&
                                                               activityIds.Contains(ra.ActivityId))
                                                        .Select(ra => ra.ActivityId)
                                                        .ToListAsync();

            if (existRoleActivities.Any())
                throw new KeyExistsException($"Role with id {roleId} already has activity with id {string.Join(", ", existRoleActivities)}");

            var newRoleActivities = req.Select(r => new SysRoleActivity
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                ActivityId = r.Id,
                C = r.C,
                R = r.R,
                U = r.U,
                D = r.D,
                CreatedById = currentUserId,
                CreatedDate = DateTime.Now,
                CreatedName = currentUserName
            }).ToList();

            await _unitOfWork.Repository<SysRoleActivity>().AddRangeAsync(newRoleActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> DeleteRoleActivities(Guid currentUserId, string currentUserName, List<Guid> ids)
        {
            var roleActivities = await _unitOfWork.Repository<SysRoleActivity>().Where(ra => ra.IsDeleted != true && ids.Contains(ra.Id)).ToListAsync();

            if (roleActivities.Count == 0)
                throw new KeyNotFoundException($"Role activities with id {string.Join(", ", ids)} doesn't exists");

            roleActivities.ForEach(ra =>
            {
                ra.IsDeleted = true;
                ra.UpdatedById = currentUserId;
                ra.UpdatedDate = DateTime.Now;
                ra.Updater = currentUserName;
            });

            _unitOfWork.Repository<SysRoleActivity>().UpdateRange(roleActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> GetRoleActivities(Guid currentUserId, string currentUserName, Guid roleId)
        {
            var role = await _unitOfWork.Repository<SysRole>().FirstOrDefaultAsync(r => r.Id == roleId && r.IsDeleted != true)
                             ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Role"));

            var roleActivities = await _unitOfWork.Repository<SysRoleActivity>()
                                        .Where(ra => ra.RoleId == roleId && ra.IsDeleted != true)
                                        .Join(_unitOfWork.Repository<SysActivity>(), ra => ra.ActivityId, a => a.Id,
                                             (ra, a) => new
                                             {
                                                 RoleId = ra.RoleId,
                                                 Activity = new SysRoleActivitiesResponse
                                                 {
                                                     Id = ra.Id,
                                                     ActivityId = a.Id,
                                                     ActivityName = a.Name,
                                                     C = ra.C,
                                                     R = ra.R,
                                                     U = ra.U,
                                                     D = ra.D
                                                 }
                                             }).ToListAsync();

            var res = new
            {
                RoleId = roleId,
                RoleName = role.Name,
                Activities = roleActivities.Select(ra => ra.Activity).ToList()
            };

            return Utils.CreateResponseModel(res);
        }

        public async Task<object> UpdateRoleActivities(Guid currentUserId, string currentUserName, List<SysRoleActivityRequest> req)
        {
            var roleActivityIds = req.Select(r => r.Id).ToList(); // Get all activity ids from request

            var roleActivities = await _unitOfWork.Repository<SysRoleActivity>()
                                        .Where(ra => roleActivityIds.Contains(ra.Id))
                                        .ToListAsync();

            if (roleActivities.Count == 0)
                throw new KeyNotFoundException($"Role activities {string.Join(", ", roleActivityIds)} not found");

            roleActivities.ForEach(ra =>
            {
                var activity = req.FirstOrDefault(r => r.Id == ra.Id);
                if (activity != null)
                {
                    ra.C = activity.C;
                    ra.R = activity.R;
                    ra.U = activity.U;
                    ra.D = activity.D;
                    ra.UpdatedById = currentUserId;
                    ra.UpdatedDate = DateTime.Now;
                    ra.Updater = currentUserName;
                }
            });

            _unitOfWork.Repository<SysRoleActivity>().UpdateRange(roleActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }
    }
}
