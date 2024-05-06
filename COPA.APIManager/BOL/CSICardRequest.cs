namespace COPA.APIManager.BOL
{
    /// <summary>
    /// Contains the information required to create a virtual card.
    /// </summary>
    public class CSICardRequest
    {
        /// <summary>
        /// Unique identifier associated to COPA clients.
        /// </summary>
        public int ClientID { get; set; }
        /// <summary>
        /// Name associated to the unique identifier associated to COPA clients.
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// The amount of the card to be created.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Determines if the amount charged needs to match the amount the card was created for.
        /// </summary>
        public bool ExactAmount { get; set; }

        /// <summary>
        /// First name of the person who will use the card
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of the person who will use the card
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// 1-12 month for when the card is set to expire.
        /// </summary>
        public int ExpirationMonth { get; set; }
        /// <summary>
        /// 2-digit year for when the card is set to expire.
        /// </summary>
        public int ExpirationYear { get; set; }
        /// <summary>
        /// The number of times the card can be used.
        /// </summary>
        public int NumberOfTransactions { get; set; }
        /// <summary>
        /// The invoice number for the card.
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// Notes to be associated with the virtual card.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Conservice Payment Identification Number, saved as Purchase Order Number on the CSI side.
        /// </summary>
        public string PoNumber { get; set; }
        /// <summary>
        /// Identifier used to send to CSI for BIN rotation.
        /// </summary>
        public string SupplierID { get; set; }

        /// <summary>
        /// Setting used to send to CSI for BIN rotation.
        /// </summary>
        public bool BinRotation { get; set; }
    }
}
