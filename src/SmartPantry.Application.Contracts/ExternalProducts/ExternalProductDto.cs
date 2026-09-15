namespace SmartPantry.ExternalProducts;

public enum ExternalProductLookupStatus
{
    Found,
    NotFound,
    RateLimited,
    Unavailable
}

public class ExternalProductDto
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
}

public class ExternalProductLookupDto
{
    public ExternalProductLookupStatus Status { get; set; }
    public ExternalProductDto? Product { get; set; }
}
