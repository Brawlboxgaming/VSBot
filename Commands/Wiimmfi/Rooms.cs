using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace VPBot.Commands.Wiimmfi
{
    public class Rooms : ApplicationCommandModule
    {
        [SlashCommand("rooms", "Checks for rooms on the Variety Pack region.")]
        public async Task RoomCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = Util.CheckEphemeral(ctx) });

        }
    }
}
