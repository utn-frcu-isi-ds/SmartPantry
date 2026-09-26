using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SmartPantry.ExternalProducts;
using Xunit;

namespace SmartPantry.HttpApi.Host.Tests;

public class OpenFoodFactsProductCatalogClientTests
{
    [Fact]
    public async Task Should_Read_Only_Fields_Returned_By_Provider()
    {
        var handler = new StubHandler(HttpStatusCode.OK,
            """{"status":"success","product":{"code":"7791234567890","product_name":"Leche"}}""");
        var result = await CreateClient(handler).GetByBarcodeAsync("7791234567890");

        Assert.NotNull(result);
        Assert.Equal("Leche", result.Name);
        Assert.Null(result.Brand);
        Assert.Null(result.ImageUrl);
        Assert.Equal("/api/v3/product/7791234567890?fields=code,product_name,brands,image_url",
            handler.LastRequestUri?.PathAndQuery);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, null)]
    [InlineData(HttpStatusCode.TooManyRequests, CatalogFailureKind.RateLimited)]
    [InlineData(HttpStatusCode.ServiceUnavailable, CatalogFailureKind.Unavailable)]
    public async Task Should_Translate_Http_Status(HttpStatusCode status, CatalogFailureKind? expectedFailure)
    {
        var client = CreateClient(new StubHandler(status, "{}"));
        if (expectedFailure == null)
        {
            Assert.Null(await client.GetByBarcodeAsync("7791234567890"));
            return;
        }

        var exception = await Assert.ThrowsAsync<CatalogClientException>(
            () => client.GetByBarcodeAsync("7791234567890"));
        Assert.Equal(expectedFailure, exception.Kind);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{not json")]
    public async Task Should_Reject_Incomplete_Or_Invalid_Response(string body)
    {
        var client = CreateClient(new StubHandler(HttpStatusCode.OK, body));
        var exception = await Assert.ThrowsAsync<CatalogClientException>(
            () => client.GetByBarcodeAsync("7791234567890"));
        Assert.Equal(CatalogFailureKind.Unavailable, exception.Kind);
    }

    private static OpenFoodFactsProductCatalogClient CreateClient(HttpMessageHandler handler)
    {
        return new OpenFoodFactsProductCatalogClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://world.openfoodfacts.org/")
        });
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public Uri? LastRequestUri { get; private set; }

        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body)
            });
        }
    }
}
