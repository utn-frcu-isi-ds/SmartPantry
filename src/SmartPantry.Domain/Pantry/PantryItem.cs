using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry;

public class PantryItem : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }

    protected PantryItem()
    {
    }

    public PantryItem(Guid id, Guid userId, Guid productId, DateOnly? expirationDate)
        : base(id)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A pantry item needs an owner.", nameof(userId));
        if (productId == Guid.Empty) throw new ArgumentException("A pantry item needs a product.", nameof(productId));
        UserId = userId;
        ProductId = productId;
        ExpirationDate = expirationDate;
        IsActive = true;
    }

    public void ChangeExpirationDate(DateOnly? expirationDate)
    {
        ExpirationDate = expirationDate;
    }

    public void MarkConsumed()
    {
        IsActive = false;
    }
}
