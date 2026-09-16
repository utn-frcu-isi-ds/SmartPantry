using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SmartPantry.ExternalProducts;
using Xunit;

namespace SmartPantry;

public class ExternalProductAppServiceMockTests
{
    private readonly IExternalProductCatalogClient _catalogClient;
    private readonly ExternalProductAppService _service;

    public ExternalProductAppServiceMockTests()
    {
        _catalogClient = Substitute.For<IExternalProductCatalogClient>();
        _service = new ExternalProductAppService(_catalogClient);
    }

    [Fact]
    public async Task Should_Return_Product_Provided_By_Mock()
    {
        _catalogClient.GetByBarcodeAsync("7791234567890").Returns(new ExternalProductDto
        {
            Code = "7791234567890",
            Name = "Leche"
        });

        var result = await _service.GetByBarcodeAsync(new BarcodeLookupInputDto
        {
            Barcode = "7791234567890"
        });

        result.Status.ShouldBe(ExternalProductLookupStatus.Found);
        result.Product!.Name.ShouldBe("Leche");
        await _catalogClient.Received(1).GetByBarcodeAsync("7791234567890");
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Mock_Returns_Null()
    {
        _catalogClient.GetByBarcodeAsync("7791234567891")
            .Returns(Task.FromResult<ExternalProductDto?>(null));

        var result = await _service.GetByBarcodeAsync(new BarcodeLookupInputDto
        {
            Barcode = "7791234567891"
        });

        result.Status.ShouldBe(ExternalProductLookupStatus.NotFound);
        result.Product.ShouldBeNull();
    }

    [Theory]
    [InlineData(CatalogFailureKind.RateLimited, ExternalProductLookupStatus.RateLimited)]
    [InlineData(CatalogFailureKind.Unavailable, ExternalProductLookupStatus.Unavailable)]
    public async Task Should_Translate_Failure_Produced_By_Mock(
        CatalogFailureKind failure,
        ExternalProductLookupStatus expectedStatus)
    {
        _catalogClient.GetByBarcodeAsync("7791234567892")
            .Returns<Task<ExternalProductDto?>>(_ => throw new CatalogClientException(failure));

        var result = await _service.GetByBarcodeAsync(new BarcodeLookupInputDto
        {
            Barcode = "7791234567892"
        });

        result.Status.ShouldBe(expectedStatus);
        result.Product.ShouldBeNull();
    }
}
