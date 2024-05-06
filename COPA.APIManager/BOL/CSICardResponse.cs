using COPA.APIManager.BOL.Interfaces;
using System.Collections.Generic;

namespace COPA.APIManager.BOL
{
    /// <summary>
    /// Maps a CSI vcard JSON response message
    /// </summary>
    public class CSICardResponse
    {

        /// <summary>
        /// The total amount allowed for the virtual card.
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// Determines if a card is blocked or not.
        /// </summary>
        public string Blocked { get; set; }
        /// <summary>
        /// Determines if the Bin Rotation strategy was used or not.
        /// </summary>
        public bool BinRotation { get; set; }
        /// <summary>
        /// The credit card number pretaining to the virtual card.
        /// </summary>
        public string CardNumber { get; set; }
        /// <summary>
        /// The date the virtual card was created.
        /// </summary>
        public string Created { get; set; }
        /// <summary>
        /// The security code pretaining to the virtual card.
        /// </summary>
        public string Cvc2 { get; set; }
        /// <summary>
        /// Determines if the amount has to match or not.
        /// </summary>
        public string ExactAmount { get; set; }
        /// <summary>
        /// The expiration date pretaining to the virtual card.
        /// </summary>
        public string ExpirationMMYY { get; set; }
        /// <summary>
        /// The external token given to a virtual card.
        /// </summary>
        public string ExternalToken { get; set; }
        /// <summary>
        /// First name given to the virtual card.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Unique Id pretaining to the virtual card.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The invoice number for the given virtual card.
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// Last for digits of the credit card number for the virtual card.
        /// </summary>
        public string LastFour { get; set; }
        /// <summary>
        /// Last name given to the virtual card.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Additional notes for a virtual card.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// The number of transaction allowed for a virtual card.
        /// </summary>
        public string NumberOfTransactions { get; set; }
        /// <summary>
        /// The address given to the virtual card.
        /// </summary>
        public string PoNumber { get; set; }
        /// <summary>
        /// Identifier used by CSI for BIN rotation.
        /// </summary>
        public string SupplierID { get; set; }
        /// <summary>
        /// The type of usages the card is allowed to be used for.
        /// </summary>
        public List<string> UsageTypes { get; set; } = new List<string>();
        /// <summary>
        /// Adapts the response from CSI into a virtual card that users are expecting
        /// </summary>
        /// <returns>Instance of a IVirtualCard</returns>
        public IVirtualCard MapCard() => new CSIAdapter().AdaptResponse(this);
    }
}
