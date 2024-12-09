using Asp.Versioning;
using Controllers;
using DomainService.Interfaces.PermissionManagement;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.PermissionManagement;

namespace API_Template.Controllers.Version1.PermissionManagement
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SysUserActivitiesController(IHttpContextAccessor httpContextAccessor, ISysUserActivitiesService _sysUserActivitiesService) : BaseController(httpContextAccessor)
    {
        [HttpGet("get-user-activities")]
        public async Task<IActionResult> GetUserActivities(Guid userId)
        {
            var result = await _sysUserActivitiesService.GetUserActivities(currentUserId, username, userId);
            return Ok(result);
        }

        [HttpPost("create-user-activities/{userId}")]
        public async Task<IActionResult> CreateUserActivities(Guid userId, List<SysRoleActivityRequest> req)
        {
            var result = await _sysUserActivitiesService.CreateUserActivities(currentUserId, username, userId, req);
            return Ok(result);
        }

        [HttpPut("update-user-activities")]
        public async Task<IActionResult> UpdateUserActivities(List<SysRoleActivityRequest> req)
        {
            var result = await _sysUserActivitiesService.UpdateUserActivities(currentUserId, username, req);
            return Ok(result);
        }

        [HttpDelete("delete-user-activities")]
        public async Task<IActionResult> DeleteUserActivities(List<Guid> ids)
        {
            var result = await _sysUserActivitiesService.DeleteUserActivities(currentUserId, username, ids);
            return Ok(result);
        }
    }
}
