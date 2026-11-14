using System;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Pantry;

public class CreatePantryItemDto : UpdatePantryItemDto
{
    [Required]
    public Guid ProductId { get; set; }
}
