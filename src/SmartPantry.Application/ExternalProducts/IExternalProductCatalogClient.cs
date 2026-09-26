using System;
using System.Threading.Tasks;

namespace SmartPantry.ExternalProducts;

public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}

public enum CatalogFailureKind
{
    RateLimited,
    Unavailable
}

public sealed class CatalogClientException : Exception
{
    public CatalogFailureKind Kind { get; }

    public CatalogClientException(CatalogFailureKind kind, Exception? innerException = null)
        : base($"External catalog failure: {kind}", innerException)
    {
        Kind = kind;
    }
}
