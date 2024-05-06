namespace COPA.APIManager.BOL
{
    /// <summary>
    /// Request object sent to CSI to update a card
    /// </summary>
    public class CSIUpdateRequest
    {
        /// <summary>
        /// Unique identifier associated to COPA clients.
        /// </summary>
        public int ClientID { get; set; }
        /// <summary>
        /// ID for the card that will be updated
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The amount of the card to be updated.
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
        /// Notes to be associated with the virtual card.
        /// </summary>
        public string Notes { get; set; }
    }
}
