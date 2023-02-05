using System.Security.Claims;

namespace Acme.Core.Extensions;

public static class ClaimExtensions
{
    public static string GetKey(this Claim claim)
    {
        string[] tokens = claim.Type.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
        return tokens[^1];
    }
}