using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Model.RequestModel.PermissionManagement;

namespace Model.RequestModel
{
    public static class ValidatorServiceRegistration
    {
        public static IServiceCollection ValidatorsServiceRegistration(this IServiceCollection service)
        {
            service.AddScoped<IValidator<SysActivityRequest>, SysActivityRequestValidator>();
            service.AddScoped<IValidator<SysRoleRequest>, SysRoleRequestValidator>();
            service.AddScoped<IValidator<SysAccountRequest>, SysAccountRequestValidator>();
            return service;
        }
    }
}
