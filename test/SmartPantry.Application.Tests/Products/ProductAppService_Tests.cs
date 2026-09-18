using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
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

    [Fact]
    public async Task Should_List_Update_And_Delete_A_Product()
    {
        var firstProduct = await _productAppService.CreateAsync(new CreateProductDto
        {
            Name = "000 Producto TP06",
            Brand = "Marca A"
        });
        var lastProduct = await _productAppService.CreateAsync(new CreateProductDto
        {
            Name = "ZZZ Producto TP06",
            Brand = "Marca Z"
        });

        var list = await _productAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 1,
            Sorting = "Name ASC"
        });
        list.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        list.Items.Count.ShouldBe(1);
        list.Items.Single().Id.ShouldBe(firstProduct.Id);

        var updated = await _productAppService.UpdateAsync(lastProduct.Id, new UpdateProductDto
        {
            Name = "  Producto actualizado  ",
            Brand = "  Marca B  "
        });
        updated.Name.ShouldBe("Producto actualizado");
        updated.Brand.ShouldBe("Marca B");

        var fetched = await _productAppService.GetAsync(lastProduct.Id);
        fetched.Name.ShouldBe("Producto actualizado");

        await _productAppService.DeleteAsync(lastProduct.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _productAppService.GetAsync(lastProduct.Id));
    }
}
