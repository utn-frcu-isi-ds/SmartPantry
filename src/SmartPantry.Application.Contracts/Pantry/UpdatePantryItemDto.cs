using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Pantry;

public class UpdatePantryItemDto
{
    [Range(0, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [StringLength(32)]
    public string? Unit { get; set; }

    public DateOnly? ExpirationDate { get; set; }
}
