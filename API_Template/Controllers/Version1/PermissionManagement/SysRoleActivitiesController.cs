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
    public class SysRoleActivitiesController(IHttpContextAccessor httpContextAccessor, ISysRoleActivitiesService _sysRoleActivitiesService) : BaseController(httpContextAccessor)
    {
        [HttpGet("get-role-activities/{roleId}")]
        public async Task<IActionResult> GetRoleActivities(Guid roleId)
        {
            var res = await _sysRoleActivitiesService.GetRoleActivities(currentUserId, username, roleId);
            return Ok(res);
        }

        [HttpPost("create-role-activities/{roleId}")]
        public async Task<IActionResult> CreateRoleActivities(Guid roleId, List<SysRoleActivityRequest> req)
        {
            var res = await _sysRoleActivitiesService.CreateRoleActivities(currentUserId, username, roleId, req);
            return Ok(res);
        }

        [HttpPut("update-role-activities")]
        public async Task<IActionResult> UpdateRoleActivities(List<SysRoleActivityRequest> req)
        {
            var res = await _sysRoleActivitiesService.UpdateRoleActivities(currentUserId, username, req);
            return Ok(res);
        }

        [HttpDelete("delete-role-activities")]
        public async Task<IActionResult> DeleteRoleActivities(List<Guid> id)
        {
            var res = await _sysRoleActivitiesService.DeleteRoleActivities(currentUserId, username, id);
            return Ok(res);
        }
    }
}
