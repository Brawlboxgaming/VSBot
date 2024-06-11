using System.ComponentModel;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using HtmlAgilityPack;

namespace VPBot.Commands.Sheet
{
    public class DownloadTracks
    {
        [Command("downloadtracks")]
        [Description("Downloads tracks listed on the spreadsheet (outdated by default).")]
        public async Task DownloadTracksCommand(CommandContext ctx,
            [Choice("True", "true")]
            [Choice("False", "false")]
            [Option("new-tracks", "Only downloads tracks that are out-of-date")] string newTracks = "true")
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
                bool allTracks = newTracks == "true" ? false : true;

                string serviceAccountEmail = "sheetbox@sonic-fiber-399810.iam.gserviceaccount.com";
                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);
                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Variety Pack Bot",
                });

                var request = service.Spreadsheets.Values.Get("19mtwtrQCgdrLEAb-z_sSd4_1tIGoJ12RjEaC7pzIyn8", "'Custom Tracks'");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
                var response = await request.ExecuteAsync();

                Directory.CreateDirectory("tracks");
                int dls = 0;

                for (int i = 1; i < response.Values.Count; i++)
                {
                    object link = response.Values[i][Util.SheetColumn.Link];

                    if (allTracks || response.Values[i][Util.SheetColumn.UpdateStatus].ToString() == "MAY NEED UPDATE")
                    {
                        HtmlWeb htmlWeb = new HtmlWeb()
                        {
                            UserAgent = Util.GetUserAgent()
                        };
                        HtmlDocument doc = await htmlWeb.LoadFromWebAsync(link.ToString());
                        var nodes = doc.DocumentNode.SelectNodes("//tr/td");
                        foreach (var node in nodes)
                        {
                            if (node.InnerText == "ct.wiimm.de\n")
                            {
                                WebClient webClient = new WebClient();
                                string html = webClient.DownloadString(node.InnerHtml.Split('"')[5]);
                                HtmlDocument wiimmDoc = new HtmlDocument();
                                wiimmDoc.LoadHtml(html);
                                var trackNameVer = response.Values[i][Util.SheetColumn.Name].ToString().Split('(')[0].Trim() + $" {response.Values[i][Util.SheetColumn.PackVer]}";
                                var wiimmNodes = wiimmDoc.DocumentNode.SelectNodes("//tr/td");
                                for (int j = 0; j < wiimmNodes.Count; j++)
                                {
                                    if (wiimmNodes[j].InnerText.Contains(trackNameVer))
                                    {
                                        var id = wiimmNodes[j - 7].InnerText;
                                        webClient.DownloadFile($"https://ct.wiimm.de/dl/@myLhAVA9/{id}", $"tracks/{response.Values[i][Util.SheetColumn.Name].ToString().Split('(')[0].Trim()}.wbz");
                                        dls++;
                                        if (dls % 30 == 0)
                                        {
                                            Thread.Sleep(600000);
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                if (Directory.EnumerateFileSystemEntries("tracks").Any())
                {
                    ZipFile.CreateFromDirectory("tracks", "C:/Files/VPTesting/Tracks.zip");
                }

                Directory.Delete("tracks", true);

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Success\n" +
                    "*https://files.brawlbox.co.uk/VPTesting/Tracks.zip*",
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
