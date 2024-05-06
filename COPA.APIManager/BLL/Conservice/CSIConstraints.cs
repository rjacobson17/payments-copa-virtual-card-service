namespace COPA.APIManager.BLL.Clients.Conservice;

public class CSIConstraints
{
    /// <summary>
    /// Bulkhead parallelization, or how many maximum concurrent requests to CSI from this runtime are allowed.
    ///
    /// Meaning, if there are 3 nodes, and we have a limit of 30 concurrent requests, then each node can have something
    /// like 10 concurrent requests.  max/nodes * 1.25 is a good rule of thumb.
    /// </summary>
    public int MaxParallelization { get; set; } = 1;

    /// <summary>
    /// When the bulkhead is full, how many requests can be queued up before we start rejecting requests.
    ///
    /// Meaning, if this node has 10 concurrent requests and a max pending queue action of 5, then when the 16th request
    /// comes in before the 1st returns, it will be rejected by the bulkhead.
    /// </summary>
    public int MaxPendingQueueActions { get; set; } = 0;
}
