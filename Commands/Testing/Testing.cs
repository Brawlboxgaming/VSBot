using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.ComponentModel;

namespace VPBot.Commands.Testing
{
    public class Testing
    {
        //[Command("test")]
        [Description("test123")]
        public async Task TestCommand(CommandContext ctx)
        {
            if (ctx is SlashCommandContext sCtx)
            {
                await sCtx.DeferResponseAsync(Util.CheckEphemeral(ctx));
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
        }
    }
}
