using System.Threading.Tasks;
using SmartPantry.ExternalProducts;

namespace SmartPantry;

public class FakeExternalProductCatalogClient : IExternalProductCatalogClient
{
    public Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        return barcode switch
        {
            "7791234567890" => Task.FromResult<ExternalProductDto?>(new ExternalProductDto
            {
                Code = barcode,
                Name = "Leche",
                Brand = "Marca de prueba"
            }),
            "7791234567891" => Task.FromResult<ExternalProductDto?>(null),
            "7791234567892" => throw new CatalogClientException(CatalogFailureKind.RateLimited),
            _ => throw new CatalogClientException(CatalogFailureKind.Unavailable)
        };
    }
}
