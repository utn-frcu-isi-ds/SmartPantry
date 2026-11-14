using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry;

public class PantryItem : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public bool IsActive { get; private set; }

    protected PantryItem()
    {
    }

    public PantryItem(Guid id, Guid userId, Guid productId, DateOnly? expirationDate,
        decimal? quantity = null, string? unit = null)
        : base(id)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A pantry item needs an owner.", nameof(userId));
        if (productId == Guid.Empty) throw new ArgumentException("A pantry item needs a product.", nameof(productId));
        UserId = userId;
        ProductId = productId;
        UpdateDetails(quantity, unit, expirationDate);
        IsActive = true;
    }

    public void UpdateDetails(decimal? quantity, string? unit, DateOnly? expirationDate)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        var normalizedUnit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        if (normalizedUnit?.Length > 32) throw new ArgumentException("Unit is too long.", nameof(unit));
        Quantity = quantity;
        Unit = normalizedUnit;
        ExpirationDate = expirationDate;
    }

    public void ChangeExpirationDate(DateOnly? expirationDate) =>
        UpdateDetails(Quantity, Unit, expirationDate);

    public void MarkConsumed()
    {
        IsActive = false;
    }
}
