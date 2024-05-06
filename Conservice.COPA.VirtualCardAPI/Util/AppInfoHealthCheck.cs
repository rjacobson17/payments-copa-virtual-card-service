using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conservice.COPA.VirtualCardAPI.Util
{
    /// <summary>
    /// Health Check with Application Info.
    /// </summary>
    public class AppInfoHealthCheck : IHealthCheck
    {
        IConfiguration _config;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config"><see cref="IConfiguration"/></param>
        public AppInfoHealthCheck(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Checks the application health.
        /// </summary>
        /// <param name="context"><see cref="HealthCheckContext"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="HealthCheckResult"/></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var status = HealthStatus.Healthy;

            var version = GetType().Assembly.GetName().Version?.ToString();
            var aspnetEnv = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (version == null || aspnetEnv == null)
            {
                status = HealthStatus.Unhealthy;
            }

            var data = new Dictionary<string, object>()
            {
                { "Version", version ?? "null" },
                { "ASPNETCORE_ENVIRONMENT", aspnetEnv ?? "null" },
            };

            return Task.FromResult(new HealthCheckResult(
                status,
                data: data
            ));
        }
    }
}
