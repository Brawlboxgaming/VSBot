using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace VPBot.Commands.General
{
    public class Source : ApplicationCommandModule
    {
        [SlashCommand("source", "Displays the source of Variety Pack related code.")]
        public async Task SourceCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = Util.CheckEphemeral(ctx) });
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#0070FF"),
                Description = "# Source\n" +
                "Discord Bot: *https://github.com/Brawlboxgaming/Variety-Pack-Bot*\n" +
                "Variety Pack Source: *https://github.com/Brawlboxgaming/Variety-Pack*\n" +
                "Pulsar Fork: *https://github.com/Brawlboxgaming/Pulsar*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
