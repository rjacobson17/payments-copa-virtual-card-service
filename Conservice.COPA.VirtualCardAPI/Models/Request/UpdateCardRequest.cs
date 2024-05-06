namespace Conservice.COPA.VirtualCardAPI.Models.Request
{
    /// <summary>
    /// Contains information required to update values for a given virtual card
    /// </summary>
    public class UpdateCardRequest
    {
        /// <summary>
        /// Id of the card that will be updated
        /// </summary>
        public int VirtualCardID { get; set; }

        /// <summary>
        /// Identifies which client/company the card is tied to
        /// </summary>
        public int ClientID { get; set; }

        /// <summary>
        /// Amount the card should be set to now
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// First name the card should be set to
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name the card should be set to
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Notes misc details tied to card
        /// </summary>
        public string Notes { get; set; }
    }
}
