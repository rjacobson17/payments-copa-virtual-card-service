using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BLL.Implementation;
using COPA.APIManager.BLL.Interface;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;
using COPA.APIManager.DAL.Interface;
using COPA.APIManager.Utilities.Interfaces;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;

[assembly: InternalsVisibleTo("COPA.APIManager.Tests")]
namespace COPA.APIManager.DAL.Implementation
{
    /// <summary>
    /// Implementation of calls to the CSI API
    /// </summary>
    public class CSIApiAccess : ICSIApiAccess
    {
        private readonly string _url;
        private readonly ICSIRateCompliance _rateCompliance;
        private readonly IAsyncPolicy _httpPolicy;


        /// <summary>
        /// Default Constructor. Initializes fields through DI
        /// </summary>
        /// <param name="config">Concrete implementation of the <see cref="IConfiguration"/> interface.</param>
        public CSIApiAccess(IConfiguration config, IHttpResiliencePolicy policy)
        {
            _url = config.GetValue<string>("ServicePrefixes:CSI");
            _rateCompliance = new CSIRateCompliance(config);
            _httpPolicy = policy.GetPolicyAsync();
        }

        /// <inheritdoc/>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> GetConserviceCardInfoAsync(string token, string cardId, string confidentialClientId, CancellationToken cancel)
        {
            Func<CancellationToken, Task<IFlurlResponse>> apiCallFunc = ct => _httpPolicy.ExecuteAsync(async () => await _url
                .AllowAnyHttpStatus()
                .AppendPathSegment(cardId)
                .WithHeaders(new
                {
                    Content_Type = "application/json",
                    Accept = "application/json",
                    AUTH = token
                })
                .GetAsync(ct));

            return await ExecuteApiCall(apiCallFunc, cancel, confidentialClientId);
        }

        /// <inheritdoc/>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> CreateConserviceCardAsync(string token, CSICardRequest request, string confidentialClientId, CancellationToken cancel)
        {
            Func<CancellationToken, Task<IFlurlResponse>> apiCallFunc = ct => _httpPolicy.ExecuteAsync(async () => await _url
                .AllowAnyHttpStatus()
                .WithHeaders(new
                {
                    Content_Type = "application/json",
                    Accept = "application/json",
                    AUTH = token
                })
                .PostJsonAsync(new
                {
                    amount = request.Amount,
                    exactAmount = request.ExactAmount,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    expirationMonth = request.ExpirationMonth,
                    expirationYear = request.ExpirationYear,
                    numberOfTransactions = request.NumberOfTransactions,
                    poNumber = request.PoNumber,
                    binRotation = request.BinRotation,
                    supplierId = request.SupplierID,
                    invoiceNumber = request.ClientName
                }, ct));

            return await ExecuteApiCall(apiCallFunc, cancel, confidentialClientId, clientName: request.ClientName);
        }

        /// <inheritdoc/>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> UpdateConserviceCardAsync(string token, CSIUpdateRequest request, string confidentialClientId, CancellationToken cancel)
        {
            Func<CancellationToken, Task<IFlurlResponse>> apiCallFunc = ct => _httpPolicy.ExecuteAsync(async () => await _url
                .AllowAnyHttpStatus()
                .AppendPathSegment(request.ID)
                .WithHeaders(new
                {
                    Content_Type = "application/json",
                    Accept = "application/json",
                    AUTH = token
                })
                .PutJsonAsync(new
                {
                    amount = request.Amount,
                    exactAmount = request.ExactAmount,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    expirationMonth = request.ExpirationMonth,
                    expirationYear = request.ExpirationYear,
                    notes = request.Notes
                }, ct));

            return await ExecuteApiCall(apiCallFunc, cancel, confidentialClientId);
        }

        /// <inheritdoc/>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> BlockCardAsync(string token, string cardID, string confidentialClientId, CancellationToken cancel)
        {
            Func<CancellationToken, Task<IFlurlResponse>> apiCallFunc = ct => _httpPolicy.ExecuteAsync(async () => await _url
                .AllowAnyHttpStatus()
                .AppendPathSegment(cardID)
                .AppendPathSegment("block")
                .WithHeaders(new
                {
                    Content_Type = "application/json",
                    Accept = "application/json",
                    AUTH = token
                })
                .PutAsync(null, ct));

            return await ExecuteApiCall(apiCallFunc, cancel, confidentialClientId);
        }

        internal async Task<TypeResult<IVirtualCard, CSIErrorResponse>> ExecuteApiCall(Func<CancellationToken, Task<IFlurlResponse>> apiCall, CancellationToken cancel, string confidentialClientId, string clientName = null, [CallerMemberName] string callMemberName = "")
        {
            TypeResult<IVirtualCard, CSIErrorResponse> result = null;
            string observedStatusCode = CSIErrorResponse.DefaultUninitializedResponseCode;
            string errorMessage = null;

            CSIConstraints constraints = _rateCompliance.GetUsageLimits();
            CSIUsage usage = new () // initialize as maxxed out. execution will set it to the actual value.
            {
                CurrentParallelization = constraints.MaxParallelization,
                CurrentPendingQueueActions = constraints.MaxPendingQueueActions
            };
            try
            {
                Func<Task<IFlurlResponse>> throttledResponseFunc = () => _rateCompliance.ExecuteAsync(async ct =>
                {
                    usage = _rateCompliance.GetCurrentUsage();
                    return await apiCall(ct);
                }, cancel);

                IFlurlResponse cardResponse = await Task.Run(throttledResponseFunc);

                observedStatusCode = cardResponse.StatusCode.ToString();
                result = await HandleResponse(cardResponse, _url.AllowAnyHttpStatus().Settings.JsonSerializer);

                errorMessage = result.Failed ? result.Failure.RawBody : null;
            }
            catch (BulkheadRejectedException rejected)
            {
                errorMessage = "VCard API Self-throttling prevented this request. " + rejected.Message;
                CSIErrorResponse throttled = new CSIErrorResponse()
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                    RawBody = errorMessage
                };
                result = TypeResult<IVirtualCard, CSIErrorResponse>.Fail(throttled);
            }
            catch (OperationCanceledException e)
            {
                // this is a timeout.
                errorMessage = "VCard API request canceled, possibly due to timed out. " + e.Message;
                CSIErrorResponse error = new CSIErrorResponse()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    RawBody = errorMessage
                };
                result = TypeResult<IVirtualCard, CSIErrorResponse>.Fail(error);
            }
            catch (Exception e)
            {
                errorMessage = $"VCard API request error. [{e.GetType().Name}] " + e.Message;
                CSIErrorResponse error = new CSIErrorResponse()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    RawBody = errorMessage
                };
                result = TypeResult<IVirtualCard, CSIErrorResponse>.Fail(error);
            }

            return result;
        }

        /// <summary>
        /// Here we have to handle some jank. Strap in.
        ///
        /// CSI returns 200 with a card. CSI also returns a 200 with error. It also returns 500 with error. CSI can
        /// also return with a 400 error, which seems to indicate that they've hit a limit with their partners. CSI can
        /// also return a 403 error. Not sure what these mean.
        ///
        /// One such error a 200|500 represents is a throttle limit error, identified by the string
        /// "Not Authorized after max concurrent connections."
        ///
        /// We have to identify if it is an error response or not. And for error responses, if it is a throttling error
        /// or some other error. The easiest way is to check for a successful response code, and within that an actual
        /// card. In that constrained case, return it. Otherwise, treat it as a failed response, guessing when it is a
        /// real or intended throttle response.
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="serializer"></param>
        /// <param name="operationName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<TypeResult<IVirtualCard, CSIErrorResponse>> HandleResponse(IFlurlResponse response, ISerializer serializer)
        {
            string responseBody = await response.GetStringAsync();

            if (response.ResponseMessage.IsSuccessStatusCode)
            {
                CSICardResponse card = serializer.Deserialize<CSICardResponse>(responseBody);
                bool hasCardInBody = !string.IsNullOrEmpty(card.CardNumber);

                if (hasCardInBody)
                {
                    return TypeResult<IVirtualCard, CSIErrorResponse>.Succeed(card.MapCard());
                }
            }

            // time to do the hokey-pokey. we don't have a successful response with a card, so it is non-success.
            CSIError error = serializer.Deserialize<CSIError>(responseBody);

            bool isThrottleError = error.ContainsThrottleMessage();

            bool isThrottleResponse = response.ResponseMessage.StatusCode == HttpStatusCode.TooManyRequests || isThrottleError;

            CSIErrorResponse errorResponse = new CSIErrorResponse()
            {
                Error = error,
                // we editorialize their non-success response code for our callers: nothing they can do. nothing we can do.
                StatusCode = isThrottleResponse ? HttpStatusCode.TooManyRequests : HttpStatusCode.InternalServerError,
                ObservedStatusCode = response.ResponseMessage.StatusCode,
                RawBody = responseBody
            };

            return TypeResult<IVirtualCard, CSIErrorResponse>.Fail(errorResponse);
        }
    }

    public static class CSIErrorStringExtension
    {
        public static bool ContainsThrottleMessage(this CSIError error)
        {
            // this is a magic string as provided by them until they provide 429 responses.
            // As of a meeting on 2024-01-17, they will not be changing their API V2 behavior.
            return (error?.Error ?? "").Contains("Not Authorized after max concurrent connections");
        }
    }
}
