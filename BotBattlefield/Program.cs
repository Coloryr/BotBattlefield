using System;
using ColoryrSDK;
using Newtonsoft.Json;

namespace BotBattlefield
{
    public class BotMain
    {
        public static string Local { get; private set; }
        public static ConfigObj Config { get; private set; }

        private static Robot robot = new Robot();
        private static Logs logs;

        public static void SendMessageGroup(long group, string message)
        {
            var temp = BuildPack.Build(new SendGroupMessagePack
            {
                id = group,
                message = new()
                {
                    message
                }
            }, 52);
            robot.AddTask(temp);
        }

        public static void SendMessageGroupImg(long group, string local)
        {
            var temp = BuildPack.Build(new SendGroupImageFilePack
            {
                id = group,
                file = local
            }, 75);
            robot.AddTask(temp);
        }

        private static void Message(byte type, object data)
        {
            switch (type)
            {
                case 49:
                    var pack = data as GroupMessageEventPack;
                    if (Config.Groups.Contains(pack.id))
                    {
                        var message = pack.message[^1].Split(' ');
                        if (message[0] == Config.BF1Head)
                        {
                            if (message.Length == 1)
                            {
                                SendMessageGroup(pack.id, $"输入{Config.BF1Head} [ID] 来生成BF1游戏统计");
                                break;
                            }
                            var name = message[1];
                            Task.Run(async () =>
                            {
                                try
                                {
                                    SendMessageGroup(pack.id, $"正在获取[{name}]的BF1游戏统计");
                                    var data = await HttpUtils.GetStats(GameType.BF1, name);
                                    if (data == null)
                                    {
                                        SendMessageGroup(pack.id, $"获取[{name}]BF1游戏统计错误");
                                        return;
                                    }
                                    var local = await GenShow.GenState(data, GameType.BF1);
                                    SendMessageGroupImg(pack.id, local);
                                }
                                catch(Exception e)
                                {
                                    logs.LogError(e);
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1游戏统计错误");
                                }
                            });
                        }
                        else if (message[0] == Config.BF1ServerHead)
                        {
                            string name;
                            if (Config.ServerLock.ContainsKey(pack.id))
                            {
                                if (message.Length > 1)
                                {
                                    SendMessageGroup(pack.id, $"该群不能查询其他服务器");
                                    break;
                                }
                                name = Config.ServerLock[pack.id];
                            }
                            else if (message.Length == 1)
                            {
                                SendMessageGroup(pack.id, $"输入{Config.BF1Head} [服务器名] 来生成BF1服务器信息");
                                break;
                            }
                            else
                            {
                                name = message[1];
                            }
                            Task.Run(async () =>
                            {
                                try
                                {
                                    SendMessageGroup(pack.id, $"正在获取[{name}]BF1服务器信息");
                                    var data = await HttpUtils.GetServers(GameType.BF1, name);
                                    if (data == null)
                                    {
                                        SendMessageGroup(pack.id, $"获取[{name}]BF1服务器信息错误");
                                        return;
                                    }
                                    if (data.servers.Count == 0)
                                    {
                                        SendMessageGroup(pack.id, $"搜索不到BF1服务器[{name}]");
                                        return;
                                    }
                                    var local = await GenShow.GenServers(data, GameType.BF1, name);
                                    SendMessageGroupImg(pack.id, local);
                                }
                                catch (Exception e)
                                {
                                    logs.LogError(e);
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1服务器信息错误");
                                }
                            });
                        }
                    }
                    break;
            }
        }

        private static void Log(LogType type, string data)
            => Log($"机器人状态:{type} {data}");

        private static void State(StateType type)
            => Log($"机器人状态:{type}");

        public static async Task Main()
        {
            Local = AppContext.BaseDirectory;
            logs = new()
            {
                Dir = Local
            };
            Reload();

            var config = new RobotConfig
            {
                IP = Config.robot.IP,
                Port = Config.robot.Port,
                Check = Config.robot.Auto,
                Name = "BotBattlefield",
                Pack = new() { 49 },
                RunQQ = Config.robot.QQ,
                Time = Config.robot.Time,
                CallAction = Message,
                LogAction = Log,
                StateAction = State
            };
            robot.Set(config);

            GenShow.Init();

            robot.Start();

            if (Environment.UserInteractive)
            {
                while (true) 
                {
                    var temp = Console.ReadLine();
                    var arg = temp.Split(' ');
                    if (arg[0] == "stop")
                    {
                        robot.Stop();
                        return;
                    }
                    else if (arg[0] == "state")
                    {
                        if (arg.Length < 2)
                        {
                            Console.WriteLine("错误的参数");
                            continue;
                        }
                       
                        var name = arg[1];
                        var data = await HttpUtils.GetStats(GameType.BF1, name);
                        if (data == null)
                        {
                            Console.WriteLine("获取错误");
                            continue;
                        }
                        await GenShow.GenState(data, GameType.BF1);
                    }
                    else if (arg[0] == "server")
                    {
                        if (arg.Length < 2)
                        {
                            Console.WriteLine("错误的参数");
                            continue;
                        }

                        var name = arg[1];
                        var data = await HttpUtils.GetServers(GameType.BF1, name);
                        if (data == null)
                        {
                            Console.WriteLine("获取错误");
                            continue;
                        }
                        if (data.servers.Count == 0)
                        {
                            Console.WriteLine("搜索不到服务器");
                            continue;
                        }
                        await GenShow.GenServers(data, GameType.BF1, name);
                    }
                }
            }
        }

        public static void Log(string data)
        {
            logs.LogWrite(data);
        }

        public static void Reload()
        {
            Config = ConfigUtils.Config(new ConfigObj()
            {
                robot = new()
                {
                    IP = "127.0.0.1",
                    Port = 23333,
                    Auto = true,
                    QQ = 0,
                    Time = 10
                },
                BF1Head = "#bf1",
                BF1ServerHead = "#bf1s",
                Groups = new List<long>(),
                ServerLock = new Dictionary<long, string>()
            }, Local + "config.json");
        }
    }
}
