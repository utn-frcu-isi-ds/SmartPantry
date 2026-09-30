using System.ComponentModel.DataAnnotations;

namespace SmartPantry.ExternalProducts;

public class BarcodeLookupInputDto
{
    [Required]
    [RegularExpression("^[0-9]{8,14}$")]
    public string Barcode { get; set; } = string.Empty;
}
