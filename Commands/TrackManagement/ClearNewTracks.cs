using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPBot.Classes;

namespace VPBot.Commands.TrackManagement
{
    public class ClearNewTracks
    {
        [Command("clearnewtracks")]
        [Description("This will clear all the tracks out of the \"New Tracks\" database for a new update.")]
        public async Task ClearNewTracksCommand(CommandContext ctx)
        {
            try
            {
                if (ctx is SlashCommandContext sCtx)
                {
                    await sCtx.DeferResponseAsync(Util.CheckEphemeral(ctx));
                }
                else
                {
                    await ctx.DeferResponseAsync();
                }
                VPContext dbCtx = new();
                List<NewTrack> newTracks = dbCtx.NewTracks.ToList();
                foreach (var track in newTracks)
                {
                    dbCtx.Remove(track);
                }
                await dbCtx.SaveChangesAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Success\n" +
                        "The database table has been cleared.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };

                await ctx.EditResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error\n" + ex,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
