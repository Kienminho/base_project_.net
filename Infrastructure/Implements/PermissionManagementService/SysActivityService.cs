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

namespace Infrastructure.Implements.PermissionManagement;

public class SysActivityService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
    : BaseService(unitOfWork, memoryCache), ISysActivityService
{
    public async Task<object> GetActivities(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize)
    {
        keyword = keyword.ToLower();
        var query = _unitOfWork.Repository<SysActivity>()
            .Where(a => a.IsDeleted != true && (string.IsNullOrWhiteSpace(keyword) || a.Code.ToLower().Contains(keyword) ||
                                                a.Name.ToLower().Contains(keyword)))
            .Select(a => new SysActivityResponse
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                Creator = a.CreatedName,
                CreatedDate = a.CreatedDate
            });

        var data = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderByDescending(d => d.CreatedDate).ToListAsync();

        return Utils.CreateResponseModel(data, await query.CountAsync());
    }

    public async Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id)
    {
        var activity = await _unitOfWork.Repository<SysActivity>().Where(a => a.IsDeleted != true && a.Id == id)
                           .Select(a => new SysActivityResponse
                           {
                               Id = a.Id,
                               Code = a.Code,
                               Name = a.Name,
                               Description = a.Description,
                               Creator = a.CreatedName,
                               CreatedDate = a.CreatedDate
                           }).FirstOrDefaultAsync()
                       ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Activity"));

        return Utils.CreateResponseModel(activity, 1);
    }
    
    public async Task<object> Create(Guid currentUserId, string currentUserName, SysActivityRequest req)
    {
        var existActivity = await _unitOfWork.Repository<SysActivity>()
            .FirstOrDefaultAsync(a => a.IsDeleted != true && (req.Code.Equals(a.Code) || req.Name.Equals(a.Name)));

        if (existActivity != null)
            throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Activity"));

        var activity = new SysActivity
        {
            Id = Guid.NewGuid(),
            Code = req.Code,
            Name = req.Name,
            Description = req.Description,
            CreatedById = currentUserId,
            CreatedDate = DateTime.Now,
            CreatedName = currentUserName,
            IsDeleted = false
        };

        _unitOfWork.Repository<SysActivity>().Add(activity);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }

    public async Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysActivityRequest req)
    {
        var activity = await _unitOfWork.Repository<SysActivity>().FirstOrDefaultAsync(a => a.IsDeleted != true && a.Id == id)
                       ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Activity"));

        var existActivity = await _unitOfWork.Repository<SysActivity>()
            .FirstOrDefaultAsync(a => a.Id != id && a.IsDeleted != true && (req.Code.Equals(a.Code) || req.Name.Equals(a.Name)));

        if (existActivity != null)
            throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Activity"));

        activity.Code = req.Code;
        activity.Name = req.Name;
        activity.Description = req.Description;
        activity.UpdatedDate = DateTime.Now;
        activity.Updater = currentUserName;
        activity.UpdatedById = currentUserId;

        _unitOfWork.Repository<SysActivity>().Update(activity);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }

    public async Task<object> Delete(Guid currentUserId, string currentUserName, Guid id)
    {
        var activity = await _unitOfWork.Repository<SysActivity>().FirstOrDefaultAsync(a => a.IsDeleted != true && a.Id == id)
                       ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Activity"));

        activity.IsDeleted = true;
        activity.UpdatedDate = DateTime.Now;
        activity.Updater = currentUserName;
        activity.UpdatedById = currentUserId;

        var userActivities = await _unitOfWork.Repository<SysUserActivity>().Where(ua => ua.IsDeleted != true && ua.ActivityId == id).ToListAsync();
        var roleActivities = await _unitOfWork.Repository<SysRoleActivity>().Where(ra => ra.IsDeleted != true && ra.ActivityId == id).ToListAsync();

        userActivities.ForEach(ua =>
        {
            ua.IsDeleted = true;
            ua.UpdatedDate = DateTime.Now;
            activity.Updater = currentUserName;
            activity.UpdatedById = currentUserId;
        });

        roleActivities.ForEach(ra =>
        {
            ra.IsDeleted = true;
            ra.UpdatedDate = DateTime.Now;
            activity.Updater = currentUserName;
            activity.UpdatedById = currentUserId;
        });

        _unitOfWork.Repository<SysUserActivity>().UpdateRange(userActivities);
        _unitOfWork.Repository<SysRoleActivity>().UpdateRange(roleActivities);
        _unitOfWork.Repository<SysActivity>().Update(activity);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }

    public async Task<bool> ValidateActivities(List<Guid> activityIds)
    {
        var activities = await _unitOfWork.Repository<SysActivity>()
            .Where(a => a.IsDeleted != true && activityIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();

        var activityNotExists = activityIds.Except(activities).ToList();
        if (activityNotExists.Any())
            throw new KeyNotFoundException($"Activity with id {string.Join(", ", activityNotExists)} doesn't exists");

        return true;
    }
}