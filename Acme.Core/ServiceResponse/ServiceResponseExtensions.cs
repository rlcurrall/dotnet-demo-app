namespace Acme.Core.ServiceResponse;

public static class ServiceResponseExtensions
{
    public static ServiceResponse CreateServiceResponse()
    {
        return new ServiceResponse
        {
            Failed = false
        };
    }

    public static ServiceResponse CreateNullServiceResponse()
    {
        return new ServiceResponse
        {
            Failed = false,
            IsNull = true
        };
    }

    public static async Task<ServiceResponse> CreateServiceResponseAsync(this Task<ServiceResponse> task,
        string applicationErrorMessage)
    {
        ServiceResponse response = new();

        try
        {
            var result = await task;
            response.Failed = result.Failed;
            response.Error = result.Error;
        }
        catch (Exception exception)
        {
            response.Error = new ErrorResponse($"{exception} - {applicationErrorMessage}");
            response.Failed = true;
        }

        return response;
    }

    public static async Task<ServiceResponse> CreateServiceResponseAsync(this Task task,
        string applicationErrorMessage)
    {
        ServiceResponse response = new();

        try
        {
            await task;
        }
        catch (Exception exception)
        {
            response.Error = new ErrorResponse($"{exception} - {applicationErrorMessage}");
            response.Failed = true;
        }

        return response;
    }

    public static ServiceResponse<T> CreateServiceResponse<T>(this T resultData)
    {
        if (resultData == null) return resultData.CreateNullServiceResponse();

        return new ServiceResponse<T>
        {
            Failed = false,
            ResultData = resultData
        };
    }

    public static ServiceResponse<T> CreateNullServiceResponse<T>(this T resultData)
    {
        return new ServiceResponse<T>
        {
            Failed = false,
            IsNull = true
        };
    }

    public static ServiceResponse<T> CreateErrorResponse<T>(string applicationErrorMessage)
    {
        return ServiceResponse<T>.CreateErrorResponse(applicationErrorMessage);
    }

    public static ServiceResponse<T> CreateErrorResponse<T>(this ErrorResponse errorResponse)
    {
        return ServiceResponse<T>.CreateErrorResponse(errorResponse.ApplicationErrorMessage);
    }

    public static ServiceResponse<T> CreateErrorResponse<T>(this ServiceResponse serviceResponse)
    {
        return ServiceResponse<T>.CreateErrorResponse(serviceResponse.Error.ApplicationErrorMessage);
    }

    public static async Task<ServiceResponse<T>> CreateServiceResponseAsync<T>(this Task<ServiceResponse<T>> task,
        string applicationErrorMessage)
    {
        ServiceResponse<T> response = new();

        try
        {
            var result = await task;
            response.ResultData = result.ResultData;
            response.Failed = result.Failed;
            response.IsNull = result.IsNull;
            response.Error = result.Error;
        }
        catch (Exception exception)
        {
            response.Error = new ErrorResponse($"{exception} - {applicationErrorMessage}");
            response.Failed = true;
        }

        return response;
    }

    public static async Task<ServiceResponse<T>> CreateServiceResponseAsync<T>(this Task<T> task,
        string applicationErrorMessage)
    {
        ServiceResponse<T> response = new();

        try
        {
            var result = await task;
            response.ResultData = result;
            response.Failed = false;
        }
        catch (Exception exception)
        {
            response.Error = new ErrorResponse($"{exception} - {applicationErrorMessage}");
            response.Failed = true;
        }

        return response;
    }
}