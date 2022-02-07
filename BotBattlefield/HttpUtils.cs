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

        public static void Init()
        {
            client = new()
            {
                Timeout = TimeSpan.FromSeconds(20),
            };
            if (BotMain.Config.Proxy.Enable)
            {
                WebProxy proxy = new WebProxy(BotMain.Config.Proxy.IP, BotMain.Config.Proxy.Port);
                HttpClient.DefaultProxy = proxy;
            }
        }

        public static HttpClient client;

        public static async Task<BF1StateObj?> GetStats(GameType game, string id, string platform)
        {
            BotMain.Log($"正在获取[{id}]的{game.url}数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/stats/?format_values=true&name={id}&platform={platform}");
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

        public static async Task<BF1WeaponsObj?> GetWeapons(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}武器数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/weapons/?format_values=true&name={name}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1WeaponsObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<BF1VehiclesObj?> GetVehicles(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}载具数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/vehicles/?format_values=true&name={name}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1VehiclesObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<byte[]> GetImage(string url) 
        {
            return await client.GetByteArrayAsync(url);
        }
    }
}
