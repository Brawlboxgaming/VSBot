using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPBot.Classes;

namespace VPBot.Commands.TrackManagement
{
    public class RejectTrack
    {
        [Command("rejecttrack")]
        [Description("To reject a track manually from the list of pending submissions.")]
        public async Task RejectTrackCommand(CommandContext ctx,
            [Option("ID", "The ID of the submission to be rejected.")] string id,
            [Option("Reason", "The reason for the rejection")] string reason)
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
                if (dbCtx.Submissions.Any(x => x.ID.ToString() == id))
                {
                    TrackSubmission submission = dbCtx.Submissions.First(x => x.ID.ToString() == id);
                    if (submission.Accepted == false && submission.Rejected == false)
                    {
                        submission.Pending = false;
                        submission.Rejected = true;
                        submission.RejectionReason = reason;
                        await dbCtx.SaveChangesAsync();

                        DiscordMember submitter = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetMemberAsync(ulong.Parse(submission.SubmitterID));
                        try
                        {
                            await submitter.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                                new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#0070FF"),
                                    Description = $"# Track Submission #{submission.ID}" +
                                    $"\nThe track {submission.Name} was rejected for the following reason:" +
                                    $"\n*{submission.RejectionReason}*",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Server Time: {DateTime.Now}"
                                    }
                                })
                            );
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = "# Success\nThe track has been rejected and the submitter has been notified.",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            }));
                        }
                        catch
                        {
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = "# Success\nThe track has been rejected, but the submitter could not be notified.",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            }));
                            Console.WriteLine($"{submitter.Nickname} does not have DMs accessible.");
                        }
                        await foreach (var m in Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.SUBMISSION_APPROVAL).Result.GetMessagesAsync())
                        {
                            if (m.Embeds[0].Description.Contains($"# Track Submission #{submission.ID}"))
                            {
                                await m.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                                {
                                    Color = new DiscordColor("#0070FF"),
                                    Description = "# Rejected\n" + m.Embeds[0].Description.Replace("# Accepted\n", ""),
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Server Time: {DateTime.Now}"
                                    }
                                }));
                            }
                        }
                        if (submission.PollID != null)
                        {
                            DiscordMessage message = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.TRACK_VOTING).Result.GetMessageAsync(ulong.Parse(submission.PollID));

                            await message.EndPollAsync();
                            await message.Channel.SendMessageAsync($"\nThe track {submission.Name} was rejected for the following reason:\n*{submission.RejectionReason}*");
                        }
                    }
                    else
                    {
                        if (submission.Accepted)
                        {
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = "# Error\nThe submission was already accepted.",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            }));
                        }
                        else
                        {
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = "# Error\nThe submission was already rejected.",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            }));
                        }
                    }
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = "# Error\nThe ID was not found in the database.",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    }));
                }
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
