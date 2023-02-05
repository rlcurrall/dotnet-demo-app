namespace Acme.Core.Models.AppSettings
{
    public class OAuth2Credentials
    {
        public string AuthorizationEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}