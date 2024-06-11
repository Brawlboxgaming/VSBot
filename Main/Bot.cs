using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VPBot.Classes;
using VPBot.Main;

namespace VPBot
{
    public class Bot
    {
        public Events events = new();
        public static DiscordClient Client { get; private set; }
        public static DiscordClientBuilder Builder { get; private set; }
        public static CommandsExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            string json = string.Empty;

            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Builder = DiscordClientBuilder.CreateDefault(configJson.Token, DiscordIntents.All).SetLogLevel(LogLevel.Debug);

            await events.AssignAllEvents();

            Client = Builder.Build();

            Commands = Client.UseCommands(new CommandsConfiguration
            {
                UseDefaultCommandErrorHandler = false
            });

            Commands.CommandErrored += CommandErrorHandler;

            RegisterCommands.RegisterAllCommands();

            DiscordActivity status = new("Variety Pack", DiscordActivityType.Playing);

            await Client.ConnectAsync(status, DiscordUserStatus.Online);

            await ScheduledTasks.StartTimers();

            await Task.Delay(-1);
        }
        private async Task CommandErrorHandler(CommandsExtension s, CommandErroredEventArgs e)
        {
            Bot.Commands.CommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (SlashCheckBaseAttribute check in slex.FailedChecks)
                    {
                        if (check is SlashRequireUserPermissionsAttribute rqu)
                        {
                            await e.Context.RespondAsync(new DiscordInteractionResponseBuilder() { Content = $"Only members with {rqu.Permissions} can run this command!", IsEphemeral = true });
                        }
                        else if (check is SlashRequireOwnerAttribute rqo)
                        {
                            await e.Context.RespondAsync(new DiscordInteractionResponseBuilder() { Content = $"Only the owner <@105742694730457088> can run this command!", IsEphemeral = true });
                        }
                        else
                        {
                            await e.Context.RespondAsync(new DiscordInteractionResponseBuilder() { Content = "An internal error has occured. Please report this to <@105742694730457088> with details of the error.", IsEphemeral = false });
                        }
                    }
                }
                else
                {
                    Console.WriteLine(e.Exception);
                }
                await Task.CompletedTask;
            };

            await Task.CompletedTask;
        }
    }
}
