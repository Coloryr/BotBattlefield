using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Drawing;
using System.Numerics;
using System.Reflection;

namespace BotBattlefield
{
    public class BF1GenShow
    {
        private static string Local;
        private static Font title;
        private static Font font;
        private static Font font1;
        private static Color back;
        private static Color text;
        private static Color back1;
        private static TextOptions FontNormalOpt;

        private static Color Team1 = Color.Parse("#1E88E5");
        private static Color Team2 = Color.Parse("#AB312D");

        private static Image star;

        static BF1GenShow() 
        {
            Assembly myAssem = Assembly.GetExecutingAssembly();
            using var stream = myAssem.GetManifestResourceStream($"BotBattlefield.Resources.star.png");
            star = Image.Load(stream);
        }

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
            Image? img = null;
            if (!string.IsNullOrWhiteSpace(obj.avatar))
            {
                var head = await HttpUtils.GetImage(obj.avatar);
                img = Image.Load(head);
                img = Utils.ZoomImage(img, 120, 120);
            }

            int rank = await HttpUtils.GetRank(game, obj.userName);
            using var stream = Utils.GetRankImg(rank);
            var img1 = Image.Load(stream);
            img1 = Utils.ZoomImage(img1, 50, 50);

            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText($"{game.name} 玩家统计", title, text, new PointF(20, 0));
                if (img != null) m.DrawImage(img, new Point(20, 50), 1);
                m.DrawText($"ID {obj.userName}", font1, text, new PointF(150, 70));
                m.DrawText($"等级 {rank}", font1, text, new PointF(210, 120));
                if (img1 != null) m.DrawImage(img1, new Point(150, 110), 1);

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
            img?.Dispose();
            img1.Dispose();
            image.Dispose();

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
            using var map = Utils.GetMapImg(server1.currentMap);
            using var team1 = Utils.GetTeamImg(one.name);
            using var team2 = Utils.GetTeamImg(two.name);
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
                m.DrawText($"{one.key}", font1, text, new PointF(342, 400));
                m.DrawImage(img2, new Point(550, 260), 1);
                m.DrawText($"{two.key}", font1, text, new PointF(578, 400));

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

        public static async Task<string> GenWeapons(BF1WeaponsObj obj, GameType game, WeaponType wtype)
        {
            BotMain.Log($"正在生成[{obj.userName}]的{game.url}武器图片");

            List<BF1Weapon> wlist = new();

            if (wtype != null)
            {
                wlist.AddRange(from items in obj.weapons where items.type == wtype.typeName select items);
            }
            else
                wlist.AddRange(obj.weapons);

            int count = 0;
            var list = (from items in wlist orderby items.kills descending select items).ToList();
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
                    using var waepon = Utils.GetWeaponImg(temp);
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
                    using var waepon = Utils.GetVehiclesImg(temp);
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

        //public static async Task<string> GenReson()
        //{ 
            
        //}

        public static async Task<string> GenScore(ScoreInfoObj obj)
        {
            BotMain.Log($"正在生成服务器计分板");

            var head = await HttpUtils.GetImage(obj.Info.TeamOneUrl);
            var img1 = Image.Load(head);
            img1 = Utils.ZoomImage(img1, 40, 40);

            head = await HttpUtils.GetImage(obj.Info.TeamTwoUrl);
            var img2 = Image.Load(head);
            img2 = Utils.ZoomImage(img2, 40, 40);

            var sb = new PathBuilder();
            sb.AddLine(new Vector2(420, 80), new Vector2(820, 80));
            sb.AddLine(new Vector2(820, y: 120), new Vector2(420, 120));
            sb.AddLine( new Vector2(420, 120), new Vector2(420, y: 80));
            var path1 = sb.Build();

            sb = new PathBuilder();
            sb.AddLine(new Vector2(1580, 80), new Vector2(1180, 80));
            sb.AddLine(new Vector2(1180, y: 120), new Vector2(1580, 120));
            sb.AddLine(new Vector2(1580, 120), new Vector2(1580, y: 80));
            var path2 = sb.Build();

            float a = (float)obj.Info.Team1Score / 1000 * 400;

            sb = new PathBuilder();
            sb.AddLine(new Vector2(820 - a, 80), new Vector2(820, 80));
            sb.AddLine(new Vector2(820, y: 120), new Vector2(820 - a, 120));
            sb.AddLine(new Vector2(820 - a, 120), new Vector2(820 - a, y: 80));
            var path3 = sb.Build();

            a = (float)obj.Info.Team2Score / 1000 * 400;

            sb = new PathBuilder();
            sb.AddLine(new Vector2(1180 + a, 80), new Vector2(1180, 80));
            sb.AddLine(new Vector2(1180, y: 120), new Vector2(1180 + a, 120));
            sb.AddLine(new Vector2(1180 + a, 120), new Vector2(1180 + a, y: 80));
            var path4 = sb.Build();

            obj.Team1.Sort((a, b) => a.Score > b.Score ? -1 : 1);
            obj.Team2.Sort((a, b) => a.Score > b.Score ? -1 : 1);

            Image<Rgba32> image = new(2000, 1490);
            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText(new TextOptions(font1) 
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(1000,0)
                }, obj.Info.ServerName, text);
                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(1000, 40)
                }, $"地图：{obj.Info.MapName} 模式：{obj.Info.Mode}", text);
                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(1000, 80)
                }, obj.Info.ServerTimeS, text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(920, 80)
                }, obj.Info.TeamOne, text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1080, 80)
                }, obj.Info.TeamTwo, text);

                m.DrawImage(img1, new Point(840, 80), 1f);
                m.DrawImage(img2, new Point(1120, 80), 1f);

                m.Draw(Team1, 1.5f, path1);
                m.Draw(Team2, 1.5f, path2);

                m.Fill(Team1, path3);
                m.Fill(Team2, path4);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Origin = new(x: 810, 80)
                }, $"{obj.Info.Team1Score}", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Origin = new(x: 1190, 80)
                }, $"{obj.Info.Team2Score}", text);

                m.DrawLines(text, 1.0f, new PointF(0, 130), new PointF(2000, 130));
                m.DrawLines(text, 1.5f, new PointF(999, 130), new PointF(999, 1490));

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 90, 140)
                }, "等级", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 180, 140)
                }, text: "名字", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 540, 140)
                }, text: "击杀", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 620, 140)
                }, text: "死亡", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 720, 140)
                }, text: "KD", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 790, 140)
                }, text: "KPM", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Origin = new(x: 980, 140)
                }, text: "分数", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1090, 140)
                }, "等级", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1180, 140)
                }, text: "名字", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1540, 140)
                }, text: "击杀", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1620, 140)
                }, text: "死亡", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1720, 140)
                }, text: "KD", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Origin = new(x: 1790, 140)
                }, text: "KPM", text);

                m.DrawText(new TextOptions(font1)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Origin = new(x: 1980, 140)
                }, text: "分数", text);

                m.DrawLines(text, 1.0f, new PointF(0, 190), new PointF(2000, 190));

                for (int a = 0; a < obj.Team1.Count; a++)
                {
                    var player = obj.Team1[a];
                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(50, 200 + a * 40)
                    }, $"{a + 1}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Origin = new(90, 200 + a * 40)
                    }, $"{player.Rank}", text);

                    string name = (string.IsNullOrWhiteSpace(player.Clan) ? "" : $"[{player.Clan}]") + player.Name;

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Origin = new(130, 200 + a * 40)
                    }, name, text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(560, 200 + a * 40)
                    }, $"{player.Kill}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(640, 200 + a * 40)
                    }, $"{player.Dead}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(740, 200 + a * 40)
                    }, $"{player.KD:0.00}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(820, 200 + a * 40)
                    }, $"{player.KPM:0.00}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(980, 200 + a * 40)
                    }, $"{player.Score}", text);
                }

                for (int a = 0; a < obj.Team2.Count; a++)
                {
                    var player = obj.Team2[a];
                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1050, 200 + a * 40)
                    }, $"{a + 1}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Origin = new(1090, 200 + a * 40)
                    }, $"{player.Rank}", text);

                    string name = (string.IsNullOrWhiteSpace(player.Clan) ? "" : $"[{player.Clan}]") + player.Name;

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Origin = new(1130, 200 + a * 40)
                    }, name, text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1560, 200 + a * 40)
                    }, $"{player.Kill}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1640, 200 + a * 40)
                    }, $"{player.Dead}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1740, 200 + a * 40)
                    }, $"{player.KD:0.00}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1820, 200 + a * 40)
                    }, $"{player.KPM:0.00}", text);

                    m.DrawText(new TextOptions(font1)
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Origin = new(1980, 200 + a * 40)
                    }, $"{player.Score}", text);
                }
            });

            string file = Local + $"server_score.png";
            image.SaveAsPng(file);
            BotMain.Log($"生成图片[server_score.png]");
            return file;
        }

        public static async Task<string> GenMatches(List<MatchesItem> list, BF1StateObj obj, GameType game)
        {

            BotMain.Log($"正在生成[{obj.userName}]的{game.url}最近图片");
            Image<Rgba32> image = new(500, 185 + 250 * list.Count + 10);
            Image? img = null;
            if (!string.IsNullOrWhiteSpace(obj.avatar))
            {
                var head = await HttpUtils.GetImage(obj.avatar);
                img = Image.Load(head);
                img = Utils.ZoomImage(img, 120, 120);
            }

            int rank = await HttpUtils.GetRank(game, obj.userName);
            using var stream = Utils.GetRankImg(rank);
            var img1 = Image.Load(stream);
            img1 = Utils.ZoomImage(img1, 50, 50);

            image.Mutate(m =>
            {
                m.Clear(back);
                m.DrawText($"{game.name} 最近数据", title, text, new PointF(20, 0));
                if (img != null) m.DrawImage(img, new Point(20, 50), 1);
                m.DrawText($"ID {obj.userName}", font1, text, new PointF(150, 70));
                m.DrawText($"等级 {rank}", font1, text, new PointF(210, 120));
                if (img1 != null) m.DrawImage(img1, new Point(150, 110), 1);

                m.DrawLines(text, 1, new PointF(0, 185), new PointF(500, 185));

                int b = 0;

                int y = 190;

                foreach (var item in list)
                {
                    int a = 0;
                    int now = 0;
                    int all = item.servername.Length;
                    string temp1;
                    FontRectangle size;
                    while (true)
                    {
                        int temp = GetStringLength(item.servername[now..], 480);
                        size = TextMeasurer.Measure(item.servername[now..(temp + now)], FontNormalOpt);
                        m.Fill(back1, new RectangleF(10, y + a * 40, size.Width + 2, 40));
                        m.DrawText(item.servername[now..(temp + now)], font1, text, new PointF(10, y + a * 40));
                        now += temp;
                        a++;
                        if (a == 2)
                            break;
                        if (now >= all)
                        {
                            break;
                        }
                    }

                    m.DrawText($"模式：{item.type}", font1, text, new PointF(10, y + 80));

                    m.DrawText($"地图：{item.map}", font1, text, new PointF(10, y + 120));

                    m.DrawText($"时间：{item.time}", font1, text, new PointF(10, y + 160));

                    m.DrawText($"得分：{item.Score} 击杀：{item.Kills} 死亡：{item.Deaths}", font1, text, new PointF(10, y + 200));

                    m.DrawLines(text, 1, new PointF(0, y + 245), new PointF(500, y + 245));

                    y += 250;
                }
            });

            string file = Local + $"{game.url}_{obj.userName}_matches.png";
            image.SaveAsPng(file);
            BotMain.Log($"生成图片[{game.url}_{obj.userName}_matches.png]");
            img?.Dispose();
            img1.Dispose();
            image.Dispose();

            return null;
        }
    }
}
