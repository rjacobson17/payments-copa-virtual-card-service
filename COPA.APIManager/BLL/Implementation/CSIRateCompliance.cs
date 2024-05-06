using System;
using System.Threading;
using System.Threading.Tasks;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BLL.Interface;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Bulkhead;

namespace COPA.APIManager.BLL.Implementation;


public class CSIRateCompliance : ICSIRateCompliance
{
    // volatile is used to ensure that assignment to the bulkhead field completes before the bulkhead is used.
    // a strict semaphore isn't necessary since this is a runtime adjustment.
    private volatile AsyncBulkheadPolicy bulkhead = null;
    private volatile CSIConstraints _currentConstraints = null;

    public CSIRateCompliance(IConfiguration configuration)
    {
        int maxParallelization = configuration.GetValue<int>("CardSettings:CSI:MaxParallelization", 10);
        int maxQueueDepth = configuration.GetValue<int>("CardSettings:CSI:MaxPendingQueueActions", 5);

        _currentConstraints = UpdateLimits(new CSIConstraints()
        {
            MaxParallelization = maxParallelization,
            MaxPendingQueueActions = maxQueueDepth
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> doIt, CancellationToken cancel)
    {
        return await bulkhead.ExecuteAsync<T>(doIt, cancel);
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> doIt, CancellationToken cancel)
    {
        await bulkhead.ExecuteAsync(doIt, cancel);
    }

    public CSIConstraints GetUsageLimits()
    {
        return _currentConstraints;
    }

    public CSIUsage GetCurrentUsage()
    {
        // the bulkhead reports available. we want the inverse.
        int parallelization = _currentConstraints.MaxParallelization - bulkhead.BulkheadAvailableCount;
        int pendingQueueActions = _currentConstraints.MaxPendingQueueActions - bulkhead.QueueAvailableCount;
        return new CSIUsage()
        {
            CurrentParallelization = parallelization,
            CurrentPendingQueueActions = pendingQueueActions
        };
    }

    public Task<CSIConstraints> UpdateLimitsAsync(CSIConstraints constraints, CancellationToken cancel)
    {
        CSIConstraints updated = UpdateLimits(constraints);

        return Task.FromResult(updated);
    }

    private CSIConstraints UpdateLimits(CSIConstraints constraints)
    {
        _currentConstraints = new CSIConstraints()
        {
            MaxParallelization = constraints.MaxParallelization,
            MaxPendingQueueActions = constraints.MaxPendingQueueActions
        };

        bulkhead = Policy.BulkheadAsync(_currentConstraints.MaxParallelization, _currentConstraints.MaxPendingQueueActions);

        return _currentConstraints;
    }
}