using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using PrzyjaznyBot.Services;
using System.Text;

namespace PrzyjaznyBot.Commands;

public class DrawAramChampionsCommands(ITeamsService teamsService)
{
    [Command("draw_aram_champions")]
    public async ValueTask DrawAramChampionsAsync(CommandContext context)
    {
        var (FirstTeam, SecondTeam) = await teamsService.GetTeams();
        var response = GenerateTeamResponse(FirstTeam, SecondTeam);

        await context.RespondAsync(response);
    }

    private static string GenerateTeamResponse(List<string> firstTeam, List<string> secondTeam)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("```");
        stringBuilder.Append($"Team 1:{Environment.NewLine}");
        stringBuilder.Append(string.Join(Environment.NewLine, firstTeam.Select(x => $"- {x}")));
        stringBuilder.Append("```");
        stringBuilder.Append("```");
        stringBuilder.Append($"Team 2:{Environment.NewLine}");
        stringBuilder.Append(string.Join(Environment.NewLine, secondTeam.Select(x => $"- {x}")));
        stringBuilder.Append("```");
        var s = stringBuilder.ToString();
        
        return s;
    }
}
