using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPantry.Products;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace SmartPantry.Pantry;

[Authorize]
public class PantryItemAppService : SmartPantryAppService, IPantryItemAppService
{
    private readonly IRepository<PantryItem, Guid> _items;
    private readonly IRepository<Product, Guid> _products;
    private readonly ICurrentUser _currentUser;

    public PantryItemAppService(
        IRepository<PantryItem, Guid> items,
        IRepository<Product, Guid> products,
        ICurrentUser currentUser)
    {
        _items = items;
        _products = products;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<PantryItemDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var ownerId = _currentUser.GetId();
        var query = (await _items.GetQueryableAsync()).Where(item => item.UserId == ownerId);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var page = await AsyncExecuter.ToListAsync(query.OrderBy(item => item.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount));
        var productIds = page.Select(item => item.ProductId).Distinct().ToArray();
        var products = await AsyncExecuter.ToListAsync((await _products.GetQueryableAsync())
            .Where(product => productIds.Contains(product.Id)));
        var names = products.ToDictionary(product => product.Id, product => product.Name);
        return new PagedResultDto<PantryItemDto>(totalCount,
            page.Select(item => ToDto(item, names[item.ProductId])).ToList());
    }

    public async Task<PantryItemDto> GetAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        return await ToDtoAsync(item);
    }

    public async Task<PantryItemDto> CreateAsync(CreatePantryItemDto input)
    {
        var product = await _products.GetAsync(input.ProductId);
        var item = new PantryItem(GuidGenerator.Create(), _currentUser.GetId(), input.ProductId,
            input.ExpirationDate, input.Quantity, input.Unit);
        await _items.InsertAsync(item, autoSave: true);
        return ToDto(item, product.Name);
    }

    public async Task<PantryItemDto> UpdateAsync(Guid id, UpdatePantryItemDto input)
    {
        var item = await GetOwnedItemAsync(id);
        item.UpdateDetails(input.Quantity, input.Unit, input.ExpirationDate);
        await _items.UpdateAsync(item, autoSave: true);
        return await ToDtoAsync(item);
    }

    public async Task<PantryItemDto> MarkConsumedAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        item.MarkConsumed();
        await _items.UpdateAsync(item, autoSave: true);
        return await ToDtoAsync(item);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _items.DeleteAsync(await GetOwnedItemAsync(id), autoSave: true);
    }

    private async Task<PantryItem> GetOwnedItemAsync(Guid id)
    {
        var ownerId = _currentUser.GetId();
        var item = await AsyncExecuter.FirstOrDefaultAsync((await _items.GetQueryableAsync())
            .Where(candidate => candidate.Id == id && candidate.UserId == ownerId));
        return item ?? throw new EntityNotFoundException(typeof(PantryItem), id);
    }

    private async Task<PantryItemDto> ToDtoAsync(PantryItem item)
    {
        var product = await _products.GetAsync(item.ProductId);
        return ToDto(item, product.Name);
    }

    private static PantryItemDto ToDto(PantryItem item, string productName) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductName = productName,
        Quantity = item.Quantity,
        Unit = item.Unit,
        ExpirationDate = item.ExpirationDate,
        IsActive = item.IsActive
    };
}
