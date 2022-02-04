using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BotBattlefield
{
    public class HttpUtils
    {
        private const string BaseUrl = "https://api.gametools.network";
        public static HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(12)
        };

        public static async Task<BF1StateObj?> GetStats(GameType game, string id)
        {
            BotMain.Log($"正在获取[{id}]的{game.url}数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/stats/?format_values=true&name={id}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;                
            }

            return JsonConvert.DeserializeObject<BF1StateObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<BF1ServerObj?> GetServers(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]BF1的服务器信息");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/servers/?name={name}&platform=pc&region=all&limit=10");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1ServerObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<byte[]> GetImage(string url) 
        {
            return await client.GetByteArrayAsync(url);
        }
    }
}
