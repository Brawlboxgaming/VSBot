using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.ComponentModel;

namespace VPBot.Commands.General
{
    public class Help
    {
        [Command("help")]
        [Description("Displays a list of commands.")]
        public async Task HelpCommand(CommandContext ctx)
        {
            if (ctx is SlashCommandContext sCtx)
            {
                await sCtx.DeferResponseAsync(Util.CheckEphemeral(ctx));
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
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

            await ctx.EditResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
