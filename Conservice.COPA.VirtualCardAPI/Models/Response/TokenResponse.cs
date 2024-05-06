using System;

namespace Conservice.COPA.VirtualCardAPI.Models.Response
{
    /// <summary>
    /// Data returned in response to the Token action
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Bearer token for authentication.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// The date and time the AccessToken expires.
        /// </summary>
        public DateTime AccessExpirationAt { get; set; }
        /// <summary>
        /// Refresh token to refresh access token.
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// The date and time the RefreshToken expires.
        /// </summary>
        public DateTime RefreshExpirationAt { get; set; }
    }
}
