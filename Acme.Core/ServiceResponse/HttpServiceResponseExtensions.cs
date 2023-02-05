namespace Acme.Core.ServiceResponse;

public static class HttpServiceResponseExtensions
{
    public static HttpServiceResponse<T> CreateHttpServiceResponse<T>(this T resultData)
    {
        return new HttpServiceResponse<T>
        {
            Failed = false,
            ResultData = resultData
        };
    }
}