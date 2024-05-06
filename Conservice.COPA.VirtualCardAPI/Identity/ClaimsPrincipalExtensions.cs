using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Payment.Portal.API.Extensions
{
    /// <summary>
    /// Extension methods for handling ClaimsPrincipal logic
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ClaimsPrincipalExtensions
    {

        /// <summary>
        /// Retrieves the EmployeeID from the <paramref name="claimsPrincipal"/>
        /// </summary>
        /// <param name="claimsPrincipal">Instance of <see cref="ClaimsPrincipal"/> to retrieve properties from</param>
        /// <returns></returns>
        public static string AuthorizedParty(this ClaimsPrincipal claimsPrincipal)
        {
            // the authorized party claim has what we want
            return FirstClaimValue(claimsPrincipal, "azp");
        }

        private static string FirstClaimValue(ClaimsPrincipal claimsPrincipal, string claim)
        {
            try
            {
                var value = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == claim).Value;
                return value;
            }
            catch (Exception ex)
            {
                throw new Exception($"{claim} doesn't exist in claims principal: {ex.Message}");
            }
        }
    }
}
