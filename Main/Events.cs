using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;
using VPBot.Classes;
using VPBot.Commands;
using DocumentFormat.OpenXml.Presentation;

namespace VPBot
{
    public class Events
    {
        public async Task AssignAllEvents()
        {
            Bot.Builder.ConfigureEventHandlers
            (
                b => b.HandleInteractionCreated(LogEvents)
                .HandleVoiceStateUpdated(UpdateVoiceChannels)
                .HandleGuildMemberAdded(LogNewMembers)
                .HandleGuildMemberUpdated(LogMemberRolesUpdated)
                .HandleGuildMemberRemoved(LogRemovedMembers)
                .HandleMessageDeleted(LogMessageDeleted)
                .HandleMessageUpdated(LogMessageUpdated)
                .HandleComponentInteractionCreated(async (c, e) =>
                {
                    if (e.Id == "track_submission") await TrackModel(e);
                    if (e.Id.StartsWith("tickButton-Submission")) await AcceptSubmission(e);
                    if (e.Id.StartsWith("crossButton-Submission")) await RejectionModel(e);
                })
                .HandleModalSubmitted(async (c, e) =>
                {
                    if (e.Interaction.Data.CustomId == "track_submission_modal") await TrackModelSubmit(e);
                    if (e.Interaction.Data.CustomId.StartsWith("track_rejection_modal")) await RejectSubmission(e, e.Interaction.Data.CustomId.Split('-')[1]);
                })
            );

            await Task.CompletedTask;
        }

        private async Task RejectSubmission(ModalSubmittedEventArgs e, string dbId)
        {
            try
            {
                await e.Interaction.DeferAsync(true);

                string reason = e.Values.First(x => x.Key == "reason").Value;
                VPContext dbCtx = new();
                TrackSubmission submission = dbCtx.Submissions.First(x => x.ID.ToString() == dbId);
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
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
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
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
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
                await foreach (var m in e.Interaction.Channel.GetMessagesAsync())
                {
                    if (m.Embeds[0].Description.StartsWith($"# Track Submission #{submission.ID}"))
                    {
                        await m.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = "# Rejected\n" + m.Embeds[0].Description,
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        }));
                    }
                }

            }
            catch (Exception ex)
            {
                await Util.ThrowInteractionlessError(ex);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error\n" + ex,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                }));
            }
        }

        private async Task RejectionModel(ComponentInteractionCreatedEventArgs e)
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                .WithTitle("Reject a Track Submission")
                .WithCustomId($"track_rejection_modal-{e.Interaction.Data.CustomId.Split('#')[1]}")
                .AddComponents(new DiscordTextInputComponent(label: "Reason", customId: "reason", required: true, style: DiscordTextInputStyle.Paragraph))
            );
        }

        private async Task AcceptSubmission(ComponentInteractionCreatedEventArgs e)
        {
            try
            {
                await e.Interaction.DeferAsync(true);

                VPContext dbCtx = new();
                string submissionId = e.Interaction.Data.CustomId.Split('#')[1];
                TrackSubmission submission = dbCtx.Submissions.First(x => x.ID.ToString() == submissionId);
                submission.Pending = false;
                await dbCtx.SaveChangesAsync();

                DiscordMember submitter = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetMemberAsync(ulong.Parse(submission.SubmitterID));
                try
                {
                    await submitter.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = $"# Track Submission #{submission.ID}" +
                            $"\nThe track {submission.Name} was added to the voting queue.",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        })
                    );
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = "# Success\nThe submission has been added to the queue and the submitter has been notified.",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    }));
                }
                catch
                {
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = "# Success\nThe submission has been added to the queue, but the submitter could not be notified.",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    }));
                    Console.WriteLine($"{submitter.Nickname} does not have DMs accessible.");
                }
                await foreach (var m in e.Interaction.Channel.GetMessagesAsync())
                {
                    if (m.Embeds[0].Description.StartsWith($"# Track Submission #{submission.ID}"))
                    {
                        await m.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = "# Accepted\n" + m.Embeds[0].Description,
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowInteractionlessError(ex);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error\n" + ex,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                }));
            }
        }

        private async Task TrackModelSubmit(ModalSubmittedEventArgs e)
        {
            try
            {
                await e.Interaction.DeferAsync(true);

                string name = e.Values.First(x => x.Key == "name").Value;
                string page = e.Values.First(x => x.Key == "page").Value.Replace(" ", "_");
                string video = e.Values.First(x => x.Key == "video").Value;
                string comments = e.Values.First(x => x.Key == "comments").Value;

                TrackSubmission submission = new(name, page, video, comments);
                submission.SubmitterID = e.Interaction.User.Id.ToString();

                VPContext dbCtx = new();

                List<TrackSubmission> existingSubmissions = dbCtx.Submissions.Where(x => x.Name == name || x.Video == video || x.WikiPage == page).ToList();

                dbCtx.Submissions.Add(submission);

                await dbCtx.SaveChangesAsync();

                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Success\nThe track has been submitted.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                }));

                string existingString = "";
                if (existingSubmissions.Count > 0)
                {
                    existingString = "\n## Potential Duplicates:";
                    foreach (var sub in existingSubmissions)
                    {
                        existingString += $"\n*#{sub.ID} {sub.Name}*";
                    }
                }

                DiscordChannel channel = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.SUBMISSION_APPROVAL);
                DiscordRole teamRole = Bot.Client.GetGuildAsync(GuildID.VP).Result.GetRole(RoleID.VP_TEAM);

                await channel.SendMessageAsync(new DiscordMessageBuilder()
#if DEBUG == false
                    .WithAllowedMention(new RoleMention(teamRole))
#endif
                    .WithContent(teamRole.Mention)
                    .AddEmbed(
                    new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Track Submission #{submission.ID}" +
                        $"{existingString}" +
                        $"\n\n## Track Name:" +
                        $"\n{submission.Name}" +
                        $"\n## Wiki Page:" +
                        $"\n{submission.WikiPage}" +
                        $"\n## Video Showcase:" +
                        $"\n{submission.Video}" +
                        $"\n## Comments:" +
                        $"\n{(submission.Comments == "" ? "*No comment.*" : submission.Comments)}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    }).AddComponents(Util.GenerateTickAndCrossButtons($"Submission#{submission.ID}"))
                );
            }
            catch (Exception ex)
            {
                await Util.ThrowInteractionlessError(ex);
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error\n" + ex,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                }));
            }
        }

        private async Task TrackModel(ComponentInteractionCreatedEventArgs e)
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                .WithTitle("Custom Track Submission")
                .WithCustomId("track_submission_modal")
                .AddComponents(new DiscordTextInputComponent(label: "Track Name", customId: "name", placeholder: "Quarter Mile Race"))
                .AddComponents(new DiscordTextInputComponent(label: "Wiki Page", customId: "page", placeholder: "https://wiki.tockdom.com/wiki/Quarter_Mile_Race"))
                .AddComponents(new DiscordTextInputComponent(label: "Video Showcase", customId: "video", placeholder: "https://youtu.be/cxbOMSvja6Q"))
                .AddComponents(new DiscordTextInputComponent(label: "Comments", customId: "comments", required: false, style: DiscordTextInputStyle.Paragraph))
            );
        }

        private async Task LogMessageUpdated(DiscordClient sender, MessageUpdatedEventArgs args)
        {
            if (args != null)
            {
                try
                {
                    if (!args.Author.IsBot)
                    {
                        DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

                        if (args.MessageBefore == null)
                        {
                            embed = new()
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = $"# Notice:\n" +
                                $"The message ({args.Message.JumpLink}) has been updated.\n" +
                                $"``{args.Message.Content}``",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            };
                        }
                        else
                        {
                            embed = new()
                            {
                                Color = new DiscordColor("#0070FF"),
                                Description = $"# Notice:\n" +
                                $"The message ({args.Message.JumpLink}) has been updated.\n" +
                                $"Before:\n" +
                                $"``{args.MessageBefore.Content}``\n\n" +
                                $"After:\n" +
                                $"``{args.Message.Content}``",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            };
                        }
                        await channel.SendMessageAsync(embed);
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
                    await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private async Task LogMemberRolesUpdated(DiscordClient sender, GuildMemberUpdatedEventArgs args)
        {
            try
            {
                DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                IEnumerable<DiscordRole> beforeRoles = args.MemberBefore.Roles;
                IEnumerable<DiscordRole> afterRoles = args.MemberAfter.Roles;
                foreach (DiscordRole role in beforeRoles)
                {
                    if (!afterRoles.Contains(role))
                    {
                        DiscordEmbedBuilder embed = new()
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = $"# Notice:\n" +
                            $"{role.Mention} was removed from {args.Member.Mention}",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        await channel.SendMessageAsync(embed);
                        continue;
                    }
                }
                foreach (DiscordRole role in afterRoles)
                {
                    if (!beforeRoles.Contains(role))
                    {
                        DiscordEmbedBuilder embed = new()
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = $"# Notice:\n" +
                            $"{role.Mention} was given to {args.Member.Mention}",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        await channel.SendMessageAsync(embed);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error\n" + ex.Message,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogMessageDeleted(DiscordClient sender, MessageDeletedEventArgs args)
        {
            try
            {
                DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                DiscordEmbedBuilder embed = new()
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = $"# Notice:\n" +
                    $"The following message was deleted from {args.Channel.Mention}:\n" +
                    $"``{args.Message.Content}``",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await channel.SendMessageAsync(embed);
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
                await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogRemovedMembers(DiscordClient sender, GuildMemberRemovedEventArgs args)
        {
            try
            {
                DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                List<DiscordAuditLogEntry> auditLogs = new List<DiscordAuditLogEntry>();
                await foreach (var a in args.Guild.GetAuditLogsAsync(1))
                {
                    auditLogs.Add(a);
                }
                var removeType = auditLogs.First().ActionType;

                DiscordEmbedBuilder embed = new()
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = $"# Notice:\n" +
                        $"Member left: {args.Member.Mention}",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                if (removeType == DiscordAuditLogActionType.Kick)
                {
                    embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                        $"Member kicked: {args.Member.Mention}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                }
                else if (removeType == DiscordAuditLogActionType.Ban)
                {
                    embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                        $"Member banned: {args.Member.Mention}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                }
                await channel.SendMessageAsync(embed);
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
                await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogNewMembers(DiscordClient sender, GuildMemberAddedEventArgs args)
        {
            try
            {
                await args.Member.GrantRoleAsync(args.Guild.GetRole(451111103225921547));
                DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                DiscordEmbedBuilder embed = new()
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = $"# Notice:\n" +
                    $"New member: {args.Member.Mention}",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await channel.SendMessageAsync(embed);
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
                await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogEvents(DiscordClient sender, InteractionCreatedEventArgs args)
        {
            DiscordChannel channel = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.BOT_LOGS);

            string options = "";

            if (args.Interaction.Data.Name != null)
            {
                if (args.Interaction.Data.Options != null)
                {
                    foreach (DiscordInteractionDataOption option in args.Interaction.Data.Options)
                    {
                        options += $" {option.Name}: *{option.Value}*";
                    }
                }

                DiscordEmbedBuilder embed = new()
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = $"# Notice:\n" +
                    $"'/{args.Interaction.Data.Name}{options}' was used by {args.Interaction.User.Mention}.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await channel.SendMessageAsync(embed);
            }
        }

        private bool _updatingChannels = false;
        private bool _eventDuringChannelUpdate = false;

        private async Task UpdateVoiceChannels(DiscordClient sender, VoiceStateUpdatedEventArgs args)
        {
            try
            {
                DiscordChannel channel = await args.Guild.GetChannelAsync(ChannelID.SERVER_LOGS);

                DiscordEmbedBuilder embed = new();

                if (args.Before == null || args.Before.Channel == null)
                {
                    embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                            $"{args.User.Mention} has joined {args.After.Channel.Mention}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                }
                else if (args.After == null || args.After.Channel == null)
                {
                    embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                            $"{args.User.Mention} has left {args.Before.Channel.Mention}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                }
                else
                {
                    embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                            $"{args.User.Mention} has moved from {args.Before.Channel.Mention} to {args.After.Channel.Mention}",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                }
                await channel.SendMessageAsync(embed);
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
                await args.Guild.GetChannelAsync(ChannelID.BOT_LOGS).Result.SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        again:
            if (_updatingChannels)
            {
                _eventDuringChannelUpdate = true;
                return;
            }
            _updatingChannels = true;
            try
            {
                // Check if the channel(s) has/have the format "<name> #<number>"
                string? channelName = null;
                Match m;
                if (args.Before != null && args.Before.Channel != null && (m = Regex.Match(args.Before.Channel.Name, @"^(.*) #\d+$")).Success)
                {
                    channelName = m.Groups[1].Value;
                    await UpdateVoiceChannelCollection(args.Guild, channelName);
                }
                if (args.After != null && args.After.Channel != null &&
                    (m = Regex.Match(args.After.Channel.Name, @"^(.*) #\d+$")).Success && m.Groups[1].Value != channelName)
                {
                    await UpdateVoiceChannelCollection(args.Guild, m.Groups[1].Value);
                }

                _updatingChannels = false;
                if (_eventDuringChannelUpdate)
                {
                    _eventDuringChannelUpdate = false;
                    goto again;
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowInteractionlessError(ex);
            }
            finally
            {
                _updatingChannels = false;
                _eventDuringChannelUpdate = false;
            }
        }

        private static async Task UpdateVoiceChannelCollection(DiscordGuild guild, string name)
        {
            // Get a list of all voice channels with this name, their number, and the number of users in them
            var channelInfos = (await guild.GetChannelsAsync())
                .Select(ch => new { Channel = ch, Match = Regex.Match(ch.Name, $@"^{Regex.Escape(name)} #(\d+)$") })
                .Where(inf => inf.Match.Success)
                .Select(inf => new { inf.Channel, UserCount = inf.Channel.Users.Count, Number = int.Parse(inf.Match.Groups[1].Value) })
                .OrderBy(inf => inf.Number)
                .ToList();

            // Delete empty voice channels except for the lowest-numbered one
            bool isFirst = true;
            for (int chIx = 0; chIx < channelInfos.Count; chIx++)
            {
                if (channelInfos[chIx].UserCount == 0)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        DiscordChannel logChannel = await guild.GetChannelAsync(ChannelID.SERVER_LOGS);
                        DiscordEmbedBuilder embed = new()
                        {
                            Color = new DiscordColor("#0070FF"),
                            Description = $"# Notice:\n" +
                            $"Deleted VC {channelInfos[chIx].Channel.Name}.",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };

                        await logChannel.SendMessageAsync(embed);

                        List<DiscordMessage> messages = new List<DiscordMessage>();
                        await foreach (var m in channelInfos[chIx].Channel.GetMessagesAsync(1000))
                        {
                            messages.Add(m);
                        }

                        if (messages.Count > 0)
                        {
                            string txtFile = $"Last 1000 Messages from {channelInfos[chIx].Channel.Name}:\r\n";
                            messages.Reverse();
                            foreach (var message in messages)
                            {
                                txtFile += $"[{message.CreationTimestamp}] {message.Author.Username}: {message.Content}";
                                foreach (var attachment in message.Attachments)
                                {
                                    txtFile += $" {attachment.Url}";
                                }
                                txtFile += "\r\n";
                            }
                            string fileName = $"{DateTime.Now.ToString().Replace(":", "").Replace("/", "-")} - {channelInfos[chIx].Channel.Name}.txt";
                            await File.WriteAllTextAsync(fileName, txtFile);
                            Stream stream = File.Open(fileName, FileMode.Open);
                            await logChannel.SendMessageAsync(new DiscordMessageBuilder().AddFile(fileName, stream));
                            stream.Close();
                            await stream.DisposeAsync();
                            File.Delete(fileName);
                        }
                        await channelInfos[chIx].Channel.DeleteAsync();
                        channelInfos.RemoveAt(chIx);
                        chIx--;
                    }
                }
            }

            // Rename channels whose numbers are now out of order
            bool hasEmpty = false;
            int curNum = 1;
            foreach (var ch in channelInfos)
            {
                if (ch.Number != curNum)
                {
                    DiscordChannel logChannel = await guild.GetChannelAsync(ChannelID.SERVER_LOGS);
                    DiscordEmbedBuilder embed = new()
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = $"# Notice:\n" +
                        $"Renamed VC {ch.Channel.Name} to {name} #{curNum}.",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };

                    await logChannel.SendMessageAsync(embed);

                    await ch.Channel.ModifyAsync(cem => cem.Name = $"{name} #{curNum}");
                }
                curNum++;
                hasEmpty = hasEmpty || ch.UserCount == 0;
            }

            // Create a new channel if no channels are empty
            if (!hasEmpty)
            {
                DiscordChannel lastChannel = channelInfos.Last().Channel;
                await guild.CreateChannelAsync($"{name} #{curNum}", DiscordChannelType.Voice, parent: lastChannel.Parent, position: lastChannel.Position, userLimit: lastChannel.UserLimit);

                DiscordChannel logChannel = await Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannelAsync(ChannelID.SERVER_LOGS);
                DiscordEmbedBuilder embed = new()
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = $"# Notice:\n" +
                    $"Created VC {name} #{curNum}.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };

                await logChannel.SendMessageAsync(embed);
            }
        }
    }
}
