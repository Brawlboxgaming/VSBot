using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using VPBot.Commands.General;
using VPBot.Commands.Scheduling;
using VPBot.Commands.Sheet;
using VPBot.Commands.Testing;
using VPBot.Commands.TrackManagement;
using VPBot.Commands.Wiimmfi;

namespace VPBot.Main
{
    public class RegisterCommands
    {
        public static void RegisterAllCommands()
        {
            Bot.Commands.AddCommands(new List<Type>() {
                typeof(Help),
                typeof(Source),
                typeof(Update),
                typeof(DownloadTracks),
                typeof(CheckTrackSubmissions),
                typeof(ClearNewTracks),
                typeof(RejectTrack)
#if DEBUG
                ,typeof(Testing)
#endif
            });
        }
    }
}
