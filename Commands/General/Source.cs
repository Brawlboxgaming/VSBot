using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.ComponentModel;

namespace VPBot.Commands.General
{
    public class Source
    {
        [Command("source")]
        [Description("Displays the source of Variety Pack related code.")]
        public async Task SourceCommand(CommandContext ctx)
        {
            if (ctx is SlashCommandContext sCtx)
            {
                await sCtx.DeferResponseAsync(Util.CheckEphemeral(ctx));
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#0070FF"),
                Description = "# Source\n" +
                "Discord Bot: *https://github.com/Brawlboxgaming/VPBot*\n" +
                "Variety Pack Source: *https://github.com/Brawlboxgaming/Variety-Pack*\n" +
                "Pulsar Fork: *https://github.com/Brawlboxgaming/Pulsar*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };

            await ctx.EditResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
