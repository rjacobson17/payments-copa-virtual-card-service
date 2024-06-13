using COPA.APIManager.BLL.Interface;
using COPA.APIManager.BOL;
using COPA.APIManager.BOL.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection.Metadata.Ecma335;

namespace COPA.APIManager.BLL
{
    /// <summary>
    /// Class used to handle both Token and Username/Password credential management
    /// </summary>
    public class CredentialsManager : ICredentialsManager
    {
        private readonly IConfiguration _config;
        private readonly string _token;

        /// <summary>
        /// Constructor. Initializes fields through DI
        /// </summary>
        /// <param name="configuration">Intsance of the applications configuration properties.</param>
        /// <param name="tokenFactory">Instance used to retrieve tokens</param>
        /// <param name="credentialFactory">Instance used to retrieve username/pasword pairs</param>
        public CredentialsManager(IConfiguration configuration)
        {
            _config = configuration;
            _token = Environment.GetEnvironmentVariable("Virtual_Card_Token");
        }

        /// <inheritdoc/>
        public string GetToken()
        {
            return _token;
        }
    }
}
