using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net;
using System.Web;

var client = new HttpClient()
{
    Timeout = TimeSpan.FromSeconds(20),
};

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12| SecurityProtocolType.Tls13;

//Directory.CreateDirectory("vehicles");

//var res = await client.GetStringAsync("https://api.gametools.network/bf1/vehicles/?name=color_yr&platform=pc&skip_battlelog=false&lang=en-us");

//var res1= JsonConvert.DeserializeObject<BF1VehiclesObj>(res);
//foreach (var item in res1.vehicles)
//{
//    string name = $"vehicles/{item.type}/{item.vehicleName.Replace("/", "_")}.png";
//    if (File.Exists(name))
//        continue;
//    Directory.CreateDirectory($"vehicles/{item.type}");
//    var res2 = await client.GetByteArrayAsync(item.image);
//    Thread.Sleep(500);
//    File.WriteAllBytes(name, res2);
//}

//var res = await client.GetStringAsync("https://battlefieldtracker.com/bf1/profile/pc/Color_yr");

// From String
//var doc = new HtmlDocument();
//doc.LoadHtml(res);

//bool GetClass(HtmlNode node, string name)
//{
//    if (!node.Attributes.Contains("class"))
//    {
//        return false;
//    }
//    return node.Attributes["class"].Value == name;
//}

//var state = new BF1StateObj();

//var nodes = doc.DocumentNode.Descendants("div")
// .Where(x => GetClass(x, "current-rank"));

//if (nodes.Any())
//{
//    foreach (var item in nodes)
//    {
//        var img = item.Descendants("img");
//        var span = item.Descendants("span");

//        if (img.Any())
//        {
//            state.rankImg = img.First().Attributes["src"].Value;
//        }

//        if (span.Any())
//        {
//            state.rank = span.First().InnerText.Replace("Rank ", "");
//        }
//    }
//}


//Directory.CreateDirectory("Map");
//Directory.CreateDirectory("Teams");

//var res = await client.GetStringAsync("https://api.gametools.network/bf1/servers/?name=d&region=all&platform=pc&limit=1000&lang=en-us");

//var res1 = JsonConvert.DeserializeObject<BF1ServerObj>(res);
//foreach (var item in res1.servers)
//{
//    string name = $"Map/{item.currentMap}.png";
//    name = name.Replace(":", "_");
//    if (File.Exists(name))
//        continue;
//    var res2 = await client.GetByteArrayAsync(item.url);
//    Thread.Sleep(500);
//    File.WriteAllBytes(name, res2);
//}

//foreach (var item in res1.servers)
//{
//    foreach (var item1 in item.teams)
//    {
//        string name = $"Teams/{item1.Value.name}.png";
//        if (File.Exists(name))
//            continue;
//        var res2 = await client.GetByteArrayAsync(item1.Value.image);
//        Thread.Sleep(500);
//        File.WriteAllBytes(name, res2);
//    }

//}

const string BaseUrl = "https://battlefieldtracker.com";

var res = await client.GetStringAsync("https://battlefieldtracker.com/bf1/profile/pc/Color_yr/matches");

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
                var temp = item1.InnerText.Split(" ");
                obj.map = temp[0];
                obj.servername = HttpUtility.HtmlDecode(temp[1]);
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
            var name = item3.ChildNodes.Where(a => a.HasClass("name")).Select(a => a.InnerText).First();
            var value = item3.ChildNodes.Where(a => a.HasClass("value")).Select(a => a.InnerText).First();

            if (name == "Score")
                obj.Score = value;
            else if (name == "Kills")
                obj.Kills = value;
            else if (name == "Deaths")
                obj.Deaths = value;
            else if (name == "Assists")
                obj.Assists = value;
            else if (name == "K/D")
                obj.KD = value;
        }
    }

    list.Add(obj);
}

//Console.WriteLine(state.rankImg);