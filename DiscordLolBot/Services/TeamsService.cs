using Accord.MachineLearning;
using PrzyjaznyBot.Clients;
using PrzyjaznyBot.Models;

namespace PrzyjaznyBot.Services;

public class TeamsService(IChampionsLolClient championsClient) : ITeamsService
{
    public async Task<(List<string> FirstTeam, List<string> SecondTeam)> GetTeams()
    {
        var random = new Random();
        var championsResponse = await championsClient.GetChampions();
        var champions = championsResponse?.Data.Values.ToList();
        var data = new List<double[]>();

        foreach (var champion in champions!)
        {
            data.Add([champion.Info.Attack, champion.Info.Defense, champion.Info.Magic]);
        }

        double[][] observations = data.ToArray();

        var kMeans = new KMeans(k: random.Next(5, 10));
        var clusters = kMeans.Learn(observations);

        int[] labels = clusters.Decide(observations);

        var groupedChampions = new Dictionary<int, List<ChampionModel>>();

        for (int i = 0; i < labels.Length; i++)
        {
            int clusterId = labels[i];

            if (!groupedChampions.ContainsKey(clusterId))
            {
                groupedChampions[clusterId] = new List<ChampionModel>();
            }

            groupedChampions[clusterId].Add(champions[i]);
        }

        var firstTeam = new List<string>();
        var secondTeam = new List<string>();
        HashSet<string> selectedChampions = new HashSet<string>();

        do
        {
            foreach (var (_, value) in groupedChampions.Where(x => x.Value.Count > 2))
            {
                var availableChampions = value.Where(c => !selectedChampions.Contains(c.Name)).ToList();

                if (availableChampions.Count >= 2)
                {
                    var first = availableChampions[random.Next(availableChampions.Count)];
                    availableChampions.Remove(first);

                    selectedChampions.Add(first.Name);
                    firstTeam.Add(first.Name);

                    var second = availableChampions[random.Next(availableChampions.Count)];

                    selectedChampions.Add(second.Name);
                    secondTeam.Add(second.Name);
                }

                if (firstTeam.Count >= 15)
                {
                    break;
                }
            }
        } while (firstTeam.Count < 15);

        return (firstTeam, secondTeam);
    }
}
