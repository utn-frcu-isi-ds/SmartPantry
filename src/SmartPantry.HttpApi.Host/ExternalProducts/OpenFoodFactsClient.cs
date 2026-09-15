using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartPantry.ExternalProducts;

public class OpenFoodFactsClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                $"api/v3/product/{barcode}?fields=code,product_name,brands,image_url");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new CatalogClientException(CatalogFailureKind.RateLimited);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new CatalogClientException(CatalogFailureKind.Unavailable);
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (!document.RootElement.TryGetProperty("product", out var product) ||
                product.ValueKind != JsonValueKind.Object)
            {
                throw new CatalogClientException(CatalogFailureKind.Unavailable);
            }

            return new ExternalProductDto
            {
                Code = GetString(product, "code") ?? barcode,
                Name = GetString(product, "product_name"),
                Brand = GetString(product, "brands"),
                ImageUrl = GetString(product, "image_url")
            };
        }
        catch (CatalogClientException)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new CatalogClientException(CatalogFailureKind.Unavailable, exception);
        }
    }

    private static string? GetString(JsonElement product, string name)
    {
        return product.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
