using PrzyjaznyBot.Models;
using System.Net.Http.Json;

namespace PrzyjaznyBot.Clients;

public class ChampionsLolClient(IVersionLolClient versionLolClient, IHttpClientFactory httpClientFactory)
    : BaseLolClient(httpClientFactory), IChampionsLolClient
{
    private readonly string _championTemplateUrl = "cdn/{0}/data/en_US/champion.json";

    public async Task<ChampionsDataModel?> GetChampions()
    {
        var version = await versionLolClient.GetLatestVersion();
        var championUrl = string.Format(_championTemplateUrl, version);

        return await GetHttpClient().GetFromJsonAsync<ChampionsDataModel>(championUrl);
    }
}
