using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BLL.Implementation;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Polly.Bulkhead;

namespace COPA.APIManager.Tests.BLL.Implementation;

public class CSIRateComplianceShould
{
    private readonly int maxParallelization = 9;
    private readonly int maxQueueDepth = 4;

    [Test]
    public async Task TestInvertingAvailabilityCounts_AddUp()
    {
        int totalShouldEnter = maxParallelization + maxQueueDepth;
        SemaphoreSlim shady = new SemaphoreSlim(0, totalShouldEnter);

        Func<CancellationToken, Task<int>> work = ct => Task.Run(async () =>
        {
            await shady.WaitAsync();
            return 7654;
        });

        CSIRateCompliance compliance = new CSIRateCompliance(CreateConfiguration());

        for (int i = 0; i < maxParallelization; i++)
        {
            int nth = i + 1;

            Task<int> task = compliance.ExecuteAsync(work, CancellationToken.None);
            
            CSIUsage usage = compliance.GetCurrentUsage();
            Assert.That(usage.CurrentParallelization, Is.EqualTo(nth));
            Assert.That(usage.CurrentPendingQueueActions, Is.EqualTo(0));
        }

        for (int i = 0; i < maxQueueDepth; i++)
        {
            int nth = i + 1;

            Task<int> task = compliance.ExecuteAsync(work, CancellationToken.None);
            
            CSIUsage usage = compliance.GetCurrentUsage();
            Assert.That(usage.CurrentParallelization, Is.EqualTo(maxParallelization));
            Assert.That(usage.CurrentPendingQueueActions, Is.EqualTo(nth));
        }

        try
        {
            await compliance.ExecuteAsync(work, CancellationToken.None);
            Assert.Fail("Expected BulkheadRejectedException");
        }
        catch (BulkheadRejectedException e)
        {
            Assert.Pass("We broke the Bulkhead. Huzzah.");
        }
    }

    private IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "CardSettings:CSI:MaxParallelization", maxParallelization.ToString() },
                { "CardSettings:CSI:MaxPendingQueueActions", maxQueueDepth.ToString() }

            }).Build();
    }
}