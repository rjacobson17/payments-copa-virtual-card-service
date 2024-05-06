using COPA.APIManager.Utilities.Interfaces;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace COPA.APIManager.Utilities.Implementations
{
    /// <summary>
    /// Contains the Polly policies for handling external api responses.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HttpResiliencePolicy : IHttpResiliencePolicy
    {
        private readonly ILogger<HttpResiliencePolicy> _logger;
        private readonly int _retryCount;
        private readonly int _circuitBreakerExceptionCount;
        private readonly int _circuitBreakerOpenInterval;
        private readonly HttpStatusCode[] retryStatusCodes =
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.NotImplemented,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.TooManyRequests
        };

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="config">Instance of <see cref="IConfiguration"/> interface.</param>
        /// <param name="logger">The concrete implementation of <see cref="ILogger"/> that will handle log messages.</param>
        public HttpResiliencePolicy(IConfiguration config, ILogger<HttpResiliencePolicy> logger)
        {
            _retryCount = config.GetValue<int>("Polly:RetryCount");
            _circuitBreakerOpenInterval = config.GetValue<int>("Polly:CircuitBreakerOpenMinutes");
            _circuitBreakerExceptionCount = config.GetValue<int>("Polly:CircuitBreakerExceptionCount");
            _logger = logger;
        }

        #region Polly Sync Policy

        /// <inheritdoc/>
        public ISyncPolicy GetPolicy() => CircuitBreakerPolicy().Wrap(RetryPolicy());

        private ISyncPolicy CircuitBreakerPolicy()
        {
            return Policy.Handle<Exception>()
                .Or<FlurlHttpException>()
                .Or<FlurlHttpTimeoutException>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: _circuitBreakerExceptionCount,
                    durationOfBreak: TimeSpan.FromMinutes(_circuitBreakerOpenInterval),
                    onBreak: (ex, t) =>
                    {
                        _logger.LogError(ex, $"Circuit broken, Reopening in: {t} minutes.");
                    },
                    onReset: () =>
                    {
                        _logger.LogTrace("Circuit Reset");
                    }
                );
        }

        private ISyncPolicy RetryPolicy()
        {
            return Policy
                .Handle<Exception>()
                .Or<FlurlHttpException>()
                .Or<FlurlHttpTimeoutException>()
                .WaitAndRetry(
                    retryCount: _retryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(value: Math.Pow(x: 5, y: retryAttempt)),
                    onRetry: (exception, timeSpan, retries, context) =>
                    {
                        _logger.LogTrace($"Polly attempt: {retries}");
                    });
        }

        #endregion

        #region Polly Async Policy

        /// <inheritdoc/>
        public IAsyncPolicy GetPolicyAsync() => CircuitBreakerPolicyAsync().WrapAsync(RetryPolicyAsync());

        private IAsyncPolicy CircuitBreakerPolicyAsync()
        {
            return Policy.Handle<Exception>()
                .Or<FlurlHttpException>()
                .Or<FlurlHttpTimeoutException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _circuitBreakerExceptionCount,
                    durationOfBreak: TimeSpan.FromMinutes(_circuitBreakerOpenInterval),
                    onBreak: (ex, t) =>
                    {
                        _logger.LogError(ex, $"Circuit broken, Reopening in: {t} minutes.");
                    },
                    onReset: () =>
                    {
                        _logger.LogTrace("Circuit Reset");
                    }
                );
        }

        private IAsyncPolicy RetryPolicyAsync()
        {
            return Policy
                .Handle<Exception>()
                .Or<FlurlHttpException>()
                .Or<FlurlHttpTimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: _retryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(value: Math.Pow(x: 5, y: retryAttempt)),
                    onRetry: (exception, timeSpan, retries, context) =>
                    {
                        _logger.LogTrace($"Polly reattempt: {retries}");
                    });
        }

        #endregion

        private bool IsTemporaryStatusCode(int statusCode) => retryStatusCodes.Contains((HttpStatusCode)statusCode);
    }
}
