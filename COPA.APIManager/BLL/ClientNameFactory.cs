using System;
using COPA.APIManager.BOL.Enums;

namespace COPA.APIManager.BLL
{
    public static class ClientNameFactory
    {
        /// <summary>
        /// Sets the name of the client we received a virtual card for.
        /// </summary>
        /// <param name="clientID">Unique identifier associated to a client.</param>
        /// <returns>Client Name.</returns>
        public static string AsClientName(int clientID)
        {
            return (Client)clientID switch
            {
                Client.Synergy => Client.Synergy.ToString(),
                Client.SingleFamily => Client.SingleFamily.ToString(),
                Client.ConAm => Client.ConAm.ToString(),
                Client.CSI => Client.CSI.ToString(),
                Client.Capturis => Client.Capturis.ToString(),
                _ => "Invalid client name",
            };
        }

        public static string AsClientName(string bizDevBrand)
        {
            int clientID = Convert.ToInt32(bizDevBrand);
            return AsClientName(clientID);
        }
    }
}
