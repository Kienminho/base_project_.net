using Asp.Versioning;
using Controllers;
using DomainService.Interfaces.PermissionManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Template.Controllers.Version1.PermissionManagement
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SysUserRolesController(IHttpContextAccessor httpContextAccessor, ISysUserRolesService _sysUserRoleService) : BaseController(httpContextAccessor)
    {
        [HttpGet("get-user-roles")]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var result = await _sysUserRoleService.GetUserRoles(currentUserId, username, userId);
            return Ok(result);
        }

        [HttpPost("create-user-roles/{userId}")]
        public async Task<IActionResult> CreateUserRoles(Guid userId, List<Guid> roleIds)
        {
            var result = await _sysUserRoleService.CreateUserRoles(currentUserId, username, userId, roleIds);
            return Ok(result);
        }

        [HttpDelete("delete-user-roles")]
        public async Task<IActionResult> DeleteUserRoles(List<Guid> ids)
        {
            var result = await _sysUserRoleService.DeleteUserRoles(currentUserId, username, ids);
            return Ok(result);
        }
    }
}
