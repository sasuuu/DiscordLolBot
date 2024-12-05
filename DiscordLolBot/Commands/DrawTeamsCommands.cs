using Accord.Math;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace PrzyjaznyBot.Commands;

public class DrawTeamsCommands
{
    private static string[] SavedUsers = [];
    private const string UsersDropdownId = "users_dropdown";
    private const string AddUsersButtonId = "add_users";
    private const string DrawButtonId = "draw_button";

    [Command("draw_teams")]
    public static async ValueTask DrawTeamsAsync(CommandContext context)
    {
        var users = new List<string>();
        await context.DeferResponseAsync();

        var builder = GenerateInteractiveResponseBuilder(context, GenerateInteractiveResponseContent([]));

        var message = await context.EditResponseAsync(builder);

        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        var interactivities = new[]
        {
            message.WaitForButtonAsync(cts.Token)
        }.ToList();

        if (builder.Components.Any(actionRow => actionRow.Components.Any(c => c.CustomId == UsersDropdownId)))
        {
            interactivities.Add(message.WaitForSelectAsync(UsersDropdownId, cts.Token));
        }

        var interactivity = await Task.WhenAny(interactivities);
        while (!interactivity.Result.TimedOut && interactivity.Result.Result.Id != DrawButtonId)
        {
            var componentId = interactivity.Result.Result.Id;
            if (componentId == AddUsersButtonId)
            {
                await HandleAddUsersButton(interactivity, context, users);

                interactivities.Remove(interactivity);
                interactivities.Add(message.WaitForButtonAsync(cts.Token));
            }

            if (componentId == UsersDropdownId)
            {
                await HandleUsersDropdown(interactivity, context, users);

                interactivities.Remove(interactivity);
                interactivities.Add(message.WaitForSelectAsync(UsersDropdownId, cts.Token));
            }

            interactivity = await Task.WhenAny(interactivities);
        }

        if (interactivity.Result.TimedOut)
        {
            await context.EditResponseAsync("Operation timed out");
            return;
        }

        var usersArray = users.ToArray();
        SavedUsers = [.. usersArray];

        await interactivity.Result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
        await context.EditResponseAsync(GenerateFinalResponse(usersArray));

        cts.Cancel();
        await Task.WhenAll(interactivities);
    }

    [Command("draw_teams_again")]
    public static async ValueTask DrawSavedTeams(CommandContext context)
    {
        if (SavedUsers.Length == 0)
        {
            await context.RespondAsync("No saved users");
            return;
        }

        await context.RespondAsync(GenerateFinalResponse(SavedUsers));
    }

    private static async ValueTask HandleAddUsersButton(Task<InteractivityResult<DSharpPlus.EventArgs.ComponentInteractionCreatedEventArgs>> interactivity,
        CommandContext context,
        List<string> users) 
    {
        var modalBuilder = new DiscordInteractionResponseBuilder();
        modalBuilder.WithTitle("Additional users");
        modalBuilder.WithCustomId("users_modal");
        modalBuilder.AsEphemeral();
        var inputText = new DiscordTextInputComponent("users", "custom_users", "Provide users for draw(write each user in separate line)", null, false, DiscordTextInputStyle.Paragraph, 0, 600);
        modalBuilder.AddComponents(inputText);

        await interactivity.Result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modalBuilder);

        InteractivityExtension interactivityExtension = context.ServiceProvider.GetRequiredService<InteractivityExtension>();
        var modal = await interactivityExtension.WaitForModalAsync("users_modal");
        var additional_users = modal
            .Result
            .Values
            .FirstOrDefault(kvp => kvp.Key == "custom_users")
            .Value
            .Split("\n")
            .Where(u => !users.Contains(u))
            .ToArray();

        users.AddRange(additional_users);

        await context.EditResponseAsync(GenerateInteractiveResponseBuilder(context, GenerateInteractiveResponseContent([.. users])));
        await modal.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
    }

    private static async ValueTask HandleUsersDropdown(Task<InteractivityResult<DSharpPlus.EventArgs.ComponentInteractionCreatedEventArgs>> interactivity,
        CommandContext context,
        List<string> users)
    {
        var selectedUsers = interactivity.Result.Result.Values;

        users.AddRange(selectedUsers.Where(u => !users.Contains(u)));

        await context.EditResponseAsync(GenerateInteractiveResponseBuilder(context, GenerateInteractiveResponseContent([.. users])));
        await interactivity.Result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
    }

    private static string GenerateInteractiveResponseContent(string[] users)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("Draw teams");
        stringBuilder.AppendLine();
        if (users.Length > 0)
        {
            stringBuilder.AppendLine("Current user list:");
        }
        foreach (var user in users) 
        {
            stringBuilder.AppendLine(user);
        }

        var s = stringBuilder.ToString();

        return s;
    }

    private static DiscordInteractionResponseBuilder GenerateInteractiveResponseBuilder(CommandContext context, string content)
    {
        var builder = new DiscordInteractionResponseBuilder();
        builder.AsEphemeral();
        builder.WithContent(content);
        var voiceChannel = context.Member?.VoiceState?.Channel;
        if (voiceChannel is not null)
        {
            var selectOptions = voiceChannel.Users.Select(u => new DiscordSelectComponentOption(u.DisplayName, u.DisplayName));
            var dropdown = new DiscordSelectComponent(UsersDropdownId, "Select users from voice channel for draw", selectOptions, false, 1, selectOptions.Count());

            builder.AddComponents(dropdown);
        }
        var addUsersButton = new DiscordButtonComponent(DiscordButtonStyle.Success, "add_users", "Add users");
        builder.AddComponents(addUsersButton);
        var drawButton = new DiscordButtonComponent(DiscordButtonStyle.Primary, "draw_button", "Draw");
        builder.AddComponents(drawButton);

        return builder;
    }

    private static string GenerateFinalResponse(string[] users) 
    {
        Random.Shared.Shuffle(users);
        var middleIndex = users.Length / 2;

        var stringBuilder = new StringBuilder();

        stringBuilder.Append("```");
        stringBuilder.AppendLine("Team 1:");
        foreach (var user in users.Take(middleIndex)) 
        {
            stringBuilder.AppendLine(user);
        }
        stringBuilder.Append("```");
        stringBuilder.Append("```");
        stringBuilder.AppendLine("Team 2:");
        foreach (var user in users.Skip(middleIndex))
        {
            stringBuilder.AppendLine(user);
        }
        stringBuilder.Append("```");

        var s = stringBuilder.ToString();

        return s;
    }
}
