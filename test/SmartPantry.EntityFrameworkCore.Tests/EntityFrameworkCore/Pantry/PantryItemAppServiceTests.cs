using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.EntityFrameworkCore.Security;
using SmartPantry.Pantry;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Pantry;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class PantryItemAppServiceTests : SmartPantryTestBase<SecurityTestModule>
{
    private readonly IPantryItemAppService _service;
    private readonly ICurrentPrincipalAccessor _principals;

    public PantryItemAppServiceTests()
    {
        _service = GetRequiredService<IPantryItemAppService>();
        _principals = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task Should_List_Only_The_Owners_Items_With_Product_Names_And_Paging()
    {
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var productId = await SeedProductAsync();
        await SeedItemAsync(owner, productId);
        await SeedItemAsync(owner, productId);
        await SeedItemAsync(other, productId);

        using (_principals.Change(Principal(owner)))
        {
            var page = await _service.GetListAsync(new PagedAndSortedResultRequestDto
                { SkipCount = 0, MaxResultCount = 1 });
            page.TotalCount.ShouldBe(2);
            page.Items.Count.ShouldBe(1);
            page.Items.Single().ProductName.ShouldBe("Arroz integral");
        }
    }

    [Fact]
    public async Task Should_Create_Update_Consume_And_Delete_Own_Item()
    {
        var owner = Guid.NewGuid();
        var productId = await SeedProductAsync();
        using (_principals.Change(Principal(owner)))
        {
            var created = await _service.CreateAsync(new CreatePantryItemDto
                { ProductId = productId, Quantity = 2, Unit = "kg" });
            created.ProductName.ShouldBe("Arroz integral");

            var updated = await _service.UpdateAsync(created.Id, new UpdatePantryItemDto
                { Quantity = 1.5m, Unit = " paquetes ", ExpirationDate = new DateOnly(2026, 12, 31) });
            updated.Quantity.ShouldBe(1.5m);
            updated.Unit.ShouldBe("paquetes");
            updated.ExpirationDate.ShouldBe(new DateOnly(2026, 12, 31));

            (await _service.MarkConsumedAsync(created.Id)).IsActive.ShouldBeFalse();
            (await _service.GetListAsync(new PagedAndSortedResultRequestDto())).Items
                .ShouldContain(item => item.Id == created.Id && !item.IsActive);

            await _service.DeleteAsync(created.Id);
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.GetAsync(created.Id));
        }
    }

    [Fact]
    public async Task Should_Hide_Foreign_Item_From_Read_Update_Consume_And_Delete()
    {
        var productId = await SeedProductAsync();
        var foreignId = await SeedItemAsync(Guid.NewGuid(), productId);
        using (_principals.Change(Principal(Guid.NewGuid())))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.GetAsync(foreignId));
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.UpdateAsync(foreignId,
                new UpdatePantryItemDto { Quantity = 3 }));
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.MarkConsumedAsync(foreignId));
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.DeleteAsync(foreignId));
        }
    }

    [Fact]
    public async Task Should_Reject_Anonymous_And_Negative_Quantity()
    {
        using (_principals.Change(new ClaimsPrincipal(new ClaimsIdentity())))
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                _service.GetListAsync(new PagedAndSortedResultRequestDto()));

        var owner = Guid.NewGuid();
        var productId = await SeedProductAsync();
        using (_principals.Change(Principal(owner)))
        {
            var item = await _service.CreateAsync(new CreatePantryItemDto { ProductId = productId });
            await Should.ThrowAsync<AbpValidationException>(() => _service.UpdateAsync(item.Id,
                new UpdatePantryItemDto { Quantity = -1 }));
        }
    }

    private async Task<Guid> SeedProductAsync()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(() => GetRequiredService<IRepository<Product, Guid>>()
            .InsertAsync(new Product(id, "Arroz integral", null)));
        return id;
    }

    private async Task<Guid> SeedItemAsync(Guid userId, Guid productId)
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(() => GetRequiredService<IRepository<PantryItem, Guid>>()
            .InsertAsync(new PantryItem(id, userId, productId, null)));
        return id;
    }

    private static ClaimsPrincipal Principal(Guid userId)
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(AbpClaimTypes.UserId, userId.ToString()));
        identity.AddClaim(new Claim(AbpClaimTypes.UserName, "usuario"));
        return new ClaimsPrincipal(identity);
    }
}
