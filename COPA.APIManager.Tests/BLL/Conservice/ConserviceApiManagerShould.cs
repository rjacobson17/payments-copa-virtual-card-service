using COPA.APIManager.BLL;
using COPA.APIManager.BLL.Clients.Conservice;
using COPA.APIManager.BLL.Interface;
using COPA.APIManager.DAL.Interface;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;

namespace COPA.APIManager.Tests.BLL.Conservice
{
    public class ConserviceApiManagerShould
    {
        private ICSIApiAccess access;
        private IConfiguration _config;
        private ICredentialsManager credentialsManager;
        private ConserviceApiManager apiManager;

        private const bool EXACT_AMMOUNT = true;
        private const int NUMBER_OF_TRANSACTIONS = 4;
        private const int EXPIRATION_DATE_OFFSET = 190;

        [SetUp]
        public void Setup()
        {
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "ServicePrevixes:CSI", "" },
                    { "CardSettings:ExactAmount", EXACT_AMMOUNT.ToString() },
                    { "CardSettings:NumberOfTransactions", NUMBER_OF_TRANSACTIONS.ToString() },
                    { "CardSettings:ExpirationDateOffset", EXPIRATION_DATE_OFFSET.ToString() }
                }).Build();

            access = Mock.Of<ICSIApiAccess>();

            credentialsManager = Mock.Of<ICredentialsManager>();

            apiManager = new ConserviceApiManager(_config, access, credentialsManager);
        }

        [Test]
        public async Task BlockCard()
        {
            string token = "UnitTestToken";
            string clientID = "100";
            string cardId = "ABC123";
            string confidential = "confidential";
            Mock.Get(credentialsManager).Setup(m => m.GetToken()).Returns(token).Verifiable();
            Mock.Get<ICSIApiAccess>(access).Setup(m => m.BlockCardAsync(token, cardId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(TypeResult<IVirtualCard, CSIErrorResponse>.Succeed(new BOL.CSIAdapter()));

            var card = await apiManager.BlockCard(clientID, cardId, confidential);

            Mock.Get<ICSIApiAccess>(access).Verify(m => m.BlockCardAsync(token, cardId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(credentialsManager).Verify(m => m.GetToken(), Times.Once);
        }

        [Test]
        public async Task CreateConserviceCard()
        {
            string token = "UnitTestToken";
            int clientID = 100;
            string confidential = "confidential";

            DateTime expirationDay = DateTime.Now.AddDays(EXPIRATION_DATE_OFFSET);
            int expectedExpirationMonth = Int32.Parse(expirationDay.ToString("MM"));
            int expectedExpirationYear = Int32.Parse(expirationDay.ToString("yy"));

            BOL.CSICardRequest request = new()
            {
                ClientID = clientID,
            };

            Mock.Get(credentialsManager).Setup(m => m.GetToken()).Returns(token).Verifiable();
            Mock.Get<ICSIApiAccess>(access).Setup(m => m.CreateConserviceCardAsync(token, It.IsAny<BOL.CSICardRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(TypeResult<IVirtualCard, CSIErrorResponse>.Succeed(new BOL.CSIAdapter()));

            var card = await apiManager.CreateConserviceCard(request, confidential);

            Mock.Get<ICSIApiAccess>(access).Verify(m => m.CreateConserviceCardAsync(token, It.Is<BOL.CSICardRequest>(p => verifyCreateRequest(p, clientID, expectedExpirationMonth, expectedExpirationYear)), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(credentialsManager).Verify(m => m.GetToken(), Times.Once);
        }
        private bool verifyCreateRequest(BOL.CSICardRequest request, int expectedClientID, int expectedExpirationMonth, int expectedExpirationYear)
        {
            return request.ClientID == expectedClientID
                && request.ExpirationMonth == expectedExpirationMonth
                && request.ExpirationYear == expectedExpirationYear
                && request.ExactAmount == EXACT_AMMOUNT
                && request.NumberOfTransactions == NUMBER_OF_TRANSACTIONS;
        }

        [Test]
        public async Task GetConserviceCardInfo()
        {
            string token = "UnitTestToken";
            string clientID = "100";
            string cardId = "ABC123";
            string paymentId = "paymentId";
            string confidential = "confidential";

            TypeResult<IVirtualCard, CSIErrorResponse> failApiCall = TypeResult<IVirtualCard, CSIErrorResponse>.Fail(new CSIErrorResponse()
                { RawBody = "Test", Error = new CSIError() { Error = "Test" } });

            Mock.Get(credentialsManager).Setup(m => m.GetToken()).Returns(token).Verifiable();
            Mock.Get<ICSIApiAccess>(access).Setup(m => m.GetConserviceCardInfoAsync(token, cardId, confidential, It.IsAny<CancellationToken>())).ReturnsAsync(failApiCall);

            var card = await apiManager.GetConserviceCardInfo(clientID, cardId, confidential);

            Assert.False(card.Succeeded);
            Mock.Get<ICSIApiAccess>(access).Verify(m => m.GetConserviceCardInfoAsync(token, cardId, confidential, It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(credentialsManager).Verify(m => m.GetToken(), Times.Once);
        }

        [Test]
        public async Task UpdateConserviceCard()
        {
            string token = "UnitTestToken";
            int clientID = 100;
            string confidential = "confidential";

            DateTime expirationDay = DateTime.Now.AddDays(EXPIRATION_DATE_OFFSET);
            int expectedExpirationMonth = Int32.Parse(expirationDay.ToString("MM"));
            int expectedExpirationYear = Int32.Parse(expirationDay.ToString("yy"));

            BOL.CSIUpdateRequest request = new()
            {
                ClientID = clientID,
            };

            Mock.Get(credentialsManager).Setup(m => m.GetToken()).Returns(token).Verifiable();
            Mock.Get<ICSIApiAccess>(access).Setup(m => m.UpdateConserviceCardAsync(token, It.IsAny<BOL.CSIUpdateRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(TypeResult<IVirtualCard, CSIErrorResponse>.Succeed(new BOL.CSIAdapter()));

            var card = await apiManager.UpdateConserviceCard(request, confidential);

            Mock.Get<ICSIApiAccess>(access).Verify(m => m.UpdateConserviceCardAsync(token, It.Is<BOL.CSIUpdateRequest>(p => VerifyUpdateRequest(p, clientID, expectedExpirationMonth, expectedExpirationYear)), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(credentialsManager).Verify(m => m.GetToken(), Times.Once);

        }
        private bool VerifyUpdateRequest(BOL.CSIUpdateRequest request, int expectedClientID, int expectedExpirationMonth, int expectedExpirationYear)
        {
            return request.ClientID == expectedClientID
                && request.ExpirationMonth == expectedExpirationMonth
                && request.ExpirationYear == expectedExpirationYear
                && request.ExactAmount == EXACT_AMMOUNT;
        }
    }
}