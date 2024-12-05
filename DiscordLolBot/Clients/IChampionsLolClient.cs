using PrzyjaznyBot.Models;

namespace PrzyjaznyBot.Clients;

public interface IChampionsLolClient
{
    Task<ChampionsDataModel?> GetChampions();
}
