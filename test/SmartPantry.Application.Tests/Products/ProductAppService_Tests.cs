using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Products;

public abstract class ProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;

    protected ProductAppService_Tests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
    }

    [Fact]
    public async Task Should_Create_And_Get_A_Product()
    {
        var createdProduct = await _productAppService.CreateAsync(new CreateProductDto
        {
            Name = "Arroz integral",
            Brand = "Molino Sur"
        });

        var product = await _productAppService.GetAsync(createdProduct.Id);

        product.Id.ShouldBe(createdProduct.Id);
        product.Name.ShouldBe("Arroz integral");
        product.Brand.ShouldBe("Molino Sur");
    }

    [Fact]
    public async Task Should_Not_Create_A_Product_Without_A_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _productAppService.CreateAsync(new CreateProductDto
            {
                Name = "",
                Brand = "Molino Sur"
            });
        });

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Any(member => member == "Name"));
    }
}
