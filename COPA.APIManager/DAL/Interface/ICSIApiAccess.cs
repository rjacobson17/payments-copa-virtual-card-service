using System.Threading;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;
using System.Threading.Tasks;
using Conservice.Platform.ProcessResults;

namespace COPA.APIManager.DAL.Interface
{
    /// <summary>
    /// Interface for communication with the CSI API
    /// </summary>
    public interface ICSIApiAccess
    {
        /// <summary>
        /// Gets an existing virtual card.
        /// </summary>
        /// <param name="token">Access toke for the API</param>
        /// <param name="cardId">The ID of the virtual card to get.</param>
        /// <returns>IVirtualCard containing information regarding the vitual card.</returns>
        Task<TypeResult<IVirtualCard, CSIErrorResponse>> GetConserviceCardInfoAsync(string token, string cardId, string confidentialClientId, CancellationToken cancel);

        /// <summary>
        /// Sends an api call to CSI to create a virtual card.
        /// </summary>
        /// <param name="token">Access toke for the API</param>
        /// <param name="request">Object containing the information to create a virtual card.</param>
        /// <returns>IVirtualCard containing information regarding the vitual card.</returns>
        Task<TypeResult<IVirtualCard, CSIErrorResponse>> CreateConserviceCardAsync(string token, CSICardRequest request, string confidentialClientId, CancellationToken cancel);

        /// <summary>
        /// Sends an api call to CSI to update a card's values
        /// </summary>
        /// <param name="token">Access toke for the API</param>
        /// <param name="request">Object containing required information to update a given card</param>
        /// <returns>Virtual card that can be used for payments</returns>
        Task<TypeResult<IVirtualCard, CSIErrorResponse>> UpdateConserviceCardAsync(string token, CSIUpdateRequest request, string confidentialClientId, CancellationToken cancel);

        /// <summary>
        /// Sends an API request to CSI for a card be blocked to not be used anymore
        /// </summary>
        /// <param name="token">Access toke for the API</param>
        /// <param name="clientID">The ID assigned to a Conservice client.</param>
        /// <param name="cardID">The ID of the virtual card to be blocked.</param>
        /// <returns>IVirtualCard containing information regarding the blocked vitual card.</returns>
        Task<TypeResult<IVirtualCard, CSIErrorResponse>> BlockCardAsync(string token, string cardID, string confidentialClientId, CancellationToken cancel);
    }
}
