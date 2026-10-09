using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SmartPantry.EntityFrameworkCore.Security;

[DependsOn(typeof(SmartPantryEntityFrameworkCoreTestModule))]
public class SecurityTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // The generated test base bypasses these services. This suite restores real authorization.
        context.Services.Replace(ServiceDescriptor.Transient<IAuthorizationService, AbpAuthorizationService>());
        context.Services.Replace(ServiceDescriptor.Transient<IAbpAuthorizationService, AbpAuthorizationService>());
        context.Services.Replace(ServiceDescriptor.Transient<IMethodInvocationAuthorizationService, MethodInvocationAuthorizationService>());
        context.Services.Replace(ServiceDescriptor.Transient<IPermissionChecker, PermissionChecker>());
    }
}
