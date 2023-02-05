using System.Net;

namespace Acme.Core.ServiceResponse;

//TODO
//TODO Crank out some AppInsights logging in here.
//TODO

public class HttpServiceResponse : ServiceResponse
{
    internal HttpServiceResponse()
    {
    }

    public HttpErrorResponse ErrorData { get; internal set; }

    public static HttpServiceResponse CreateHttpErrorResponse(string applicationErrorMessage, string reasonPhrase,
        HttpStatusCode statusCode)
    {
        return new HttpServiceResponse
        {
            Failed = true,
            ErrorData = new HttpErrorResponse(applicationErrorMessage, reasonPhrase, statusCode)
        };
    }

    public static HttpServiceResponse CreateHttpErrorResponse(HttpErrorResponse errorResponse)
    {
        return new HttpServiceResponse
        {
            Failed = true,
            ErrorData = new HttpErrorResponse(
                errorResponse.ApplicationErrorMessage,
                errorResponse.HttpReasonPhrase,
                errorResponse.ErrorCode)
        };
    }
}

public class HttpServiceResponse<T> : HttpServiceResponse
{
    internal HttpServiceResponse()
    {
    }

    public T ResultData { get; internal set; }

    public static HttpServiceResponse<T> CreateHttpErrorResponse<T>(string applicationErrorMessage,
        string reasonPhrase, HttpStatusCode statusCode)
    {
        return new HttpServiceResponse<T>
        {
            Failed = true,
            ErrorData = new HttpErrorResponse(applicationErrorMessage, reasonPhrase, statusCode)
        };
    }

    public static HttpServiceResponse<T> CreateHttpServiceResponse<T>(
        HttpErrorResponse errorResponse)
    {
        return new HttpServiceResponse<T>
        {
            Failed = true,
            ErrorData = new HttpErrorResponse(
                errorResponse.ApplicationErrorMessage,
                errorResponse.HttpReasonPhrase,
                errorResponse.ErrorCode)
        };
    }
}