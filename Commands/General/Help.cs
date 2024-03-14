using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace VPBot.Commands.General
{
    public class Help : ApplicationCommandModule
    {
        [SlashCommand("help", "Displays a list of commands.")]
        public async Task HelpCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = Util.CheckEphemeral(ctx) });
            string description = "# Help\n" +
                "### Standard Commands\n" +
                "/help";

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Id == 451024579805052968)
                {
                    {
                        description += "\n\n### Admin Commands" +
                            "\n/update" +
                            "\n/downloadoutdated";
                    }
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#0070FF"),
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
