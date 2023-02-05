using System.Security.Claims;
using Acme.Core.Extensions;

namespace Acme.Core.Helpers;

public class ClaimsHelper
{
    public static Dictionary<string, string> ResolveOAuthClaims(IEnumerable<Claim> claims)
    {
        if (claims == null)
            return null;

        Dictionary<string, string> userData = new Dictionary<string, string>();
        foreach (var claim in claims)
        {
            var key = claim.GetKey();
            switch (key.ToUpper())
            {
                case "NAME":
                    if (claim.Type == "name")
                    {
                        userData.Add("FullName", claim.Value);
                        var nametokens = claim.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        userData.Add("LastName", nametokens[0]);
                        userData.Add("FirstName", nametokens[1]);
                    }

                    break;
                case "ROLE":
                    userData.Add("Roles", claim.Value);
                    break;
                case "PREFERRED_USERNAME":
                    userData.Add("Email", claim.Value);
                    string[] tokens = claim.Value.Split(new[] {'@'}, StringSplitOptions.RemoveEmptyEntries);
                    userData.Add("Naa", tokens[0]);
                    break;
                case "VER":
                    userData.Add("Version", claim.Value);
                    break;
                case "SCOPE":
                    userData.Add("Scope", claim.Value);
                    break;
            }
        }

        return userData;
    }
}