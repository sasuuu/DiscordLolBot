namespace PrzyjaznyBot.Clients;

public interface IVersionLolClient
{
    Task<string> GetLatestVersion();
}
