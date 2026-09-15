using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Pantry;

public class ExpirationAlert : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid PantryItemId { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }

    protected ExpirationAlert()
    {
    }

    public ExpirationAlert(Guid id, PantryItem item)
        : base(id)
    {
        UserId = item.UserId;
        PantryItemId = item.Id;
        Refresh(item);
    }

    public void Refresh(PantryItem item)
    {
        if (item.Id != PantryItemId || item.UserId != UserId)
        {
            throw new ArgumentException("The alert belongs to another pantry item.", nameof(item));
        }

        ExpirationDate = item.ExpirationDate
            ?? throw new ArgumentException("The pantry item has no expiration date.", nameof(item));
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
