namespace Acme.Core.ServiceResponse
{
    public class ErrorResponse
    {
        internal ErrorResponse(string applicationErrorMessage)
        {
            ApplicationErrorMessage = applicationErrorMessage;
        }

        public string ApplicationErrorMessage { get; internal set; }
    }
}