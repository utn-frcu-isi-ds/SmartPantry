using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications.ExternalProducts;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreExternalProductAppService_Tests
    : ExternalProductAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}
