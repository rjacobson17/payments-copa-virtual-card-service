namespace COPA.APIManager.BOL.Interfaces
{
    /// <summary>
    /// Enforces the requirements that make up a virtual card.
    /// </summary>
    public interface IVirtualCard
    {
        /// <summary>
        /// Name of the client the card belongs to.
        /// </summary>
        string ClientName { get; set; }
        /// <summary>
        /// Unique identifier associated to the card.
        /// </summary>
        string VirtualCardID { get; set; }
        /// <summary>
        /// Determines if the virtual card is active or blocked.
        /// </summary>
        string IsBlocked { get; set; }
        /// <summary>
        /// The virtual credit card number.
        /// </summary>
        string CreditCardNumber { get; set; }
        /// <summary>
        /// The virtual card security code.
        /// </summary>
        string SecurityCode { get; set; }
        /// <summary>
        /// The virtual card expiration month.
        /// </summary>
        string ExpirationMonth { get; set; }
        /// <summary>
        /// The virtual card expiration year.
        /// </summary>
        string ExpirationYear { get; set; }
        /// <summary>
        /// The name of the network the card is associated to.
        /// </summary>
        string CardNetwork { get; set; }
        /// <summary>
        /// External Token, id to track card history
        /// </summary>
        string VirtualCardExternalToken { get; set; }
    }
}
