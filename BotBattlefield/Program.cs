using System;
using System.Collections.Concurrent;
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

        private static ConcurrentDictionary<string, bool> queues = new();

        public static void SendMessageGroup(long group, List<string> message)
        {
            var temp = BuildPack.Build(new SendGroupMessagePack
            {
                id = group,
                message = message
            }, 52);
            robot.AddTask(temp);
        }
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
                        var Help = Config.Help;
                        var BF1Head = Config.BF1Head;
                        var BF1ServerHead = Config.BF1ServerHead;
                        var BF1WeaponHead = Config.BF1WeaponHead;
                        var BF1VehicleHead = Config.BF1VehicleHead;

                        if (Config.GroupHeads.ContainsKey(pack.id))
                        {
                            var item = Config.GroupHeads[pack.id];
                            Help = item.Help;
                            BF1Head = item.BF1Head;
                            BF1ServerHead = item.BF1ServerHead;
                            BF1WeaponHead = item.BF1WeaponHead;
                            BF1VehicleHead = item.BF1VehicleHead;
                        }

                        var message = pack.message[^1].Trim();
                        var temp1 = message.Split(' ');
                        if (temp1[0] == Help)
                        {
                            SendMessageGroup(pack.id, new List<string>() 
                            {
                                $"输入{BF1Head} [ID] (平台) 来生成BF1游戏统计\n",
                                $"输入{BF1ServerHead} [服务器名] 来生成BF1服务器信息\n",
                                $"输入{BF1WeaponHead} [ID] 来生成BF1武器统计\n",
                                $"输入{BF1VehicleHead} [ID] 来生成BF1载具统计"
                            });
                        }
                        else if (temp1[0] == BF1Head)
                        {
                            var arg = message.Substring(BF1Head.Length).Trim().Split(' ');
                            if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                            {
                                SendMessageGroup(pack.id, $"输入{BF1Head} [ID] (平台) 来生成BF1游戏统计");
                                break;
                            }
                            string name = arg[0];
                            string uname = $"bf1_{name}";
                            if (queues.ContainsKey(uname)) 
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"正在查询中"
                                });
                                break;
                            }
                            if (Delay.Check(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>() 
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"你的查询过于频繁"
                                });
                                break;
                            }
                            string pl = "pc";
                            if (arg.Length > 1)
                            {
                                pl = arg[1];
                            }
                            Task.Run(async () =>
                            {
                                try
                                {
                                    queues.TryAdd(uname, true);
                                    SendMessageGroup(pack.id, $"正在获取[{name}]的BF1游戏统计");
                                    var data = await HttpUtils.GetStats(GameType.BF1, name, pl);
                                    if (data == null)
                                    {
                                        SendMessageGroup(pack.id, $"获取[{name}]BF1游戏统计错误");
                                        return;
                                    }
                                    var local = await GenShow.GenState(data, GameType.BF1);
                                    SendMessageGroupImg(pack.id, local);
                                }
                                catch (Exception e)
                                {
                                    logs.LogError(e);
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1游戏统计错误");
                                }
                                finally
                                {
                                    Delay.AddDelay(uname);
                                    queues.TryRemove(uname, out var temp);
                                }
                            });
                        }
                        else if (temp1[0] == BF1ServerHead)
                        {
                            var name = message.Substring(BF1ServerHead.Length).Trim();
                            if (Config.ServerLock.ContainsKey(pack.id))
                            {
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    SendMessageGroup(pack.id, $"该群不能查询其他服务器");
                                    break;
                                }
                                name = Config.ServerLock[pack.id];
                            }
                            else if(string.IsNullOrWhiteSpace(name))
                            {
                                SendMessageGroup(pack.id, $"输入{BF1ServerHead} [服务器名] 来生成BF1服务器信息");
                                break;
                            }
                            string uname = $"bf1s_{name}";
                            if (queues.ContainsKey(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"正在查询中"
                                });
                                break;
                            }
                            if (Delay.Check(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"你的查询过于频繁"
                                });
                                break;
                            }
                            Task.Run(async () =>
                            {
                                try
                                {
                                    queues.TryAdd(uname, true);
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
                                finally
                                {
                                    Delay.AddDelay(uname);
                                    queues.TryRemove(uname, out var temp);
                                }
                            });
                        }
                        else if (temp1[0] == BF1WeaponHead)
                        {
                            var arg = message.Substring(BF1WeaponHead.Length).Trim().Split(' ');
                            if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                            {
                                SendMessageGroup(pack.id, $"输入{BF1WeaponHead} [ID] 来生成BF1武器统计");
                                break;
                            }
                            string name = arg[0];
                            string uname = $"bf1_{name}";
                            if (queues.ContainsKey(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"正在查询中"
                                });
                                break;
                            }
                            if (Delay.Check(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"你的查询过于频繁"
                                });
                                break;
                            }
                            Task.Run(async () =>
                            {
                                try
                                {
                                    queues.TryAdd(uname, true);
                                    SendMessageGroup(pack.id, $"正在获取[{name}]的BF1武器统计");
                                    var data = await HttpUtils.GetWeapons(GameType.BF1, name);
                                    if (data == null)
                                    {
                                        SendMessageGroup(pack.id, $"获取[{name}]BF1武器统计错误");
                                        return;
                                    }
                                    var local = await GenShow.GenWeapons(data, GameType.BF1);
                                    SendMessageGroupImg(pack.id, local);
                                }
                                catch (Exception e)
                                {
                                    logs.LogError(e);
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1武器统计错误");
                                }
                                finally
                                {
                                    Delay.AddDelay(uname);
                                    queues.TryRemove(uname, out var temp);
                                }
                            });
                        }
                        else if (temp1[0] == BF1VehicleHead)
                        {
                            var arg = message.Substring(BF1VehicleHead.Length).Trim().Split(' ');
                            if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                            {
                                SendMessageGroup(pack.id, $"输入{BF1VehicleHead} [ID] 来生成BF1载具统计");
                                break;
                            }
                            string name = arg[0];
                            string uname = $"bf1_{name}";
                            if (queues.ContainsKey(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"正在查询中"
                                });
                                break;
                            }
                            if (Delay.Check(uname))
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"你的查询过于频繁"
                                });
                                break;
                            }
                            Task.Run(async () =>
                            {
                                try
                                {
                                    queues.TryAdd(uname, true);
                                    SendMessageGroup(pack.id, $"正在获取[{name}]的BF1载具统计");
                                    var data = await HttpUtils.GetVehicles(GameType.BF1, name);
                                    if (data == null)
                                    {
                                        SendMessageGroup(pack.id, $"获取[{name}]BF1载具统计错误");
                                        return;
                                    }
                                    var local = await GenShow.GenVehicles(data, GameType.BF1);
                                    SendMessageGroupImg(pack.id, local);
                                }
                                catch (Exception e)
                                {
                                    logs.LogError(e);
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1载具统计错误");
                                }
                                finally
                                {
                                    Delay.AddDelay(uname);
                                    queues.TryRemove(uname, out var temp);
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

            HttpUtils.Init();
            GenShow.Init();

            Delay.Start();
            robot.Start();

            if (Environment.UserInteractive)
            {
                while (true) 
                {
                    var temp = Console.ReadLine();
                    var arg = temp.Split(' ');
                    if (arg[0] == "stop")
                    {
                        Close();
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
                        var data = await HttpUtils.GetStats(GameType.BF1, name, "pc");
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
                    else if (arg[0] == "weapon") 
                    {
                        if (arg.Length < 2)
                        {
                            Console.WriteLine("错误的参数");
                            continue;
                        }

                        var name = arg[1];
                        var data = await HttpUtils.GetWeapons(GameType.BF1, name);
                        if (data == null)
                        {
                            Console.WriteLine("获取错误");
                            continue;
                        }
                        await GenShow.GenWeapons(data, GameType.BF1);
                    }
                    else if (arg[0] == "vehicle")
                    {
                        if (arg.Length < 2)
                        {
                            Console.WriteLine("错误的参数");
                            continue;
                        }

                        var name = arg[1];
                        var data = await HttpUtils.GetVehicles(GameType.BF1, name);
                        if (data == null)
                        {
                            Console.WriteLine("获取错误");
                            continue;
                        }
                        await GenShow.GenVehicles(data, GameType.BF1);
                    }
                }
            }
        }

        public static void Close() 
        {
            robot.Stop();
            Delay.Stop();
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
                Proxy = new() 
                {
                    Enable = false,
                    IP = "127.0.0.1",
                    Port = 123
                },
                CheckDelay = 10,
                Help = "#bf",
                BF1Head = "#bf1",
                BF1ServerHead = "#bf1s",
                BF1WeaponHead = "#bf1w",
                BF1VehicleHead = "#bf1v",
                GroupHeads = new(),
                Groups = new(),
                ServerLock = new()
            }, Local + "config.json");
        }
    }
}
