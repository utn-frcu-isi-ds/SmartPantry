using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SmartPantry.Pantry;

public class ExpirationAlertProcessor : ITransientDependency
{
    private readonly IRepository<PantryItem, Guid> _items;
    private readonly IRepository<ExpirationAlert, Guid> _alerts;
    private readonly IGuidGenerator _guidGenerator;

    public ExpirationAlertProcessor(
        IRepository<PantryItem, Guid> items,
        IRepository<ExpirationAlert, Guid> alerts,
        IGuidGenerator guidGenerator)
    {
        _items = items;
        _alerts = alerts;
        _guidGenerator = guidGenerator;
    }

    public async Task ProcessAsync(DateOnly today, int warningDays = 3)
    {
        if (warningDays < 0) throw new ArgumentOutOfRangeException(nameof(warningDays));

        var items = await _items.GetListAsync();
        var alerts = (await _alerts.GetListAsync()).ToDictionary(alert => alert.PantryItemId);
        var limit = today.AddDays(warningDays);

        foreach (var item in items)
        {
            var isDue = item.IsActive && item.ExpirationDate is { } date && date <= limit;
            alerts.TryGetValue(item.Id, out var alert);

            if (isDue && alert == null)
            {
                await _alerts.InsertAsync(new ExpirationAlert(_guidGenerator.Create(), item));
            }
            else if (isDue && alert != null &&
                     (!alert.IsActive || alert.ExpirationDate != item.ExpirationDate))
            {
                alert.Refresh(item);
                await _alerts.UpdateAsync(alert);
            }
            else if (!isDue && alert?.IsActive == true)
            {
                alert.Deactivate();
                await _alerts.UpdateAsync(alert);
            }
        }
    }
}
