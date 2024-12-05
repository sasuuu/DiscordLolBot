using System.Net.Http.Json;

namespace PrzyjaznyBot.Clients;

public class VersionLolClient(IHttpClientFactory httpClientFactory)
    : BaseLolClient(httpClientFactory), IVersionLolClient
{
    private const string VersionUrl = "api/versions.json";

    public async Task<string> GetLatestVersion()
    {
        var versions = await GetHttpClient().GetFromJsonAsync<List<string>>(VersionUrl);

        return versions!.First();
    }
}
