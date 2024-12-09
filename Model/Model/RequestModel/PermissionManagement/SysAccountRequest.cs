using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Model.RequestModel.PermissionManagement
{
    public class SysAccountRequest
    {
        public string Code { get; set; } = String.Empty;
        public string UserName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public string? Group { get; set; }
        public string? Base { get; set; }
        public string? Note { get; set; }
        public IFormFile? Avatar { get; set; }
    }

    public class SysAccountRequestValidator : AbstractValidator<SysAccountRequest>
    {
        public SysAccountRequestValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required");
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required")
                                       .Matches(@"^(\+84|84|0)(3|5|7|8|9|1[2|6|8|9])[0-9]{8}$")
                                       .WithMessage("Phone number is invalid");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                                 .EmailAddress().WithMessage("Email is invalid");

        }
    }
}
