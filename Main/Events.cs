using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands;

namespace VPBot
{
    public class Events
    {
        public async Task AssignAllEvents()
        {
            Bot.SlashCommands.SlashCommandErrored += SlashCommandErrorHandler;
            await Task.CompletedTask;
        }
        private async Task SlashCommandErrorHandler(SlashCommandsExtension s, SlashCommandErrorEventArgs e)
        {
            Bot.SlashCommands.SlashCommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (SlashCheckBaseAttribute check in slex.FailedChecks)
                    {
                        if (check is SlashRequireUserPermissionsAttribute rqu)
                        {
                            await e.Context.CreateResponseAsync($"Only members with {rqu.Permissions} can run this command!", true);
                        }
                        else if (check is SlashRequireOwnerAttribute rqo)
                        {
                            await e.Context.CreateResponseAsync($"Only the owner <@105742694730457088> can run this command!", true);
                        }
                        else
                        {
                            await e.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("An internal error has occured. Please report this to <@105742694730457088> with details of the error."));
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
