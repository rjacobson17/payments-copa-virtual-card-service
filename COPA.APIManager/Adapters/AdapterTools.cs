namespace COPA.APIManager.Adapters
{
    /// <summary>
    /// Contains helper methods used by Adapters classes.
    /// </summary>
    public static class AdapterTools
    {
        /// <summary>
        /// The name of the Card Network a credit card belongs to.
        /// </summary>
        /// <param name="value">First digit of the credit card number.</param>
        /// <returns>Card Network name.</returns>
        public static string SetCardNetwork(string value)
        {
            return value switch
            {
                "3" => "American Express",
                "4" => "Visa",
                "5" => "Mastercard",
                "6" => "Discover",
                _ => "Invalid Card Network",
            };
        }
    }
}
