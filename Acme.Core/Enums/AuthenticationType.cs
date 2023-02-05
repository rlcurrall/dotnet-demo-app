namespace Acme.Core.Enums;

[Flags]
public enum AuthenticationType
{
    None = 1,

    OAuthPKCE = 2,

    OAuthImplicit = 4,

    OAuthClientCredentials = 8,

    OAuthPassword = 16
}