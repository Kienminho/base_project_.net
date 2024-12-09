using Common.Authorization.Utils;
using Common.Constant;
using Common.Enum;
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
    public class SysUserActivitiesService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, ISysActivityService _sysActivityService) : BaseService(unitOfWork, memoryCache), ISysUserActivitiesService
    {
        public async Task<object> CreateUserActivities(Guid currentUserId, string currentUserName, Guid userId, List<SysRoleActivityRequest> req)
        {
            var reqActivitiIds = req.Select(a => a.Id).ToList();
            _ = _sysActivityService.ValidateActivities(reqActivitiIds);

            var existUserActivities = await _unitOfWork.Repository<SysUserActivity>()
                                                   .Where(ua => ua.IsDeleted != true && ua.UserId == userId && reqActivitiIds.Contains(ua.ActivityId))
                                                   .Select(ua => ua.Id)
                                                   .ToListAsync();

            if (existUserActivities.Any())
                throw new KeyExistsException($"User with id {userId} already has activity with id {string.Join(", ", existUserActivities)}");

            var newUserActivities = req.Select(r => new SysUserActivity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActivityId = r.Id,
                C = r.C,
                R = r.R,
                U = r.U,
                D = r.D,
                CreatedDate = DateTime.Now,
                CreatedById = currentUserId,
                CreatedName = currentUserName
            }).ToList();

            _unitOfWork.Repository<SysUserActivity>().AddRange(newUserActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res);
        }

        public async Task<object> DeleteUserActivities(Guid currentUserId, string currentUserName, List<Guid> ids)
        {
            var userActivities = await _unitOfWork.Repository<SysUserActivity>().Where(ua => ua.IsDeleted != true && ids.Contains(ua.Id)).ToListAsync();
            if (userActivities.Count == 0)
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "User Activities"));

            userActivities.ForEach(ua =>
            {
                ua.IsDeleted = true;
                ua.UpdatedDate = DateTime.Now;
                ua.UpdatedById = currentUserId;
                ua.Updater = currentUserName;
            });

            _unitOfWork.Repository<SysUserActivity>().UpdateRange(userActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<List<UserPermissionBaseResponse>> GetPermissionOfUser(Guid currentUserId)
        {
            var roleAdmin = await _unitOfWork.Repository<SysRole>()
                                             .FirstOrDefaultAsync(s => s.IsDeleted != true &&
                                                                 (s.RoleType != null &&
                                                                 s.RoleType == RoleType.Admin.GetValue<int>()))
                                  ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Role Admin"));

            var listpermission = new List<UserPermissionBaseResponse>();
            var listRoleOfUser = await _unitOfWork.Repository<SysUserRole>()
                .Where(s => s.UserId == currentUserId && s.IsDeleted != true)
                .ToListAsync();

            var haveAdminRole = listRoleOfUser.Any(s => s.RoleId == roleAdmin.Id);
            if (haveAdminRole)
            {
                var listActivity = await _unitOfWork.Repository<SysActivity>().Where(t => t.IsDeleted != true).ToListAsync();
                if (haveAdminRole)
                {
                    foreach (var item in listActivity)
                    {
                        listpermission.Add(new UserPermissionBaseResponse
                        {
                            ActivityId = item.Id,
                            ActivityName = item.Name,
                            Code = item.Code,
                            C = true,
                            R = true,
                            U = true,
                            D = true
                        });
                    }
                }
            }

            var listPermissionFromRoleOfUser = await _unitOfWork.Repository<SysUserRole>()
                .Where(s => s.UserId == currentUserId && s.IsDeleted != true)
                .Join(_unitOfWork.Repository<SysRoleActivity>().Where(ra => ra.IsDeleted != true),
                    usr => usr.RoleId, ract => ract.RoleId,
                    (usr, ract) => ract)
                .Join(_unitOfWork.Repository<SysActivity>().Where(t => t.IsDeleted != true),
                    ract => ract.ActivityId, act => act.Id,
                    (ract, act) => new { ract, act })
                .Select(s => new UserPermissionBaseResponse
                {
                    ActivityId = s.act.Id,
                    ActivityName = s.act.Name,
                    Code = s.act.Code,
                    C = s.ract.C,
                    R = s.ract.R,
                    U = s.ract.U,
                    D = s.ract.D
                })
                .ToListAsync();

            var listUserPermission = await _unitOfWork.Repository<SysUserActivity>()
                .Where(s => s.UserId == currentUserId && s.IsDeleted != true)
                .Join(_unitOfWork.Repository<SysActivity>().Where(t => t.IsDeleted != true),
                    usact => usact.ActivityId, act => act.Id,
                    (usact, act) => new { usact, act })
                .Select(s => new UserPermissionBaseResponse
                {
                    ActivityId = s.act.Id,
                    ActivityName = s.act.Name,
                    Code = s.act.Code,
                    C = s.usact.C,
                    R = s.usact.R,
                    U = s.usact.U,
                    D = s.usact.D
                })
                .ToListAsync();

            var allPermission = listpermission;
            allPermission.AddRange(listPermissionFromRoleOfUser);
            allPermission.AddRange(listUserPermission);

            listpermission = allPermission
                .GroupBy(s => s.ActivityId)
                .Select(s =>
                {
                    var permission = s.FirstOrDefault();
                    var permissionRes = new UserPermissionBaseResponse
                    {
                        ActivityId = permission?.ActivityId,
                        ActivityName = permission?.ActivityName,
                        Code = permission?.Code,
                        C = s.Any(s => s.C == true),
                        R = s.Any(s => s.R == true),
                        U = s.Any(s => s.U == true),
                        D = s.Any(s => s.D == true),
                    };
                    return permissionRes;
                })
                .ToList();
            return listpermission;
        }

        public async Task<object> GetUserActivities(Guid currentUserId, string currentUserName, Guid userId)
        {
            var activities = await _unitOfWork.Repository<SysUserActivity>()
                                              .Where(ua => ua.IsDeleted != true && ua.UserId == userId)
                                              .Join(_unitOfWork.Repository<SysActivity>(), ua => ua.ActivityId,
                                                    a => a.Id,
                                                    (ua, a) => new SysRoleActivitiesResponse
                                                    {
                                                        Id = ua.Id,
                                                        ActivityId = a.Id,
                                                        ActivityName = a.Name,
                                                        C = ua.C,
                                                        R = ua.R,
                                                        U = ua.U,
                                                        D = ua.D,
                                                    })
                                              .ToListAsync();
            var res = new
            {
                UserId = userId,
                Activities = activities
            };

            return Utils.CreateResponseModel(res, 1);
        }

        public async Task<object> UpdateUserActivities(Guid currentUserId, string currentUserName, List<SysRoleActivityRequest> req)
        {
            var roleActivityIds = req.Select(a => a.Id).ToList();
            var userActivities = await _unitOfWork.Repository<SysUserActivity>()
                                                  .Where(ua => roleActivityIds.Contains(ua.Id))
                                                  .ToListAsync();

            var activitiesNotExists = roleActivityIds.Except(userActivities.Select(ua => ua.Id)).ToList();
            if (activitiesNotExists.Any())
                throw new KeyNotFoundException($"Role activities with id {string.Join(", ", activitiesNotExists)} not found");

            userActivities.ForEach(ua =>
            {
                var activity = req.FirstOrDefault(a => a.Id == ua.Id);
                if (activity != null)
                {
                    ua.C = activity.C;
                    ua.R = activity.R;
                    ua.U = activity.U;
                    ua.D = activity.D;
                    ua.UpdatedDate = DateTime.Now;
                    ua.UpdatedById = currentUserId;
                    ua.Updater = currentUserName;
                }
            });

            _unitOfWork.Repository<SysUserActivity>().UpdateRange(userActivities);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }
    }
}
