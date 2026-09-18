using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Products;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string? Brand { get; private set; }

    protected Product()
    {
    }

    public Product(Guid id, string name, string? brand)
        : base(id)
    {
        SetName(name);
        SetBrand(brand);
    }

    private void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), ProductConsts.MaxNameLength);
    }

    private void SetBrand(string? brand)
    {
        Brand = brand?.Trim();

        if (Brand?.Length > ProductConsts.MaxBrandLength)
        {
            throw new ArgumentException($"Brand cannot be longer than {ProductConsts.MaxBrandLength} characters.", nameof(brand));
        }
    }
}
