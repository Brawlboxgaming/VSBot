using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using VPBot.Commands.General;
using VPBot.Commands.Scheduling;
using VPBot.Commands.Sheet;

namespace VPBot.Main
{
    public class RegisterCommands
    {
        public static void RegisterAllCommands()
        {
            //General
            Bot.SlashCommands.RegisterCommands<Help>();
            Bot.SlashCommands.RegisterCommands<Source>();

            //Scheduling
            Bot.SlashCommands.RegisterCommands<Update>();

            //Sheet
            Bot.SlashCommands.RegisterCommands<DownloadTracks>();
#if DEBUG
            //Bot.SlashCommands.RegisterCommands<Testing>();
#endif
        }
    }
}
