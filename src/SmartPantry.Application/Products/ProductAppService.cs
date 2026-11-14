using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

[Authorize]
public class ProductAppService : CrudAppService<
    Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateProductDto, UpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

    // Keep inherited CRUD declarations here so service interception also sees class authorization.
    public override Task<ProductDto> GetAsync(Guid id)
    {
        return base.GetAsync(id);
    }

    public override Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        return base.GetListAsync(input);
    }

    public override Task DeleteAsync(Guid id)
    {
        return base.DeleteAsync(id);
    }

    public override async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        await CheckCreatePolicyAsync();
        var product = new Product(GuidGenerator.Create(), input.Name, input.Brand);
        await Repository.InsertAsync(product, autoSave: true);
        return MapToGetOutputDto(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        await CheckUpdatePolicyAsync();
        var product = await Repository.GetAsync(id);
        product.UpdateDetails(input.Name, input.Brand);
        await Repository.UpdateAsync(product, autoSave: true);
        return MapToGetOutputDto(product);
    }
}
