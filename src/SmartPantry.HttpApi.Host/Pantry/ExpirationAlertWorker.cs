using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Pantry;

public class ExpirationAlertWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ExpirationAlertWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(24).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var manager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var processor = workerContext.ServiceProvider.GetRequiredService<ExpirationAlertProcessor>();
        using var unitOfWork = manager.Begin();
        await processor.ProcessAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        await unitOfWork.CompleteAsync();
    }
}
