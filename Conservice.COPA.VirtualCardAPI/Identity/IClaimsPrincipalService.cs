using System;
using System.Threading.Tasks;

namespace Payment.Portal.CL.Utilities.Interfaces
{
    /// <summary>
    /// Interface for handling ClaimsPrincipal logic
    /// </summary>
    public interface IClaimsPrincipalService
    {
        /// <summary>
        /// Get the value of the azp claim
        /// </summary>
        string GetAuthorizedParty();

        /// <summary>
        /// Gets the bearer token value from the HttpContext headers
        /// </summary>
        Task<string> GetAccessToken();
    }
}