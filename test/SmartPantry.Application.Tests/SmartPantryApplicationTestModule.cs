using Volo.Abp.Modularity;

namespace SmartPantry;

using Microsoft.Extensions.DependencyInjection;

[DependsOn(
    typeof(SmartPantryApplicationModule),
    typeof(SmartPantryDomainTestModule)
)]
public class SmartPantryApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ExternalProducts.IExternalProductCatalogClient, FakeExternalProductCatalogClient>();
    }
}
