using System;
using System.Threading;
using System.Threading.Tasks;
using COPA.APIManager.BLL.Clients.Conservice;

namespace COPA.APIManager.BLL.Interface;

/// <summary>
/// Matthew Davis at CSI (Eden Red) confirmed the constraint is simply 50 concurrent requests in prod, 10 in sandbox.
/// No time-based constraint.
///
/// As per Meeting on the phone Jan 17, 2024. In attendance:
/// * Parikshit Singh
/// * Colton Milne
/// * Robert Jacobson
/// * Wendel Schultz
/// 
/// </summary>
public interface ICSIRateCompliance
{
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> doIt, CancellationToken cancel);

    public Task ExecuteAsync(Func<CancellationToken, Task> doIt, CancellationToken cancel);

    public Task<CSIConstraints> UpdateLimitsAsync(CSIConstraints constraints, CancellationToken cancel);

    public CSIConstraints GetUsageLimits();
    public CSIUsage GetCurrentUsage();
}
