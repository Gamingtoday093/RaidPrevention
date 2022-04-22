using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Logging;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace RaidPrevention.Connections
{
    public class DiscordWebhook
    {
        public static async Task SendDiscordWebhook(string URL, string jsonData)
        {
            WebRequest client = WebRequest.Create(URL);
            client.Method = "POST";
            client.ContentType = "application/json";

            using (var writer = new StreamWriter(client.GetRequestStream()))
            {
                writer.Write(jsonData);
                writer.Flush();
            }

            try
            {
                await client.GetResponseAsync();
            }
            catch (WebException we)
            {
                Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Failed to Post to Discord API - {we}");
            }
        }

        public static string FormatDiscordWebhook(string Username, string AvatarURL, string Title, int Color, string FooterText, string IconUrl, string PlayerName, ulong SteamID, string Build, string IP)
        {
            return JsonConvert.SerializeObject(new 
            {
                username = Username,
                avatar_url = AvatarURL,
                embeds = new[]
                {
                    new
                    {
                        title = Title,
                        color = Color,
                        fields = new[]
                        {
                            new
                            {
                                name = PlayerName,
                                value = SteamID.ToString(),
                                inline = true
                            },
                            new
                            {
                                name = "Buildable",
                                value = Build,
                                inline = false
                            },
                            new
                            {
                                name = "Server IP",
                                value = IP,
                                inline = false
                            }
                        },
                        footer = new
                        {
                            text = FooterText,
                            icon_url = IconUrl
                        }
                    }
                }
            });
        }
    }
}
