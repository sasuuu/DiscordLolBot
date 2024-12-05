namespace PrzyjaznyBot.Clients;

public abstract class BaseLolClient(IHttpClientFactory httpClientFactory)
{
    private const string BaseUrl = "https://ddragon.leagueoflegends.com/";

    protected HttpClient GetHttpClient()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(BaseUrl);

        return client;
    }
}
