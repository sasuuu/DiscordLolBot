namespace PrzyjaznyBot.Services;

public interface ITeamsService
{
    Task<(List<string> FirstTeam, List<string> SecondTeam)> GetTeams();
}
