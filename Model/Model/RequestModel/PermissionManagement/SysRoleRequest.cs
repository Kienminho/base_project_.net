using FluentValidation;

namespace Model.RequestModel.PermissionManagement;

public class SysRoleRequest
{
    public string Name { get; set; }  = String.Empty;
    public string? Description { get; set; }
    public int? Type { get; set; }
}

public class SysRoleRequestValidator : AbstractValidator<SysRoleRequest>
{
    public SysRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}