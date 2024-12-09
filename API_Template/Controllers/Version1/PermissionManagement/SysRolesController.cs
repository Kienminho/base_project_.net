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
    public class SysRolesController(IHttpContextAccessor httpContextAccessor, IValidator<SysRoleRequest> validator,
            ISysRoleService sysRoleService) : BaseController(httpContextAccessor)
    {
        [HttpGet("get-roles")]
        public async Task<IActionResult> GetRoles(string keyword = "", int pageIndex = 1, int pageSize = 50)
        {
            var result = await sysRoleService.GetRoles(currentUserId, username, keyword, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("get-detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await sysRoleService.GetDetail(currentUserId, username, id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(SysRoleRequest req)
        {
            var validationResult = await validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return BadRequest(Utils.CreateResponseModel(validationResult.Errors[0].ErrorMessage));

            var result = await sysRoleService.Create(currentUserId, username, req);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, SysRoleRequest req)
        {
            var validationResult = await validator.ValidateAsync(req);
            if (!validationResult.IsValid)
                return BadRequest(Utils.CreateResponseModel(validationResult.Errors[0].ErrorMessage));

            var result = await sysRoleService.Update(currentUserId, username, id, req);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await sysRoleService.Delete(currentUserId, username, id);
            return Ok(result);
        }
    }
}
