using COPA.APIManager.BOL;
using COPA.APIManager.DAL.Implementation;
using Flurl.Http.Testing;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;
using COPA.APIManager.BOL.Interfaces;
using Flurl.Http;
using Moq;
using COPA.APIManager.Utilities.Interfaces;

namespace COPA.APIManager.Tests.DAL.Implementation
{
    public class CSIApiAccessShould
    {
        private HttpTest _httpTest;
        private CSIApiAccess _access;

        private readonly string _url = "https://unittest.vcapi.com";
        // zero retries means faster unit test runs
        private readonly int _retryCount = 0;
        private readonly int _circuitBreakerExceptionCount = 1;
        private readonly int _circuitBreakerOpenInterval = 2;
        private IHttpResiliencePolicy _resiliencePolicy;

        private readonly int maxParallelization = 9;
        private readonly int maxQueueDepth = 4;

        [SetUp]
        public void Setup()
        {
            _httpTest = new HttpTest();
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ServicePrefixes:CSI", _url },
                    { "CardSettings:CSI:MaxParallelization", maxParallelization.ToString() },
                    { "CardSettings:CSI:MaxPendingQueueActions", maxQueueDepth.ToString() }

                }).Build();
            _resiliencePolicy = Mock.Of<IHttpResiliencePolicy>();
            _access = new CSIApiAccess(config, _resiliencePolicy);
        }

        [TearDown]
        public void Teardown()
        {
            _httpTest.Dispose();
        }

        [Test]
        public async Task GetConserviceCardInfo_Success()
        {
            var card = GetCSICardResponse();

            var token = "unit_test_token";
            var cardId = "1234";
            string paymentIdNumber = "6789";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Get)
                .RespondWithJson(card);

            _ = await _access.GetConserviceCardInfoAsync(token, cardId, confidential, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"{_url}/{cardId}");
        }

        [Test]
        public async Task GetConserviceCardInfo_EmptyResponse()
        {
            var card = GetEmptyCSICardResponse();

            var token = "unit_test_token";
            var cardId = "1234";
            string paymentIdNumber = "6789";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Get)
                .RespondWithJson(card);

            try
            {
                _ = await _access.GetConserviceCardInfoAsync(token, cardId, confidential, CancellationToken.None);
                Assert.Fail("Should have thrown exception for empty card.");
            }
            catch (Exception e)
            {
                Assert.Pass("This is The Way");
            }

            _httpTest.ShouldHaveCalled($"{_url}/{cardId}");
        }

        [Test]
        public async Task GetConserviceCardInfo_ServerError()
        {
            var card = GetCSICardResponse();

            var token = "unit_test_token";
            var cardId = "1234";
            string paymentIdNumber = "6789";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Get)
                .RespondWithJson(card, 500);

            try
            {
                _ = await _access.GetConserviceCardInfoAsync(token, cardId, confidential, CancellationToken.None);
            }
            catch (Exception e)
            {
                Assert.NotNull(e.Message);
            }

            _httpTest.ShouldHaveCalled($"{_url}/{cardId}");
        }

        [Test]
        public async Task CreateConserviceCard_Success()
        {
            var card = GetCSICardResponse();
            var request = GetCSICardRequest();
            var token = "unit_test_token";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Post)
                .RespondWithJson(card);

            _ = await _access.CreateConserviceCardAsync(token, request, confidential, CancellationToken.None);
        }

        [Test]
        public async Task CreateConserviceCard_EmptyResponse()
        {
            var card = GetEmptyCSICardResponse();
            var request = GetCSICardRequest();
            var token = "unit_test_token";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Post)
                .RespondWithJson(card);

            try
            {
                _ = await _access.CreateConserviceCardAsync(token, request, confidential, CancellationToken.None);
                Assert.Fail("Should have thrown exception for empty card.");
            }
            catch (Exception e)
            {
                Assert.Pass("This is The Way");
            }
        }

        [Test]
        public async Task CreateConserviceCard_ServerError()
        {
            var card = GetCSICardResponse();
            var request = GetCSICardRequest();
            var token = "unit_test_token";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Post)
                .RespondWithJson(card, 500);

            try
            {
                _ = await _access.CreateConserviceCardAsync(token, request, confidential, CancellationToken.None);
            }
            catch (Exception e)
            {
                Assert.NotNull(e.Message);
            }
        }

        [Test]
        public async Task UpdateConserviceCard_Success()
        {
            var card = GetCSICardResponse();
            var request = GetCSIUpdateRequest();
            var token = "unit_test_token";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Put)
                .RespondWithJson(card);

            _ = await _access.UpdateConserviceCardAsync(token, request, confidential, CancellationToken.None);
        }

        [Test]
        public async Task UpdateConserviceCard_ServerError()
        {
            var card = GetCSICardResponse();
            var request = GetCSIUpdateRequest();
            var token = "unit_test_token";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Put)
                .RespondWithJson(card, 500);

            try
            {
                _ = await _access.UpdateConserviceCardAsync(token, request, confidential, CancellationToken.None);
            }
            catch (Exception e)
            {
                Assert.NotNull(e.Message);
            }
        }

        [Test]
        public async Task BlockCard_Success()
        {
            var card = GetCSICardResponse();

            var token = "unit_test_token";
            var cardId = "1234";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Put)
                .RespondWithJson(card);

            _ = await _access.BlockCardAsync(token, cardId, confidential, CancellationToken.None);

            _httpTest.ShouldHaveCalled($"{_url}/{cardId}/block");
        }

        [Test]
        public async Task BlockCard_ServerError()
        {
            var card = GetCSICardResponse();

            var token = "unit_test_token";
            var cardId = "1234";
            string confidential = "confidential";

            _httpTest
                .ForCallsTo($"{_url}*")
                .WithHeader("AUTH", token)
                .WithVerb(HttpMethod.Put)
                .RespondWithJson(card, 500);

            try
            {
                _ = await _access.BlockCardAsync(token, cardId, confidential, CancellationToken.None);
            }
            catch(Exception e)
            {
                Assert.NotNull(e.Message);
            }

            _httpTest.ShouldHaveCalled($"{_url}/{cardId}/block");
        }

        [Test]
        public async Task ExecuteApiCall_BreakBulkhead()
        {
            string confidentialClient = "confidential";
            int shouldAccept = maxParallelization + maxQueueDepth;

            SemaphoreSlim shady = new SemaphoreSlim(0, shouldAccept);

            Func<CancellationToken, Task<IFlurlResponse>> apiCall = ct => Task.Run(async () =>
            {
                await shady.WaitAsync(ct);
                return (IFlurlResponse)null;
            });

            for (int i = 0; i < shouldAccept; i++)
            {
                // no need to wait for this guy ... fill it up.
                _access.ExecuteApiCall(apiCall, CancellationToken.None, confidentialClient);
            }

            TypeResult<IVirtualCard, CSIErrorResponse> execute = await _access.ExecuteApiCall(apiCall, CancellationToken.None, confidentialClient);

            Assert.True(execute.Failed);
            Assert.That(execute.Failure.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(execute.Failure.IsThrottleResponse, Is.True);
        }

        [Test]
        public async Task ExecuteApiCall_CutsItClose()
        {
            string confidentialClient = "confidential";
            int shouldAccept = maxParallelization + maxQueueDepth;
            // a completely unexpected response
            HttpStatusCode responseCode = HttpStatusCode.RedirectKeepVerb;
            string amazingError = "This is an amazing error";
            string errorBody = "{ \"Error\": \"" + amazingError + "\" }";

            SemaphoreSlim shady = new SemaphoreSlim(0, shouldAccept);

            Mock<IFlurlResponse> mockResponse = new Mock<IFlurlResponse>();
            mockResponse.SetupGet(x => x.StatusCode).Returns((int)responseCode);
            mockResponse.SetupGet(x => x.ResponseMessage).Returns(new HttpResponseMessage(responseCode));
            mockResponse.Setup(x => x.GetStringAsync()).Returns(Task.FromResult(errorBody));

            Func<CancellationToken, Task<IFlurlResponse>> apiCall = ct => Task.Run(async () =>
            {
                await shady.WaitAsync(ct);
                return mockResponse.Object;
            });

            List<Task<TypeResult<IVirtualCard, CSIErrorResponse>>> tasks = new();
            for (int i = 0; i < shouldAccept; i++)
            {
                // no need to wait for this guy ... fill it up.
                Task<TypeResult<IVirtualCard, CSIErrorResponse>> task = _access.ExecuteApiCall(apiCall, CancellationToken.None, confidentialClient);
                tasks.Add(task);
            }

            shady.Release(shouldAccept);

            TypeResult<IVirtualCard, CSIErrorResponse>[] results = await Task.WhenAll(tasks);

            foreach (var typeResult in results)
            {
                Assert.That(typeResult.Succeeded, Is.False);
                // the lack of a card means this was an error ...
                Assert.That(typeResult.Failure.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
                Assert.That(typeResult.Failure.ObservedStatusCode, Is.EqualTo(responseCode));
                Assert.That(typeResult.Failure.RawBody, Contains.Substring(amazingError));
                Assert.That(typeResult.Failure.IsThrottleResponse, Is.False);
            }
        }

        private static CSICardRequest GetCSICardRequest() => new()
        {
            Amount = 12M,
            ExactAmount = true,
            FirstName = "FirstName",
            LastName = "LastName",
            ExpirationMonth = 01,
            ExpirationYear = 99,
            NumberOfTransactions = 4,
            PoNumber = "PoNumber",
            BinRotation = true,
            SupplierID = "SupplierID",
            InvoiceNumber = "InvoiceNumber"
        };

        private static CSIUpdateRequest GetCSIUpdateRequest() => new()
        {
            Amount = 12M,
            ExactAmount = true,
            FirstName = "FirstName",
            LastName = "LastName",
            ExpirationMonth = 01,
            ExpirationYear = 99,
            Notes = "Notes"
        };

        private static CSICardResponse GetEmptyCSICardResponse() => new()
        {

        };

        private static CSICardResponse GetCSICardResponse() => new()
        {
            Amount = "1234",
            Blocked = "false",
            BinRotation = true,
            CardNumber = "CardNumber",
            Created = "Created",
            Cvc2 = "Cvc2",
            ExactAmount = "ExactAmount",
            ExpirationMMYY = "ExpirationMMYY",
            ExternalToken = "ExternalToken",
            FirstName = "FirstName",
            Id = "Id",
            InvoiceNumber = "InvoiceNumber",
            LastFour = "LastFour",
            LastName = "LastName",
            Notes = "Notes",
            NumberOfTransactions = "NumberOfTransactions",
            PoNumber = "PoNumber",
            SupplierID = "SupplierID",
            UsageTypes = new List<string>() { "UsageTypes" }
        };
    }
}