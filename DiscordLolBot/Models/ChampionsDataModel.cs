namespace PrzyjaznyBot.Models;

public record ChampionsDataModel(Dictionary<string, ChampionModel> Data);

public record ChampionModel(string Name, Stats Info);

public record Stats(int Attack, int Defense, int Magic, int Difficulty);
