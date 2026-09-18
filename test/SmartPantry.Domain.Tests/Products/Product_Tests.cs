using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Products;

public class Product_Tests
{
    [Fact]
    public void Should_Create_A_Product_With_Trimmed_Values()
    {
        var product = new Product(Guid.NewGuid(), "  Arroz integral  ", "  Molino Sur  ");

        product.Name.ShouldBe("Arroz integral");
        product.Brand.ShouldBe("Molino Sur");
    }

    [Fact]
    public void Should_Not_Create_A_Product_Without_A_Name()
    {
        Should.Throw<ArgumentException>(() => new Product(Guid.NewGuid(), " ", null));
    }
}
