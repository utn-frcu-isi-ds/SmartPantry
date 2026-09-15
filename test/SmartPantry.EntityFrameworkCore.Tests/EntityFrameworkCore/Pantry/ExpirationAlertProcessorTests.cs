using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.Pantry;
using SmartPantry.Products;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Pantry;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class ExpirationAlertProcessorTests : SmartPantryTestBase<SmartPantryEntityFrameworkCoreTestModule>
{
    private static readonly DateOnly Today = new(2026, 9, 15);

    [Fact]
    public async Task Should_Process_Only_Due_Items_And_Not_Duplicate_Alerts()
    {
        var userId = Guid.NewGuid();
        var dueId = Guid.NewGuid();
        var overdueId = Guid.NewGuid();
        var laterId = Guid.NewGuid();
        var noDateId = Guid.NewGuid();
        var consumedId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var products = GetRequiredService<IRepository<Product, Guid>>();
            var items = GetRequiredService<IRepository<PantryItem, Guid>>();
            var product = new Product(Guid.NewGuid(), "Producto de prueba", null);
            await products.InsertAsync(product);
            await items.InsertAsync(new PantryItem(dueId, userId, product.Id, Today.AddDays(3)));
            await items.InsertAsync(new PantryItem(overdueId, userId, product.Id, Today.AddDays(-1)));
            await items.InsertAsync(new PantryItem(laterId, userId, product.Id, Today.AddDays(4)));
            await items.InsertAsync(new PantryItem(noDateId, userId, product.Id, null));
            var consumed = new PantryItem(consumedId, userId, product.Id, Today);
            consumed.MarkConsumed();
            await items.InsertAsync(consumed);
        });

        await RunProcessor();
        await RunProcessor();

        await WithUnitOfWorkAsync(async () =>
        {
            var alerts = await GetRequiredService<IRepository<ExpirationAlert, Guid>>().GetListAsync();
            alerts.Count.ShouldBe(2);
            alerts.Select(x => x.PantryItemId).ShouldBe(new[] { dueId, overdueId }, ignoreOrder: true);
            alerts.All(x => x.IsActive && x.UserId == userId).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Should_Refresh_Deactivate_And_Reactivate_One_Alert()
    {
        var itemId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var products = GetRequiredService<IRepository<Product, Guid>>();
            var items = GetRequiredService<IRepository<PantryItem, Guid>>();
            var product = new Product(Guid.NewGuid(), "Producto mutable", null);
            await products.InsertAsync(product);
            await items.InsertAsync(new PantryItem(itemId, Guid.NewGuid(), product.Id, Today.AddDays(1)));
        });

        await RunProcessor();
        await ChangeExpirationDate(itemId, Today.AddDays(2));
        await RunProcessor();
        await CheckOneAlert(itemId, active: true, Today.AddDays(2));

        await ChangeExpirationDate(itemId, Today.AddDays(5));
        await RunProcessor();
        await CheckOneAlert(itemId, active: false, Today.AddDays(2));

        await ChangeExpirationDate(itemId, Today.AddDays(3));
        await RunProcessor();
        await CheckOneAlert(itemId, active: true, Today.AddDays(3));
    }

    private Task RunProcessor()
    {
        return WithUnitOfWorkAsync(() =>
            GetRequiredService<ExpirationAlertProcessor>().ProcessAsync(Today));
    }

    private Task ChangeExpirationDate(Guid itemId, DateOnly date)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IRepository<PantryItem, Guid>>();
            var item = await repository.GetAsync(itemId);
            item.ChangeExpirationDate(date);
            await repository.UpdateAsync(item);
        });
    }

    private Task CheckOneAlert(Guid itemId, bool active, DateOnly date)
    {
        return WithUnitOfWorkAsync(async () =>
        {
            var alerts = await GetRequiredService<IRepository<ExpirationAlert, Guid>>().GetListAsync();
            alerts.Count.ShouldBe(1);
            alerts[0].PantryItemId.ShouldBe(itemId);
            alerts[0].IsActive.ShouldBe(active);
            alerts[0].ExpirationDate.ShouldBe(date);
        });
    }
}
