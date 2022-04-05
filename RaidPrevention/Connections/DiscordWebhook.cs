using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Logging;
using Newtonsoft.Json;

namespace RaidPrevention.Connections
{
    public class DiscordWebhook
    {
        public static async Task SendDiscordWebhook(string URL, string jsonData)
        {
            HttpClient client = new HttpClient();
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(URL, content);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Status Code: {response.StatusCode} - Failed to Post to Discord API");
            }
            response.EnsureSuccessStatusCode();
        }

        public static string FormatDiscordWebhook(string Username, string AvatarURL, string Title, string Color, string FooterText, string IconUrl, string PlayerName, ulong SteamID, string Build, string IP)
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
