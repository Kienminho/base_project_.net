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

public class SysRoleService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
    : BaseService(unitOfWork, memoryCache), ISysRoleService
{
    public async Task<object> GetRoles(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize)
    {
        keyword = keyword.ToLower();
        var query = _unitOfWork.Repository<SysRole>()
            .Where(r => r.IsDeleted != true &&
                        r.Name.ToLower().Contains(keyword))
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new SysRoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedDate = r.CreatedDate,
                Creator = r.CreatedName,
            });

        var data = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderByDescending(d => d.CreatedDate).ToListAsync();

        return Utils.CreateResponseModel(data, await query.CountAsync());
    }

    public async Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id)
    {
        var role = await _unitOfWork.Repository<SysRole>()
                       .Where(r => r.IsDeleted != true && r.Id == id)
                       .Select(r => new SysRoleResponse
                       {
                           Id = r.Id,
                           Name = r.Name,
                           Description = r.Description,
                           CreatedDate = r.CreatedDate,
                           Creator = r.CreatedName,
                       })
                       .FirstOrDefaultAsync()
                   ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Role"));

        return Utils.CreateResponseModel(role, 1);
    }

    public async Task<object> Create(Guid currentUserId, string currentUserName, SysRoleRequest req)
    {
        var existRole = await _unitOfWork.Repository<SysRole>().FirstOrDefaultAsync(r => r.IsDeleted != true && req.Name.Equals(r.Name));

        if (existRole != null)
            throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Role"));

        var role = new SysRole
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            CreatedDate = DateTime.Now,
            CreatedById = currentUserId,
            CreatedName = currentUserName,
        };

        _unitOfWork.Repository<SysRole>().Add(role);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }

    public async Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysRoleRequest req)
    {
        var existRole = await _unitOfWork.Repository<SysRole>().FirstOrDefaultAsync(r => r.IsDeleted != true && r.Id != id && req.Name.Equals(r.Name));

        if (existRole != null)
            throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Role"));

        var role = await _unitOfWork.Repository<SysRole>().FirstOrDefaultAsync(r => r.IsDeleted != true && r.Id == id)
                   ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Role"));

        role.Name = req.Name;
        role.Description = req.Description;
        role.UpdatedDate = DateTime.Now;
        role.UpdatedById = currentUserId;
        role.Updater = currentUserName;

        _unitOfWork.Repository<SysRole>().Update(role);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }

    public async Task<object> Delete(Guid currentUserId, string currentUserName, Guid id)
    {
        var existRole = await _unitOfWork.Repository<SysRole>().FirstOrDefaultAsync(r => r.IsDeleted != true && r.Id == id)
                        ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Role"));

        existRole.IsDeleted = true;
        existRole.UpdatedDate = DateTime.Now;
        existRole.UpdatedById = currentUserId;
        existRole.Updater = currentUserName;

        var userRoles = await _unitOfWork.Repository<SysUserRole>().Where(ur => ur.IsDeleted != true && ur.RoleId == id).ToListAsync();
        var roleActivities = await _unitOfWork.Repository<SysRoleActivity>().Where(ra => ra.IsDeleted != true && ra.RoleId == id).ToListAsync();

        userRoles.ForEach(ur =>
        {
            ur.IsDeleted = true;
            ur.UpdatedDate = DateTime.Now;
            ur.UpdatedById = currentUserId;
            ur.Updater = currentUserName;
        });

        roleActivities.ForEach(ra =>
        {
            ra.IsDeleted = true;
            ra.UpdatedDate = DateTime.Now;
            ra.UpdatedById = currentUserId;
            ra.Updater = currentUserName;
        });

        _unitOfWork.Repository<SysUserRole>().UpdateRange(userRoles);
        _unitOfWork.Repository<SysRoleActivity>().UpdateRange(roleActivities);
        _unitOfWork.Repository<SysRole>().Update(existRole);
        var res = await _unitOfWork.SaveChangesAsync();

        return Utils.CreateResponseModel(res > 0);
    }
}