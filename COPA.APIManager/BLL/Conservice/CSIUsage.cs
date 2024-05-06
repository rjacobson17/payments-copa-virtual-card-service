namespace COPA.APIManager.BLL.Clients.Conservice;

public class CSIUsage
{
    /// <summary>
    /// Current Bulkhead parallelization, or how many concurrent requests to CSI from this runtime.
    /// </summary>
    public int CurrentParallelization { get; set; } = -1;

    /// <summary>
    /// How many requests are queued up currently.
    /// </summary>
    public int CurrentPendingQueueActions { get; set; } = 0;
}
