using Asp.Versioning;
using Common.Utils;
using Controllers;
using DomainService.Interfaces.PermissionManagement;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.PermissionManagement;

namespace API_Template.Controllers.Version1.PermissionManagement
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class SysActivitiesController(IHttpContextAccessor httpContextAccessor, IValidator<SysActivityRequest> validator,
                                        ISysActivityService sysActivityService) : BaseController(httpContextAccessor)
    {
        [HttpPost("create-activity")]
        public async Task<IActionResult> CreateActivity(SysActivityRequest req)
        {
            var activityValidator = await validator.ValidateAsync(req);
            if (!activityValidator.IsValid)
                return BadRequest(Utils.CreateResponseModel(activityValidator.Errors[0].ErrorMessage));

            var res = await sysActivityService.Create(currentUserId, username, req);
            return Ok(res);
        }

        [HttpGet("get-activities")]
        public async Task<IActionResult> GetActivities(string keyword = "", int pageIndex = 1, int pageSize = 50)
        {
            var res = await sysActivityService.GetActivities(currentUserId, username, keyword, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpGet("get-detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var res = await sysActivityService.GetDetail(currentUserId, username, id);
            return Ok(res);
        }

        [HttpPut("update-activity/{id}")]
        public async Task<IActionResult> Update(Guid id, SysActivityRequest req)
        {
            var activityValidator = await validator.ValidateAsync(req);
            if (!activityValidator.IsValid)
                return BadRequest(Utils.CreateResponseModel(activityValidator.Errors[0].ErrorMessage));

            var res = await sysActivityService.Update(currentUserId, username, id, req);
            return Ok(res);
        }

        [HttpDelete("delete-activity/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var res = await sysActivityService.Delete(currentUserId, username, id);
            return Ok(res);
        }
    }
}
