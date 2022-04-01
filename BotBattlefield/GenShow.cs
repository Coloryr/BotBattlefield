using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColoryrSDK;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace BotBattlefield
{
    public class GenShow
    {
        private static string Local;
        private static Font title;
        private static Font font;
        private static Font font1;
        private static Color back;
        private static Color text;
        private static Color back1;
        private static TextOptions FontNormalOpt;

        private static Image star = Image.Load(Resource1.star);

        public static void Init()
        {
            Local = BotMain.Local + "temp/";
            if (!Directory.Exists(Local))
            {
                Directory.CreateDirectory(Local);
            }

            var temp = SystemFonts.Families.Where(a => a.Name == "Microsoft YaHei").FirstOrDefault();

            title = temp.CreateFont(40);
            font = temp.CreateFont(35);
            font1 = temp.CreateFont(30);

            back = Color.Parse("#000000");
            text = Color.Parse("#FFFFFF");
            back1 = Color.Parse("#000000").WithAlpha(0.6f);

            FontNormalOpt = new TextOptions(font1)
            {

            };

            star.Mutate(m => m.Resize(30, 30));
        }
        public static async Task<string> GenState(BF1StateObj obj, GameType game)
        {
            BotMain.Log($"正在生成[{obj.userName}]的{game.url}统计图片");
            Image<Rgba32> image = new(500, 860);
            var head = await HttpUtils.GetImage(obj.avatar);
            var rank = await HttpUtils.GetImage(obj.rankImg);
            var img = Image.Load(head);
            var img1 = Image.Load(rank);
            img = Utils.ZoomImage(img, 120, 120);
            img1 = Utils.ZoomImage(img1, 50, 50);
            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText($"{game.name} 玩家统计", title, text, new PointF(20, 0));
                m.DrawImage(img, new Point(20, 50), 1);
                m.DrawText($"ID {obj.userName}", font1, text, new PointF(150, 70));
                m.DrawText($"等级 {obj.rank}", font1, text, new PointF(210, 120));
                m.DrawImage(img1, new Point(150, 110), 1);

                m.DrawLines(text, 1, new PointF(0, 185), new PointF(500, 185));
                m.DrawText($"个人对局", font1, text, new PointF(20, 190));
                m.DrawLines(text, 1, new PointF(0, 235), new PointF(500, 235));

                m.DrawText($"SPM {obj.scorePerMinute}", font1, text, new PointF(20, 240));
                m.DrawText($"KPM {obj.killsPerMinute}", font1, text, new PointF(270, 240));
                m.DrawText($"击杀 {obj.kills}", font1, text, new PointF(20, 280));
                m.DrawText($"死亡 {obj.deaths}", font1, text, new PointF(270, 280));
                m.DrawText($"KD {obj.killDeath}", font1, text, new PointF(20, 320));
                m.DrawText($"技巧 {obj.skill}", font1, text, new PointF(270, 320));
                m.DrawText($"命中率 {obj.accuracy}", font1, text, new PointF(20, 360));
                m.DrawText($"爆头率 {obj.headshots}", font1, text, new PointF(270, 360));

                m.DrawLines(text, 1, new PointF(0, 405), new PointF(500, 405));
                m.DrawText($"个人战局", font1, text, new PointF(20, 410));
                m.DrawLines(text, 1, new PointF(0, 455), new PointF(500, 455));

                m.DrawText($"总分数 {obj.awardScore}", font1, text, new PointF(20, 460));
                m.DrawText($"获胜 {obj.wins}", font1, text, new PointF(20, 500));
                m.DrawText($"败局 {obj.loses}", font1, text, new PointF(270, 500));
                m.DrawText($"胜率 {obj.winPercent}", font1, text, new PointF(20, 540));

                m.DrawLines(text, 1, new PointF(0, 585), new PointF(500, 585));
                m.DrawText($"个人统计", font1, text, new PointF(20, 590));
                m.DrawLines(text, 1, new PointF(0, 635), new PointF(500, 635));

                m.DrawText($"游戏时间 {obj.timePlayed.Replace("days", "天")}", font1, text, new PointF(20, 640));
                m.DrawText($"最远爆头 {obj.longestHeadShot}", font1, text, new PointF(20, 680));
                m.DrawText($"复仇击杀 {obj.avengerKills}", font1, text, new PointF(270, 680));
                m.DrawText($"拯救击杀 {obj.headShots}", font1, text, new PointF(20, 720));
                m.DrawText($"狗牌获取 {obj.dogtagsTaken}", font1, text, new PointF(270, 720));
                m.DrawText($"最高连杀 {obj.highestKillStreak}", font1, text, new PointF(20, 760));
                m.DrawText($"急救数 {obj.revives}", font1, text, new PointF(270, 760));
                m.DrawText($"治疗数 {obj.heals}", font1, text, new PointF(20, 800));
                m.DrawText($"维修数 {obj.repairs}", font1, text, new PointF(270, 800));
            });

            string file = Local + $"{game.url}_{obj.userName}_state.png";
            image.SaveAsPng(file);
            BotMain.Log($"生成图片[{game.url}_{obj.userName}_state.png]");
            return file;
        }

        public static int GetStringLength(string text, int size)
        {
            int now = 70;
            if (now >= text.Length)
            {
                now = text.Length;
            }
            FontRectangle font;
            while (true)
            {
                font = TextMeasurer.Measure(text[..now], FontNormalOpt);
                if (font.Width > size)
                {
                    now--;
                }
                else
                {
                    return now;
                }
            }
        }

        public static async Task<Image> GenServer(BF1Server server1)
        {
            BF1Team one = server1.teams["teamOne"];
            BF1Team two = server1.teams["teamTwo"];
            Image<Rgba32> image = new(980, 630);
            var map = await HttpUtils.GetImage(server1.url);
            var team1 = await HttpUtils.GetImage(one.image);
            var team2 = await HttpUtils.GetImage(two.image);
            var img = Image.Load(map);
            var img1 = Image.Load(team1);
            var img2 = Image.Load(team2);
            img.Mutate(m => m.Resize(960, 0));
            img1 = Utils.ZoomImage(img1, 120, 120);
            img2 = Utils.ZoomImage(img2, 120, 120);
            image.Mutate(m =>
            {
                int a = 0;
                m.Clear(back);
                m.DrawImage(img, new Point(10, 10), 1);
                int now = 0;
                int all = server1.prefix.Length;
                string temp1;
                FontRectangle size;
                while (true)
                {
                    int temp = GetStringLength(server1.prefix[now..], 960);
                    size = TextMeasurer.Measure(server1.prefix[now..(temp + now)], FontNormalOpt);
                    m.Fill(back1, new RectangleF(10, 10 + a * 40, size.Width + 2, 40));
                    m.DrawText(server1.prefix[now..(temp + now)], font1, text, new PointF(10, 10 + a * 40));
                    now += temp;
                    a++;
                    if (now >= all)
                    {
                        break;
                    }
                }

                m.DrawImage(img1, new Point(310, 260), 1);
                m.DrawText($"{one.key}", font1, text, new PointF(340, 400));
                m.DrawImage(img2, new Point(550, 260), 1);
                m.DrawText($"{two.key}", font1, text, new PointF(580, 400));

                temp1 = $"在线人数 {server1.playerAmount}/{server1.maxPlayers} 排队中 {server1.inQue}";
                size = TextMeasurer.Measure(temp1, FontNormalOpt);
                m.Fill(back1, new RectangleF(10, 540, size.Width + 2, 40));
                m.DrawText(temp1, font1, text, new PointF(10, 540));
                temp1 = $"模式 {server1.mode} 地图 {server1.currentMap}";
                size = TextMeasurer.Measure(temp1, FontNormalOpt);
                m.Fill(back1, new RectangleF(10, 580, size.Width + 2, 40));
                m.DrawText(temp1, font1, text, new PointF(10, 580));
            });

            return image;
        }

        public static async Task<string> GenServers(BF1ServerObj obj, GameType game, string name)
        {
            BotMain.Log($"正在生成[{name}]的{game.url}服务器图片");
            Image<Rgba32> server = new(980, obj.servers.Count * 620 + 20);
            List<Image> servers = new();
            foreach (var server1 in obj.servers)
            {
                var item = await GenServer(server1);
                servers.Add(item);
            }

            int nowY = 0;

            server.Mutate(m =>
            {
                m.Clear(back);
            });

            foreach (var item in servers)
            {
                server.Mutate(m =>
                {
                    m.DrawImage(item, new Point(0, nowY), 1);
                });
                nowY += 620;
            }

            string file = Local + $"{game.url}_{name}_server.jpg";
            server.SaveAsJpeg(file, new JpegEncoder()
            {
                Quality = 95
            });
            BotMain.Log($"生成图片[{game.url}_{name}_server.jpg]");

            return file;
        }

        public static async Task<string> GenWeapons(BF1WeaponsObj obj, GameType game)
        {
            BotMain.Log($"正在生成[{obj.userName}]的{game.url}武器图片");

            int count = 0;
            var list = (from items in obj.weapons orderby items.kills descending select items).ToList();
            foreach (var item in list)
            {
                if (item.kills > 0)
                {
                    count++;
                }
            }
            if (count > 10)
                count = 10;
            else if (count == 0)
            {
                count = 1;
            }

            int heiall = 170;

            Image<Rgba32> image = new(500, 380 + 170 * count + 10);
            var head = await HttpUtils.GetImage(obj.avatar);
            var img = Image.Load(head);
            img = Utils.ZoomImage(img, 120, 120);

            long kills = 0;
            long count1 = 0;
            long headkills = 0;
            long shotsFireds = 0;
            long shotsHit = 0;
            float kpms = 0f;

            foreach (var item in list)
            {
                if (item.timeEquipped <= 0)
                    continue;
                kills += item.kills;
                headkills += item.headshotKills;
                shotsFireds += item.shotsFired;
                shotsHit += item.shotsHit;
                kpms += item.killsPerMinute;
                count1++;
            }

            float accuracys = (float)shotsHit / shotsFireds * 100;
            kpms /= count1;
            float headshots = (float)headkills / kills * 100;

            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText($"{game.name} 武器统计", title, text, new PointF(20, 0));
                m.DrawImage(img, new Point(20, 50), 1);
                m.DrawText($"ID {obj.userName}", font1, text, new PointF(150, 70));

                m.DrawLines(text, 1, new PointF(0, 185), new PointF(500, 185));
                m.DrawText($"全部统计", font1, text, new PointF(20, 190));
                m.DrawLines(text, 1, new PointF(0, 235), new PointF(500, 235));

                m.DrawText($"总击杀 {kills}", font1, text, new PointF(20, 240));
                m.DrawText($"平均KPM {kpms:0.00}", font1, text, new PointF(270, 240));
                m.DrawText($"命中率 {accuracys:0.00}%", font1, text, new PointF(20, 280));
                m.DrawText($"爆头率 {headshots:0.00}%", font1, text, new PointF(270, 280));

                m.DrawLines(text, 1, new PointF(0, 325), new PointF(500, 325));
                m.DrawText($"击杀前{count}排行", font1, text, new PointF(20, 330));
                m.DrawLines(text, 1, new PointF(0, 375), new PointF(500, 375));

                int nowY = 380;
                for (int index = 0; index < count; index++)
                {
                    var temp = list[index];
                    var waepon = HttpUtils.GetImage(temp.image).Result;
                    var img1 = Image.Load(waepon);

                    m.DrawImage(img1, new Point(10, nowY + 50 + (heiall * index)), 1);
                    m.DrawImage(star, new Point(10, nowY + 120 + (heiall * index)), 1);

                    long temp1 = temp.kills / 100;
                    if (temp1 > 100)
                    {
                        temp1 = 100;
                    }

                    m.DrawText($"{temp1}", font1, text, new PointF(45, nowY + 120 + (heiall * index)));

                    m.DrawText($"{temp.weaponName}", font1, text, new PointF(20, nowY + (heiall * index)));
                    m.DrawText($"击杀 {temp.kills}", font1, text, new PointF(270, nowY + 40 + (heiall * index)));
                    m.DrawText($"命中率 {temp.accuracy}", font1, text, new PointF(270, nowY + 80 + (heiall * index)));
                    m.DrawText($"KPM {temp.killsPerMinute}", font1, text, new PointF(110, nowY + 120 + (heiall * index)));
                    m.DrawText($"爆头率 {temp.headshots}", font1, text, new PointF(270, nowY + 120 + (heiall * index)));

                    m.DrawLines(text, 1, new PointF(0, nowY + 165 + (heiall * index)), new PointF(500, nowY + 165 + (heiall * index)));
                }
            });

            string file = Local + $"{game.url}_{obj.userName}_weapon.png";
            image.SaveAsPng(file);
            BotMain.Log($"生成图片[{game.url}_{obj.userName}_weapon.png]");

            return file;
        }

        public static async Task<string> GenVehicles(BF1VehiclesObj obj, GameType game) 
        {
            BotMain.Log($"正在生成[{obj.userName}]的{game.url}载具图片");

            int count = 0;
            var list = (from items in obj.vehicles orderby items.kills descending select items).ToList();
            foreach (var item in list)
            {
                if (item.kills > 0)
                {
                    count++;
                }
            }
            if (count > 10)
                count = 10;
            else if (count == 0)
            {
                count = 1;
            }

            int heiall = 170;

            Image<Rgba32> image = new(500, 380 + 170 * count + 10);
            var head = await HttpUtils.GetImage(obj.avatar);
            var img = Image.Load(head);
            img = Utils.ZoomImage(img, 120, 120);

            long kills = 0;
            long count1 = 0;
            long destroyeds = 0;
            float kpms = 0f;

            foreach (var item in list)
            {
                if (item.timeIn <= 0)
                    continue;
                kills += item.kills;
                destroyeds += item.destroyed;
                kpms += item.killsPerMinute;
                count1++;
            }

            kpms /= count1;

            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText($"{game.name} 载具统计", title, text, new PointF(20, 0));
                m.DrawImage(img, new Point(20, 50), 1);
                m.DrawText($"ID {obj.userName}", font1, text, new PointF(150, 70));

                m.DrawLines(text, 1, new PointF(0, 185), new PointF(500, 185));
                m.DrawText($"全部统计", font1, text, new PointF(20, 190));
                m.DrawLines(text, 1, new PointF(0, 235), new PointF(500, 235));

                m.DrawText($"总击杀 {kills}", font1, text, new PointF(20, 240)); 
                m.DrawText($"总摧毁 {destroyeds}", font1, text, new PointF(270, 240));
                m.DrawText($"平均KPM {kpms:0.00}", font1, text, new PointF(20, 280));

                m.DrawLines(text, 1, new PointF(0, 325), new PointF(500, 325));
                m.DrawText($"击杀前{count}排行", font1, text, new PointF(20, 330));
                m.DrawLines(text, 1, new PointF(0, 375), new PointF(500, 375));

                int nowY = 380;
                for (int index = 0; index < count; index++)
                {
                    var temp = list[index];
                    var waepon = HttpUtils.GetImage(temp.image).Result;
                    var img1 = Image.Load(waepon);

                    m.DrawImage(img1, new Point(10, nowY + 50 + (heiall * index)), 1);
                    m.DrawImage(star, new Point(10, nowY + 123 + (heiall * index)), 1);

                    m.DrawText($"{temp.kills / 100}", font1, text, new PointF(50, nowY + 120 + (heiall * index)));

                    m.DrawText($"{temp.vehicleName}", font1, text, new PointF(20, nowY + (heiall * index)));
                    m.DrawText($"击杀 {temp.kills}", font1, text, new PointF(270, nowY + 40 + (heiall * index)));
                    m.DrawText($"KPM {temp.killsPerMinute}", font1, text, new PointF(270, nowY + 80 + (heiall * index)));
                    m.DrawText($"摧毁数 {temp.destroyed}", font1, text, new PointF(270, nowY + 120 + (heiall * index)));

                    m.DrawLines(text, 1, new PointF(0, nowY + 165 + (heiall * index)), new PointF(500, nowY + 165 + (heiall * index)));
                }
            });

            string file = Local + $"{game.url}_{obj.userName}_vehicle.png";
            image.SaveAsPng(file);
            BotMain.Log($"生成图片[{game.url}_{obj.userName}_vehicle.png]");

            return file;
        }
    }
}
