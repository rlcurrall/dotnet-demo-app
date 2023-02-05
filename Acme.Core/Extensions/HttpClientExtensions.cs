namespace Acme.Core.Extensions;

public static class HttpClientExtensions
{
    public static void BuildBaseAddress(this HttpClient client, string baseUrl, string version)
    {
        client.BaseAddress = new Uri(baseUrl + "/" + version + "/");
    }
}