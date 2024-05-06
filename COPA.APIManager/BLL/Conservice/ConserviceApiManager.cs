using COPA.APIManager.BLL.Interface;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;
using COPA.APIManager.DAL.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;

namespace COPA.APIManager.BLL.Clients.Conservice
{
    /// <summary>
    /// Contains the methods used to communicate with CSI's API through Conservice.
    /// </summary>
    public class ConserviceApiManager
    {
        private readonly bool _exactAmount;
        private readonly int _numberOfTransactions;
        private readonly int _csiTimeoutSeconds;
        public readonly int _expirationDateOffset;

        private readonly ICSIApiAccess _apiAccess;
        private readonly IConfiguration _config;
        private readonly ICredentialsManager _credentialsManager;

        /// <summary>
        /// Constructor. Initializes fields through DI
        /// </summary>
        /// <param name="config">Instance of appsettings configuration values</param>
        /// <param name="access">Instance used to access the CSI API</param>
        /// <param name="credentialsManager">Instance used to retrieve credentials</param>
        public ConserviceApiManager(IConfiguration config, ICSIApiAccess access, ICredentialsManager credentialsManager)
        {
            _apiAccess = access;
            _config = config;
            _credentialsManager = credentialsManager;
            _exactAmount = config.GetValue<bool>("CardSettings:ExactAmount", false);
            _numberOfTransactions = config.GetValue<int>("CardSettings:NumberOfTransactions", 4);
            _expirationDateOffset = config.GetValue<int>("CardSettings:ExpirationDateOffset", 190);
            _csiTimeoutSeconds = config.GetValue<int>("CardSettings:CSI:TimeoutInSeconds", 60);
        }

        /// <summary>
        /// Gets an existing virtual card.
        /// </summary>
        /// <param name="clientID">The ID assigned to a Conservice client.</param>
        /// <param name="cardId">The ID of the virtual card to get.</param>
        /// <returns>IVirtualCard containing information regarding the vitual card.</returns>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> GetConserviceCardInfo(string clientID, string cardId, string confidentialClientId)
        {
            string token = _credentialsManager.GetToken();

            CancellationToken cancel = new CancellationTokenSource(TimeSpan.FromSeconds(_csiTimeoutSeconds)).Token;
            TypeResult<IVirtualCard, CSIErrorResponse> result = await _apiAccess.GetConserviceCardInfoAsync(token, cardId, confidentialClientId, cancel);
            if (result.Succeeded)
            {
                result.Value.ClientName = ClientNameFactory.AsClientName(clientID);
            }
            return result;
        }

        /// <summary>
        /// Sends an api call to CSI to create a virtual card.
        /// </summary>
        /// <param name="request">Object containing the information to create a virtual card.</param>
        /// <returns>IVirtualCard containing information regarding the vitual card.</returns>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> CreateConserviceCard(CSICardRequest request, string confidentialClientId)
        {
            string token = _credentialsManager.GetToken();
            string clientName = ClientNameFactory.AsClientName(request.ClientID);
            var expirationDate = DateTime.Now + TimeSpan.FromDays(_expirationDateOffset);

            request.ExpirationMonth = Convert.ToInt32(expirationDate.ToString("MM"));
            request.ExpirationYear = Convert.ToInt32(expirationDate.ToString("yy"));
            request.ExactAmount = _exactAmount;
            request.NumberOfTransactions = _numberOfTransactions;
            request.ClientName = clientName;

            CancellationToken cancel = new CancellationTokenSource(TimeSpan.FromSeconds(_csiTimeoutSeconds)).Token;
            TypeResult<IVirtualCard, CSIErrorResponse> result = await _apiAccess.CreateConserviceCardAsync(token, request, confidentialClientId, cancel);
            if (result.Succeeded)
            {
                result.Value.ClientName = clientName;
            }
            return result;
        }

        /// <summary>
        /// Sends an api call to CSI to update a card's values
        /// </summary>
        /// <param name="request">Object containing required information to update a given card</param>
        /// <returns>Virtual card that can be used for payments</returns>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> UpdateConserviceCard(CSIUpdateRequest request, string confidentialClientId)
        {
            string token = _credentialsManager.GetToken();
            var expirationDate = DateTime.Now + TimeSpan.FromDays(_expirationDateOffset);

            request.ExpirationMonth = Convert.ToInt32(expirationDate.ToString("MM"));
            request.ExpirationYear = Convert.ToInt32(expirationDate.ToString("yy"));
            request.ExactAmount = _exactAmount;

            CancellationToken cancel = new CancellationTokenSource(TimeSpan.FromSeconds(_csiTimeoutSeconds)).Token;
            TypeResult<IVirtualCard, CSIErrorResponse> result = await _apiAccess.UpdateConserviceCardAsync(token, request, confidentialClientId, cancel);
            if (result.Succeeded)
            {
                result.Value.ClientName = ClientNameFactory.AsClientName(request.ClientID);
            }
            return result;
        }

        /// <summary>
        /// Sends an API request to CSI for a card be blocked to not be used anymore
        /// </summary>
        /// <param name="clientID">The ID assigned to a Conservice client.</param>
        /// <param name="cardID">The ID of the virtual card to be blocked.</param>
        /// <returns>IVirtualCard containing information regarding the blocked vitual card.</returns>
        public async Task<TypeResult<IVirtualCard, CSIErrorResponse>> BlockCard(string clientID, string cardID, string confidentialClientId)
        {
            string token = _credentialsManager.GetToken();

            CancellationToken cancel = new CancellationTokenSource(TimeSpan.FromSeconds(_csiTimeoutSeconds)).Token;
            TypeResult<IVirtualCard, CSIErrorResponse> result = await _apiAccess.BlockCardAsync(token, cardID, confidentialClientId, cancel);
            if (result.Succeeded)
            {
                result.Value.ClientName = ClientNameFactory.AsClientName(clientID);
            }
            return result;
        }
    }
}
