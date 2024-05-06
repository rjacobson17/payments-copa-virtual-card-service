using Flurl.Http;
using Polly;

namespace COPA.APIManager.Utilities.Interfaces
{
    /// <summary>
    /// Interface for defining the resilience strategy for http calls
    /// </summary>
    public interface IHttpResiliencePolicy
    {
        /// <summary>
        /// Creates an <see cref="ISyncPolicy"/> containing the Circuit Breaker and Retry policy for handling a <see cref="HttpResponse"/>.
        /// </summary>
        /// <returns>An <see cref="ISyncPolicy"/> containing defined policies for an <see cref="IFlurlResponse"/>.</returns>
        ISyncPolicy GetPolicy();

        /// <summary>
        /// Creates an <see cref="IAsyncPolicy"/> containing the Circuit Breaker and Retry policy for handling a <see cref="HttpResponse"/>.
        /// </summary>
        /// <returns>An <see cref="IAsyncPolicy"/> containing defined policies for an <see cref="IFlurlResponse"/>.</returns>
        IAsyncPolicy GetPolicyAsync();
    }
}
