namespace Acme.Core.ServiceResponse
{
    //TODO
    //TODO Crank out some AppInsights logging in here.
    //TODO

    public class ServiceResponse
    {
        public bool Failed { get; set; }

        public bool IsNull { get; set; }

        public ErrorResponse Error { get; internal set; }

        public static ServiceResponse CreateErrorResponse(string applicationErrorMessage)
        {
            return new ServiceResponse
            {
                Failed = true,
                Error = new ErrorResponse(applicationErrorMessage)
            };
        }

        public static ServiceResponse CreateErrorResponse(ErrorResponse errorResponse)
        {
            return new ServiceResponse
            {
                Failed = true,
                Error = new ErrorResponse(errorResponse.ApplicationErrorMessage)
            };
        }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        internal ServiceResponse()
        {
        }

        public T? ResultData { get; internal set; }

        public new static ServiceResponse<T> CreateErrorResponse(string applicationErrorMessage)
        {
            return new ServiceResponse<T>
            {
                Failed = true,
                Error = ServiceResponse.CreateErrorResponse(applicationErrorMessage).Error
            };
        }

        public new static ServiceResponse CreateErrorResponse(ErrorResponse errorResponse)
        {
            return new ServiceResponse
            {
                Failed = true,
                Error = ServiceResponse.CreateErrorResponse(errorResponse.ApplicationErrorMessage).Error
            };
        }
    }
}