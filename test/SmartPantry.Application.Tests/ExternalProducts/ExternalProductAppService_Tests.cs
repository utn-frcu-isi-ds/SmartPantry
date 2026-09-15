using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.ExternalProducts;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry;

public abstract class ExternalProductAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IExternalProductAppService _service;

    protected ExternalProductAppService_Tests()
    {
        _service = GetRequiredService<IExternalProductAppService>();
    }

    [Theory]
    [InlineData("7791234567890", ExternalProductLookupStatus.Found)]
    [InlineData("7791234567891", ExternalProductLookupStatus.NotFound)]
    [InlineData("7791234567892", ExternalProductLookupStatus.RateLimited)]
    [InlineData("7791234567893", ExternalProductLookupStatus.Unavailable)]
    public async Task Should_Translate_Catalog_Result(string barcode, ExternalProductLookupStatus status)
    {
        var result = await _service.GetByBarcodeAsync(new BarcodeLookupInputDto { Barcode = barcode });
        result.Status.ShouldBe(status);
        (result.Product != null).ShouldBe(status == ExternalProductLookupStatus.Found);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1234567")]
    public async Task Should_Reject_Invalid_Barcodes(string barcode)
    {
        var exception = await Should.ThrowAsync<AbpValidationException>(() =>
            _service.GetByBarcodeAsync(new BarcodeLookupInputDto { Barcode = barcode }));
        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Any(name => name == "Barcode"));
    }
}
