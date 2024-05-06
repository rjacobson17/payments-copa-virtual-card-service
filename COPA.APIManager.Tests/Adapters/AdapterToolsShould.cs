using COPA.APIManager.Adapters;
using NUnit.Framework;

namespace COPA.APIManager.Tests.Adapters
{
    public class AdapterToolsShould
    {
        [Test]
        [TestCase("3", "American Express")]
        [TestCase("4", "Visa")]
        [TestCase("5", "Mastercard")]
        [TestCase("6", "Discover")]
        [TestCase(" ", "Invalid Card Network")]
        public void CardNetworkMap(string firstDigit, string expectedCardNetwork)
        {
            var cardNetwork = AdapterTools.SetCardNetwork(firstDigit);

            StringAssert.AreEqualIgnoringCase(expectedCardNetwork, cardNetwork);
        }
    }
}