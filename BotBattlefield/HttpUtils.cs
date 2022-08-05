using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BotBattlefield
{
    public class HttpUtils
    {
        private const string BaseUrl = "https://api.gametools.network";

        public static void Init()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                
            };
            
            if (BotMain.Config.Proxy.Enable)
            {
                WebProxy proxy = new(BotMain.Config.Proxy.IP, BotMain.Config.Proxy.Port);
                socketsHandler.Proxy = proxy;
            }
            client = new()
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
        }

        public static HttpClient client;

        public static void Stop()
        {
            client?.Dispose();
        }

        public static async Task<BF1StateObj?> GetStats(GameType game, string name, string platform)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/stats/?format_values=true&name={HttpUtility.UrlEncode(name)}&platform={platform}");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            var data1 = await data.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BF1StateObj>(data1);
        }

        public static async Task<BF1ServerObj?> GetServers(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]BF1的服务器信息");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/servers/?name={HttpUtility.UrlEncode(name)}&platform=pc&region=all&limit=10");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1ServerObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<BF1WeaponsObj?> GetWeapons(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}武器数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/weapons/?format_values=true&name={HttpUtility.UrlEncode(name)}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1WeaponsObj>(await data.Content.ReadAsStringAsync());
        }

        public static async Task<BF1VehiclesObj?> GetVehicles(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}载具数据");
            var data = await client.GetAsync($"{BaseUrl}/{game.url}/vehicles/?format_values=true&name={HttpUtility.UrlEncode(name)}&platform=pc");
            if (data.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BF1VehiclesObj>(await data.Content.ReadAsStringAsync());
        }

        static bool GetClass(HtmlNode node, string name)
        {
            if (!node.Attributes.Contains("class"))
            {
                return false;
            }
            return node.Attributes["class"].Value == name;
        }

        public static async Task<int> GetRank(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}等级数据");
            var res = await client.GetStringAsync($"https://battlefieldtracker.com/{game.url}/profile/pc/{name}");

            var doc = new HtmlDocument();
            doc.LoadHtml(res);

            var nodes = doc.DocumentNode.Descendants("div")
             .Where(x => GetClass(x, "current-rank"));

            string rank = "";

            if (nodes.Any())
            {
                foreach (var item in nodes)
                {
                    var span = item.Descendants("span");

                    if (span.Any())
                    {
                        rank = span.First().InnerText.Replace("Rank ", "");
                    }
                }
            }
            return int.Parse(rank);
        }

        public static async Task<List<MatchesItem>> GetMatches(GameType game, string name)
        {
            BotMain.Log($"正在获取[{name}]的{game.url}最近数据");
            const string BaseUrl = "https://battlefieldtracker.com";

            var res = await client.GetStringAsync($"https://battlefieldtracker.com/{game.url}/profile/pc/{name}/matches");

            var doc = new HtmlDocument();
            doc.LoadHtml(res);

            bool GetClass(HtmlNode node, string name)
            {
                if (!node.Attributes.Contains("class"))
                {
                    return false;
                }
                return node.Attributes["class"].Value == name;
            }

            var nodes = doc.DocumentNode.Descendants("a")
             .Where(x => GetClass(x, "match"));

            List<MatchesItem> list = new();

            int a = 0;
            foreach (var item in nodes)
            {
                if (a >= 5)
                    break;
                try
                {
                    a++;
                    var obj = new MatchesItem();
                    var url = BaseUrl + item.Attributes["href"].Value;

                    var res1 = await client.GetStringAsync(url);
                    var doc1 = new HtmlDocument();
                    doc1.LoadHtml(res1);

                    var nodes1 = doc1.DocumentNode.Descendants("div")
                        .Where(x => GetClass(x, "activity-details"));
                    if (nodes1.Any())
                    {
                        foreach (var item1 in nodes1.First().ChildNodes)
                        {
                            if (item1.HasClass("map-name"))
                            {
                                var temp = item1.InnerHtml.Split(" <small class=\"hidden-sm hidden-xs\">");
                                obj.map = temp[0];
                                obj.servername = HttpUtility.HtmlDecode(temp[1])[..^8];
                            }
                            else if (item1.HasClass("type"))
                            {
                                obj.type = item1.InnerText;
                            }
                            else if (item1.HasClass("date"))
                            {
                                obj.time = item1.InnerText;
                            }
                        }
                    }

                    var nodeselect = doc1.DocumentNode.Descendants("div")
                        .Where(x => x.HasClass("active")).First();
                    nodeselect = nodeselect.Descendants("div")
                        .Where(x => x.HasClass("player-header")).First();

                    var nodes2 = nodeselect.Descendants("span").Where(x => x.HasClass("player-subline"));
                    if (nodes2.Any())
                    {
                        obj.playtime = nodes2.First().InnerText.Trim();
                    }

                    nodes2 = nodeselect.Descendants("div").Where(x => x.HasClass("quick-stats"));
                    foreach (var item2 in nodes2)
                    {
                        var nodes3 = item2.Descendants("div").Where(x => x.HasClass("stat"));
                        foreach (var item3 in nodes3)
                        {
                            var name1 = item3.ChildNodes.Where(a => a.HasClass("name")).Select(a => a.InnerText).First();
                            var value = item3.ChildNodes.Where(a => a.HasClass("value")).Select(a => a.InnerText).First();

                            if (name1 == "Score")
                                obj.Score = value;
                            else if (name1 == "Kills")
                                obj.Kills = value;
                            else if (name1 == "Deaths")
                                obj.Deaths = value;
                            else if (name1 == "Assists")
                                obj.Assists = value;
                            else if (name1 == "K/D")
                                obj.KD = value;
                        }
                    }

                    list.Add(obj);
                }
                catch
                {

                }

            }

            return list;
        }

        public static async Task<byte[]> GetImage(string url)
        {
            return await client.GetByteArrayAsync(url);
        }
    }
}
