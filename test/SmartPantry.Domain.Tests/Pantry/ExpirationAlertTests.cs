using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Pantry;

public class ExpirationAlertTests
{
    [Fact]
    public void Should_Reject_An_Alert_For_Another_Owner()
    {
        var first = new PantryItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 9, 18));
        var second = new PantryItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 9, 18));
        var alert = new ExpirationAlert(Guid.NewGuid(), first);

        Should.Throw<ArgumentException>(() => alert.Refresh(second));
        alert.PantryItemId.ShouldBe(first.Id);
        alert.UserId.ShouldBe(first.UserId);
    }
}
