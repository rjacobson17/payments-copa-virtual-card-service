namespace COPA.APIManager.BOL;

public class CSIError
{
    /// <summary>
    /// Message explaining what went wrong when retrieving a virtual card.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Seemingly, whenever there is a valid instance of CSIError, this will be false
    /// </summary>
    public bool? Success { get; set; } = null;
}
