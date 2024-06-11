using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPBot.Classes;

namespace VPBot.Commands.Scheduling
{
    public class CheckTrackSubmissions
    {
        [Command("checkpolls")]
        [Description("Checks if the track polls have completed.")]
        [RequireApplicationOwner]
        public static async Task CheckTrackPollsWrapper(CommandContext ctx)
        {
            if (ctx is SlashCommandContext sCtx)
            {
                await sCtx.DeferResponseAsync(true);
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
            await CheckTrackPolls(ctx);
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#0070FF"),
                Description = "# Notice\n" +
                    "Track Polls have been checked.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };
            await ctx.EditResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        public static async Task CheckTrackPolls(CommandContext ctx)
        {
            try
            {
                VPContext dbCtx = new();
                List<TrackSubmission> trackSubmissions = dbCtx.Submissions.ToList();
                bool isPollRunning = false;
                foreach (var sub in trackSubmissions)
                {
                    if (sub.Pending == false && sub.Accepted == false && sub.Rejected == false && sub.PollID != null)
                    {
                        DiscordMember submitter = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetMemberAsync(ulong.Parse(sub.SubmitterID));
                        DiscordMessage message = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.TRACK_VOTING).Result.GetMessageAsync(ulong.Parse(sub.PollID));
                        if (message.Poll.Expiry.Value.UtcDateTime <= DateTime.UtcNow)
                        {
                            int acceptedId = message.Poll.Answers.First(x => x.AnswerData.Text == "Accept").AnswerId;
                            int acceptedCount = 0;
                            await foreach (var answer in message.GetAllPollAnswerVotersAsync(acceptedId))
                            {
                                ++acceptedCount;
                            }
                            int rejectedId = message.Poll.Answers.First(x => x.AnswerData.Text == "Reject").AnswerId;
                            int rejectedCount = 0;
                            await foreach (var answer in message.GetAllPollAnswerVotersAsync(rejectedId))
                            {
                                ++rejectedCount;
                            }

                            sub.FinalScore = acceptedCount * 100 / (acceptedCount + rejectedCount);

                            if (sub.FinalScore >= 60)
                            {
                                sub.Accepted = true;
                                await dbCtx.SaveChangesAsync();

                                List<NewTrack> newTracks = dbCtx.NewTracks.ToList();
                                foreach (var accepted in trackSubmissions.OrderBy(x => x.TimeSubmitted).Where(x => x.Accepted && !x.Added).ToList())
                                {
                                    if (newTracks.Count >= 10) break;
                                    dbCtx.NewTracks.Add(accepted.ToNewTrack());
                                    accepted.Added = true;
                                }
                                await dbCtx.SaveChangesAsync();
                                try
                                {
                                    await submitter.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                                        new DiscordEmbedBuilder
                                        {
                                            Color = new DiscordColor("#0070FF"),
                                            Description = $"# Track Submission #{sub.ID}" +
                                            $"\nThe track {sub.Name} was accepted with a {sub.FinalScore}% vote for acceptance.",
                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                            {
                                                Text = $"Server Time: {DateTime.Now}"
                                            }
                                        })
                                    );
                                }
                                catch
                                {
                                    Console.WriteLine($"{submitter.Nickname} does not have DMs accessible.");
                                }
                                await message.Channel.SendMessageAsync($"The track {sub.Name} was accepted with a {sub.FinalScore}% vote for acceptance.");
                            }
                            else
                            {
                                sub.Rejected = true;
                                sub.RejectionReason = $"Poll produced results of less than 60%: {sub.FinalScore}";
                                await dbCtx.SaveChangesAsync();
                                try
                                {
                                    await submitter.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                                        new DiscordEmbedBuilder
                                        {
                                            Color = new DiscordColor("#0070FF"),
                                            Description = $"# Track Submission #{sub.ID}" +
                                            $"\nThe track {sub.Name} was rejected with a {sub.FinalScore}% vote for acceptance.",
                                            Footer = new DiscordEmbedBuilder.EmbedFooter
                                            {
                                                Text = $"Server Time: {DateTime.Now}"
                                            }
                                        })
                                    );
                                }
                                catch
                                {
                                    Console.WriteLine($"{submitter.Nickname} does not have DMs accessible.");
                                }
                                await message.Channel.SendMessageAsync($"The track {sub.Name} was rejected with a {sub.FinalScore}% vote for acceptance.");
                            }
                        }
                        else
                        {
                            isPollRunning = true;
                        }
                    }
                }
                if (!isPollRunning)
                {
                    DiscordChannel channel = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.TRACK_VOTING);
                    DiscordRole pollsRole = Bot.Client.GetGuildAsync(GuildID.VP).Result.GetRole(RoleID.POLLS);

                    List<TrackSubmission> queuedSubmissions = dbCtx.Submissions.Where(x => x.Pending == false && x.Accepted == false && x.Rejected == false && x.PollID == null).OrderBy(x => x.TimeSubmitted).ToList();
                    if (queuedSubmissions.Count > 0)
                    {
                        TrackSubmission submission = queuedSubmissions.First();
                        DiscordPollBuilder poll = new DiscordPollBuilder()
                        {
                            Duration = 168,
                            IsMultipleChoice = false,
                            Question = "Should this track be added to Variety Pack?"
                        }
                        .AddOption("Accept", new DiscordComponentEmoji() { Id = 1249668179052068935, Name = "yes" })
                        .AddOption("Reject", new DiscordComponentEmoji() { Id = 1249668216041635900, Name = "no" });
                        DiscordMessage message = await channel.SendMessageAsync(new DiscordMessageBuilder()
#if DEBUG == false
                        .WithAllowedMention(new RoleMention(pollsRole))
#endif
                            .WithPoll(poll)
                            .WithContent($"{pollsRole.Mention} **#{submission.ID} {submission.Name}:** {submission.WikiPage} - {submission.Video}\n" +
                            $"Ensure you either play the track or watch the showcase before voting. Playing and testing the track before voting is much appreciated."));

                        submission.PollID = message.Id.ToString();
                    }
                    await dbCtx.SaveChangesAsync();
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
                if (ctx != null) await ctx.EditResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
