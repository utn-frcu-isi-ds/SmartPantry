using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Pantry;

public class PantryItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}
