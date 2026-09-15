using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace SmartPantry.ExternalProducts;

[AllowAnonymous]
public class ExternalProductAppService : ApplicationService, IExternalProductAppService
{
    private readonly IExternalProductCatalogClient _catalogClient;

    public ExternalProductAppService(IExternalProductCatalogClient catalogClient)
    {
        _catalogClient = catalogClient;
    }

    public async Task<ExternalProductLookupDto> GetByBarcodeAsync(BarcodeLookupInputDto input)
    {
        try
        {
            var product = await _catalogClient.GetByBarcodeAsync(input.Barcode);
            return new ExternalProductLookupDto
            {
                Status = product == null ? ExternalProductLookupStatus.NotFound : ExternalProductLookupStatus.Found,
                Product = product
            };
        }
        catch (CatalogClientException exception)
        {
            return new ExternalProductLookupDto
            {
                Status = exception.Kind == CatalogFailureKind.RateLimited
                    ? ExternalProductLookupStatus.RateLimited
                    : ExternalProductLookupStatus.Unavailable
            };
        }
    }
}
