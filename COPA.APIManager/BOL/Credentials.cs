
namespace COPA.APIManager.BOL
{
    /// <inheritdoc/>
    public record Credentials : Interfaces.ICredentials
    {
        /// <inheritdoc/>
        public string Username { get; set; } = "";

        /// <inheritdoc/>
        public string Password { get; set; } = "";

        /// <inheritdoc/>
        public string Token { get; set; } = "";
    }
}