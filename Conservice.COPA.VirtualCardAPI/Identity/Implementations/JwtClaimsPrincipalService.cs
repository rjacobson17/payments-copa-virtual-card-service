using Microsoft.AspNetCore.Http;
using Payment.Portal.API.Extensions;
using Payment.Portal.CL.Utilities.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Payment.Portal.API.Auth
{
    /// <summary>
    /// Implementation of <see cref="IClaimsPrincipalService"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class JwtClaimsPrincipalService : IClaimsPrincipalService
    {
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor">Instance of httpContextAccessor object</param>
        /// <param name="paymentUserManager">Instance of PaymentUserManager</param>
        /// <param name="keycloakService">Instance of KeycloakService -> manages getting authentication tokens</param>
        public JwtClaimsPrincipalService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _claimsPrincipal = httpContextAccessor.HttpContext?.User;
        }

        public string GetAuthorizedParty()
        {
            return _claimsPrincipal.AuthorizedParty();
        }

        /// <inheritdoc/>
        public async Task<string> GetAccessToken()
        {
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader))
            {
                if (AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue header))
                {
                    // header.Scheme is "Bearer"
                    // header.Parameter is the token
                    return header.Parameter;
                }
            }

            return "";
        }
    }
}
