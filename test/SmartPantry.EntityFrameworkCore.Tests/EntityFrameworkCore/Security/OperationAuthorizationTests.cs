using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.ExternalProducts;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Security;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class OperationAuthorizationTests : SmartPantryTestBase<SecurityTestModule>
{
    private readonly IProductAppService _products;
    private readonly IExternalProductAppService _external;
    private readonly IIdentityUserAppService _users;
    private readonly ICurrentPrincipalAccessor _principals;

    public OperationAuthorizationTests()
    {
        _products = GetRequiredService<IProductAppService>();
        _external = GetRequiredService<IExternalProductAppService>();
        _users = GetRequiredService<IIdentityUserAppService>();
        _principals = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task RF05_Should_Require_Authentication()
    {
        var input = new BarcodeLookupInputDto { Barcode = "7791234567890" };
        using (_principals.Change(new ClaimsPrincipal(new ClaimsIdentity())))
            await Should.ThrowAsync<AbpAuthorizationException>(() => _external.GetByBarcodeAsync(input));
        using (_principals.Change(UserPrincipal()))
            (await _external.GetByBarcodeAsync(input)).Status.ShouldBe(ExternalProductLookupStatus.Found);
    }

    [Fact]
    public async Task RF08_Should_Reject_Anonymous_Access_Including_Inherited_Crud()
    {
        using var anonymous = _principals.Change(new ClaimsPrincipal(new ClaimsIdentity()));
        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            _products.CreateAsync(new CreateProductDto { Name = "Sin login" }));
        await Should.ThrowAsync<AbpAuthorizationException>(() => _products.GetAsync(Guid.NewGuid()));
        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            _products.GetListAsync(new PagedAndSortedResultRequestDto()));
        await Should.ThrowAsync<AbpAuthorizationException>(() =>
            _products.UpdateAsync(Guid.NewGuid(), new UpdateProductDto { Name = "Sin login" }));
        await Should.ThrowAsync<AbpAuthorizationException>(() => _products.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RF08_Should_Allow_Common_User_To_Save_And_Use_Catalog()
    {
        using var user = _principals.Change(UserPrincipal());
        var product = await _products.CreateAsync(new CreateProductDto { Name = "Guardado por usuario" });
        (await _products.GetAsync(product.Id)).Name.ShouldBe(product.Name);
        (await _products.GetListAsync(new PagedAndSortedResultRequestDto())).Items
            .ShouldContain(x => x.Id == product.Id);
        (await _products.UpdateAsync(product.Id, new UpdateProductDto { Name = "Cambio permitido" }))
            .Name.ShouldBe("Cambio permitido");
        await _products.DeleteAsync(product.Id);
    }

    [Fact]
    public async Task RF04_Should_Require_Identity_Users_Permission()
    {
        using (_principals.Change(UserPrincipal()))
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                _users.GetListAsync(new GetIdentityUsersInput()));
        await WithUnitOfWorkAsync(() => GetRequiredService<IPermissionManager>()
            .SetForRoleAsync("admin", IdentityPermissions.Users.Default, true));
        using (_principals.Change(UserPrincipal("admin")))
            (await _users.GetListAsync(new GetIdentityUsersInput())).ShouldNotBeNull();
    }

    private static ClaimsPrincipal UserPrincipal(string? role = null)
    {
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()));
        identity.AddClaim(new Claim(AbpClaimTypes.UserName, role ?? "usuario"));
        if (role != null) identity.AddClaim(new Claim(AbpClaimTypes.Role, role));
        return new ClaimsPrincipal(identity);
    }
}
