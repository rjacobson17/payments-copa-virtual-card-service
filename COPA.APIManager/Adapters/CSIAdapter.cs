using COPA.APIManager.Adapters;
using COPA.APIManager.BOL.Interfaces;
using System;

namespace COPA.APIManager.BOL
{
    /// <summary>
    /// Converts a CSICardResponse object into a Virtual Card object.
    /// </summary>
    public class CSIAdapter : IVirtualCard
    {
        /// <summary>
        /// Name of the client the card belongs to.
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// Unique identifier assigned to a virtual card.
        /// </summary>
        public string VirtualCardID { get; set; }
        /// <summary>
        /// Determines if the virtual card is active or blocked.
        /// </summary>
        public string IsBlocked { get; set; }
        /// <summary>
        /// The credit card number for a virtual card.
        /// </summary>
        public string CreditCardNumber { get; set; }
        /// <summary>
        /// The security code for a virtual card.
        /// </summary>
        public string SecurityCode { get; set; }
        /// <summary>
        /// The expiration month for a virtual card.
        /// </summary>
        public string ExpirationMonth { get; set; }
        /// <summary>
        /// The expiration year for a virtual card.
        /// </summary>
        public string ExpirationYear { get; set; }
        /// <summary>
        /// Description of error that occurred when creating a virtual card.
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// External Token, id to track card history
        /// </summary>
        public string VirtualCardExternalToken { get; set; }

        private string _cardNetwork;
        /// <summary>
        /// The name of the card network the card is affilated with.
        /// </summary>
        public string CardNetwork
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_cardNetwork) && !String.IsNullOrWhiteSpace(CreditCardNumber))
                    _cardNetwork = AdapterTools.SetCardNetwork(CreditCardNumber.Substring(0, 1));

                return _cardNetwork;
            }
            set
            {
                _cardNetwork = value;
            }
        }

        /// <summary>
        /// Converts a CSICardResponse object into a VirtualCard object.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>An object containing the information for a virtual card.</returns>
        public IVirtualCard AdaptResponse(CSICardResponse response)
        {
            return new CSIAdapter
            {
                VirtualCardID = response.Id,
                IsBlocked = response.Blocked,
                CreditCardNumber = response.CardNumber,
                SecurityCode = response.Cvc2,
                ExpirationMonth = response.ExpirationMMYY?.Substring(0, 2),
                ExpirationYear = response.ExpirationMMYY?.Substring(2, 2),
                VirtualCardExternalToken = response.ExternalToken
            };
        }
    }
}
