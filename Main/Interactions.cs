using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using VPBot.Classes;
using VPBot.Commands;

namespace VPBot
{
    public class Interactions
    {
        public async Task AssignAllInteractions()
        {
            Bot.Client.InteractionCreated += LogInteractions;
            Bot.Client.VoiceStateUpdated += UpdateVoiceChannels;
            Bot.Client.GuildMemberAdded += LogNewMembers;
            Bot.Client.GuildMemberUpdated += LogMemberRolesUpdated;
            Bot.Client.GuildMemberRemoved += LogRemovedMembers;
            Bot.Client.MessageDeleted += LogMessageDeleted;
            Bot.Client.MessageUpdated += LogMessageUpdated;

            await Task.CompletedTask;
        }

        private async Task LogMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
        {
            try
            {
                if (!args.Author.IsBot)
                {
                    DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogMemberRolesUpdated(DiscordClient sender, GuildMemberUpdateEventArgs args)
        {
            try
            {
                DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
        {
            try
            {
                DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogRemovedMembers(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
            try
            {
                DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

                var auditLogs = await args.Guild.GetAuditLogsAsync(1);
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
                if (removeType == AuditLogActionType.Kick)
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
                else if (removeType == AuditLogActionType.Ban)
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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogNewMembers(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            try
            {
                await args.Member.GrantRoleAsync(args.Guild.GetRole(451111103225921547));
                DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

                Console.WriteLine(ex.ToString());
            }
        }

        private async Task LogInteractions(DiscordClient sender, InteractionCreateEventArgs args)
        {
            DiscordChannel channel = Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannel(ChannelID.BOTLOG);

            string options = "";

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

        private bool _updatingChannels = false;
        private bool _eventDuringChannelUpdate = false;

        private async Task UpdateVoiceChannels(DiscordClient sender, VoiceStateUpdateEventArgs args)
        {
            try
            {
                DiscordChannel channel = args.Guild.GetChannel(ChannelID.SERVERLOG);

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
                await args.Guild.GetChannel(ChannelID.BOTLOG).SendMessageAsync(embed);

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
                        DiscordChannel logChannel = guild.GetChannel(ChannelID.SERVERLOG);
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

                        if (channelInfos[chIx].Channel.GetMessagesAsync(1000).Result.Count > 0)
                        {
                            string txtFile = $"Last 1000 Messages from {channelInfos[chIx].Channel.Name}:\r\n";
                            var messages = channelInfos[chIx].Channel.GetMessagesAsync(1000).Result.ToList();
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
                    DiscordChannel logChannel = guild.GetChannel(ChannelID.SERVERLOG);
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
                await guild.CreateChannelAsync($"{name} #{curNum}", ChannelType.Voice, parent: lastChannel.Parent, position: lastChannel.Position, userLimit: lastChannel.UserLimit);

                DiscordChannel logChannel = Bot.Client.GetGuildAsync(GuildID.VP).Result.GetChannel(ChannelID.SERVERLOG);
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
