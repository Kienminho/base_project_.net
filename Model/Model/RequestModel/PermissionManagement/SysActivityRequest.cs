using FluentValidation;

namespace Model.RequestModel.PermissionManagement;

public class SysActivityRequest
{
    public string Code { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public string? Description { get; set; }
}

public class SysActivityRequestValidator : AbstractValidator<SysActivityRequest>
{
    public SysActivityRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required");
    }
}