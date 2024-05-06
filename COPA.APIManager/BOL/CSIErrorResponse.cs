using System.Net;

namespace COPA.APIManager.BOL;

public class CSIErrorResponse
{
    public const string DefaultUninitializedResponseCode = "9998";

    public HttpStatusCode ObservedStatusCode { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string RawBody { get; set; }
    
    public CSIError Error { get; set; }

    public bool IsThrottleResponse => StatusCode == HttpStatusCode.TooManyRequests;
}
