using System.ComponentModel;
using COPA.APIManager.BOL.Enums;

namespace Conservice.COPA.VirtualCardAPI.Models.Request
{
    /// <summary>
    /// Contains the information required to create a virtual card.
    /// </summary>
    public class CreateCardRequest
    {
        /// <summary>
        /// Unique identifier associated to COPA clients.
        /// </summary>
        public int ClientID { get; set; }
        /// <summary>
        /// The amount of the card to be created.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// First name of the person who will use the card
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of the person who will use the card
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Additional notes assigned to a virtual card request.
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Payment Identification Number
        /// </summary>
        public string Pin { get; set; }
        /// <summary>
        /// Identifier used to send to CSI for BIN rotation.
        /// </summary>
        [DefaultValue("")]
        public string SupplierID { get; set; } = "";

        /// <summary>
        /// Setting used to send to CSI for BIN rotation.
        /// </summary>
        [DefaultValue(true)]
        public bool BinRotation { get; set; } = true;
    }
}
