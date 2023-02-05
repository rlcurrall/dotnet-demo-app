using System.Net;

namespace Acme.Core.ServiceResponse;

public class HttpErrorResponse : ErrorResponse
{
    internal HttpErrorResponse(string httpReasonPhrase, HttpStatusCode errorCode)
        : this(null, httpReasonPhrase, errorCode)
    {
    }

    internal HttpErrorResponse(string applicationErrorMessage, string httpReasonPhrase, HttpStatusCode errorCode)
        : base(applicationErrorMessage)
    {
        ErrorCode = errorCode;
        HttpReasonPhrase = httpReasonPhrase;
    }

    public HttpStatusCode ErrorCode { get; internal set; }

    public string HttpReasonPhrase { get; internal set; }
}