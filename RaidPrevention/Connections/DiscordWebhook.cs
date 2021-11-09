using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Logging;

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
    }
}
