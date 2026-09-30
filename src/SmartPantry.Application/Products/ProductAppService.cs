using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Products;

[AllowAnonymous]
public class ProductAppService : CrudAppService<
    Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateProductDto, UpdateProductDto>,
    IProductAppService
{
    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
    }

    public override async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        var product = new Product(GuidGenerator.Create(), input.Name, input.Brand);
        await Repository.InsertAsync(product, autoSave: true);
        return MapToGetOutputDto(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        var product = await Repository.GetAsync(id);
        product.UpdateDetails(input.Name, input.Brand);
        await Repository.UpdateAsync(product, autoSave: true);
        return MapToGetOutputDto(product);
    }
}
