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
            Timeout = TimeSpan.FromSeconds(8)
        };

        public static async Task<StateObj?> GetStats(GameType game, string id)
        {
            BotMain.Log($"正在获取[{id}]的数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/stats/?format_values=true&name={id}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;                
            }

            return JsonConvert.DeserializeObject<StateObj>(await data.Content.ReadAsStringAsync());
        }
        
        public static async Task<byte[]> GetImage(string url) 
        {
            return await client.GetByteArrayAsync(url);
        }
    }
}
