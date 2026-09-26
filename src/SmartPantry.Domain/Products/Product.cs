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
        UpdateDetails(name, brand);
    }

    public void UpdateDetails(string name, string? brand)
    {
        var normalizedName = Check.NotNullOrWhiteSpace(name?.Trim(), nameof(name), ProductConsts.MaxNameLength);
        var normalizedBrand = brand?.Trim();
        if (normalizedBrand?.Length > ProductConsts.MaxBrandLength)
        {
            throw new ArgumentException($"Brand cannot be longer than {ProductConsts.MaxBrandLength} characters.", nameof(brand));
        }

        Name = normalizedName;
        Brand = normalizedBrand;
    }
}
