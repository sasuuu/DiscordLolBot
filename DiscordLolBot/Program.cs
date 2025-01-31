using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrzyjaznyBot.Clients;
using PrzyjaznyBot.Commands;
using PrzyjaznyBot.Services;

var configurationBuilder = new ConfigurationBuilder()
    .AddEnvironmentVariables();

var configuration = configurationBuilder.Build();

var discordToken = configuration.GetValue("DiscordToken", default(string));
if (discordToken == null)
{
    throw new ArgumentNullException(nameof(discordToken));
}

var intents = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.GuildVoiceStates | DiscordIntents.MessageContents;
var clientBuilder = DiscordClientBuilder.CreateDefault(discordToken, intents);

clientBuilder.ConfigureServices(services =>
{
    services.AddTransient<IVersionLolClient, VersionLolClient>();
    services.AddTransient<IChampionsLolClient, ChampionsLolClient>();
    services.AddTransient<ITeamsService, TeamsService>();
    services.AddHttpClient();
});

clientBuilder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
{
    extension.AddCommands([typeof(DrawTeamsCommands), typeof(DrawAramChampionsCommands)]);
    var textCommandProcessor = new TextCommandProcessor(new()
    {
        PrefixResolver = new DefaultPrefixResolver(true, "/").ResolvePrefixAsync
    });

    extension.AddProcessor(textCommandProcessor);
});

clientBuilder.UseInteractivity(new()
{
    PollBehaviour = PollBehaviour.KeepEmojis,
    Timeout = TimeSpan.FromSeconds(30)
});

var client = clientBuilder.Build();

var status = new DiscordActivity("/draw_teams", DiscordActivityType.Playing);

await client.ConnectAsync(status, DiscordUserStatus.Online);
await Task.Delay(-1);
