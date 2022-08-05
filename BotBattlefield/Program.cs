using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Resources;
using ColoryrSDK;
using Newtonsoft.Json;

namespace BotBattlefield;

public class BotMain
{
    public const string Version = "1.3.0";
    public static string Local { get; private set; }
    public static ConfigObj Config { get; private set; }

    public static RobotSDK robot = new();
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
                    var BF1MatchesHead = Config.BF1MatchesHead;

                    if (Config.GroupHeads.ContainsKey(pack.id))
                    {
                        var item = Config.GroupHeads[pack.id];
                        Help = item.Help;
                        BF1Head = item.BF1Head;
                        BF1ServerHead = item.BF1ServerHead;
                        BF1WeaponHead = item.BF1WeaponHead;
                        BF1VehicleHead = item.BF1VehicleHead;
                        BF1MatchesHead = item.BF1MatchesHead;
                    }

                    var message = pack.message[^1].Trim();
                    var temp1 = message.Split(' ');
                    if (temp1[0] == Help)
                    {
                        SendMessageGroup(pack.id, new List<string>()
                        {
                            $"输入{BF1Head} [ID] (平台) 来生成BF1游戏统计\n",
                            "平台：pc ps4 xboxone\n",
                            $"输入{BF1ServerHead} [服务器名] 来生成BF1服务器信息\n",
                            $"输入{BF1WeaponHead} [ID] (类别) 来生成BF1武器统计\n",
                            "类别：特种 机枪 近战 狙击枪 装备 半自动 手雷 步枪 霰弹枪 驾驶员武器 冲锋枪 手枪\n",
                            $"输入{BF1VehicleHead} [ID] 来生成BF1载具统计\n",
                            $"输入{BF1MatchesHead} [ID] 来获取BF1最近数据"
                        });
                    }
                    else if (temp1[0] == BF1Head)
                    {
                        var arg = message.Substring(BF1Head.Length).Trim().Split(' ');
                        if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                        {
                            SendMessageGroup(pack.id, $"输入{BF1Head} [ID] (平台) 来生成BF1游戏统计，平台：pc ps4 xboxone");
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
                                var local = await BF1GenShow.GenState(data, GameType.BF1);
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
                        else if (string.IsNullOrWhiteSpace(name))
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
                                var local = await BF1GenShow.GenServers(data, GameType.BF1, name);
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
                            SendMessageGroup(pack.id, $"输入{BF1WeaponHead} [ID] (类别) 来生成BF1武器统计，类别：特种 机枪 近战 狙击枪 装备 半自动 手雷 步枪 霰弹枪 驾驶员武器 冲锋枪 手枪");
                            break;
                        }
                        string name = arg[0];
                        WeaponType wtype = null;
                        if (arg.Length > 1)
                        {
                            wtype = WeaponType.GetType(arg[1]);
                            if (wtype == null)
                            {
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"[mirai:at:{pack.fid}]",
                                    $"没有找到武器：{arg[1]}"
                                });
                                break;
                            }
                        }
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
                                SendMessageGroup(pack.id, $"正在获取[{name}]的BF1{(wtype == null ? "武器" : wtype.name)}统计");
                                var data = await HttpUtils.GetWeapons(GameType.BF1, name);
                                if (data == null)
                                {
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1{(wtype == null ? "武器" : wtype.name)}统计错误");
                                    return;
                                }
                                var local = await BF1GenShow.GenWeapons(data, GameType.BF1, wtype);
                                SendMessageGroupImg(pack.id, local);
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"获取[{name}]BF1武器{(wtype == null ? "武器" : wtype.name)}错误");
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
                                var local = await BF1GenShow.GenVehicles(data, GameType.BF1);
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
                    else if (temp1[0] == BF1MatchesHead)
                    {
                        var name = message.Substring(BF1MatchesHead.Length).Trim();
                        if (Config.ServerLock.ContainsKey(pack.id))
                        {
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                SendMessageGroup(pack.id, $"该群不能查询其他服务器");
                                break;
                            }
                            name = Config.ServerLock[pack.id];
                        }
                        else if (string.IsNullOrWhiteSpace(name))
                        {
                            SendMessageGroup(pack.id, $"输入{BF1MatchesHead} [ID] 来获取BF1最近数据");
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
                                SendMessageGroup(pack.id, $"正在获取[{name}]的BF1最近数据");

                                var data = await HttpUtils.GetMatches(GameType.BF1, name);
                                if (data == null)
                                {
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1最近数据错误");
                                    return;
                                }

                                var data1 = await HttpUtils.GetStats(GameType.BF1, name, "pc");
                                if (data1 == null)
                                {
                                    SendMessageGroup(pack.id, $"获取[{name}]BF1最近数据错误");
                                    return;
                                }
                                var local = await BF1GenShow.GenMatches(data, data1, GameType.BF1);
                                SendMessageGroupImg(pack.id, local);
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"获取[{name}]BF1最近数据");
                            }
                            finally
                            {
                                Delay.AddDelay(uname);
                                queues.TryRemove(uname, out var temp);
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.State)
                    {
                        if (!Config.Netty.Admins.Contains(pack.fid))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"你没有权限使用这条指令",
                            });
                            break;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                var res = await NettyClient.CheckState();
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"获取服管工具状态失败",
                                    });
                                }
                                else
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"游戏启动状态：{res.IsGameRun}\n",
                                        $"工具初始化状态：{res.IsGameRun}",
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"获取服管工具状态错误");
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.Image)
                    {
                        if (!Config.Netty.Admins.Contains(pack.fid))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"你没有权限使用这条指令",
                            });
                            break;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                var res = await NettyClient.GetImage();
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"获取游戏截图失败",
                                    });
                                }
                                else
                                {
                                    robot.SendGroupImageFile(0, pack.id, res);
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"获取服管工具状态错误");
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.Score)
                    {
                        if (queues.ContainsKey("Netty_Gen_Socre!"))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"正在生成中",
                            });
                            return;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                queues.TryAdd("Netty_Gen_Socre!", true);
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"正在生成服务器计分板",
                                });
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                var res = await NettyClient.GetServerScore();
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服务器计分板获取失败",
                                    });
                                }
                                else
                                {
                                    var local = await BF1GenShow.GenScore(res);
                                    if (local == null)
                                    {
                                        SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"生成服务器计分板失败",
                                    });
                                        return;
                                    }
                                    SendMessageGroupImg(pack.id, local);
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"生成服务器计分板错误");
                            }
                            finally
                            {
                                queues.TryRemove("Netty_Gen_Socre!", out var temp);
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.Maps)
                    {
                        if (!Config.Netty.Admins.Contains(pack.fid))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"你没有权限使用这条指令",
                            });
                            break;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                var res = await NettyClient.GetServerMap();
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"获取地图列表失败",
                                    });
                                }
                                else
                                {
                                    var list1 = new List<string>()
                                    {
                                        $"服务器地图列表：\n"
                                    };
                                    foreach (var item in res.Maps)
                                    {
                                        list1.Add($"{item.Key}:{item.Value}\n");
                                    }
                                    list1.Add($"输入{Config.Netty.Switch} [编号] 来切换地图");
                                    SendMessageGroup(pack.id, list1);
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"获取地图列表错误");
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.Maps)
                    {
                        if (!Config.Netty.Admins.Contains(pack.fid))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"你没有权限使用这条指令",
                            });
                            break;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                var arg = message.Substring(Config.Netty.Maps.Length).Trim().Split(' ');
                                if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                                {
                                    SendMessageGroup(pack.id, $"输入{Config.Netty.Switch} [编号] 来切换地图");
                                    return;
                                }
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"正在切换地图{arg[0]}",
                                });
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                if (!int.TryParse(arg[0], out var index))
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"错误的编号",
                                    });
                                    return;
                                }
                                var res = await NettyClient.GetServerMap();
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"切换地图失败",
                                    });
                                    return;
                                }
                                if (!res.Maps.ContainsKey(index))
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"不存在编号",
                                    });
                                    return;
                                }
                                var res1 = await NettyClient.SwitchMap(index);
                                if (res1 == null || res1 == false)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"切换地图失败",
                                    });
                                }
                                else
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"已切换",
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"切换地图错误");
                            }
                        });
                    }
                    else if (temp1[0] == Config.Netty.Kick)
                    {
                        if (!Config.Netty.Admins.Contains(pack.fid))
                        {
                            SendMessageGroup(pack.id, new List<string>()
                            {
                                $"你没有权限使用这条指令",
                            });
                            break;
                        }
                        Task.Run(async () =>
                        {
                            try
                            {
                                var arg = message.Substring(Config.Netty.Kick.Length).Trim().Split(' ');
                                if (arg.Length == 1 && string.IsNullOrWhiteSpace(arg[0]))
                                {
                                    SendMessageGroup(pack.id, $"输入{Config.Netty.Kick} [玩家] (理由) 来踢出玩家");
                                    return;
                                }
                                SendMessageGroup(pack.id, new List<string>()
                                {
                                    $"正在踢出玩家{arg[0]}",
                                });
                                if (!NettyClient.IsConnect)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"服管工具未连接，正在尝试连接",
                                    });
                                    Console.WriteLine("");
                                    await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                                }
                                var res = await NettyClient.KickPlayer(arg[0], arg.Length > 1 ? arg[1] : "Admin Kick");
                                if (res == null)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"踢出玩家失败",
                                    });
                                }
                                else if (!res.HavePlayer)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"不存在玩家{arg[0]}",
                                    });
                                }
                                else if (!res.Success)
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"踢出玩家失败",
                                    });
                                }
                                else
                                {
                                    SendMessageGroup(pack.id, new List<string>()
                                    {
                                        $"已踢出玩家",
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                logs.LogError(e);
                                SendMessageGroup(pack.id, $"踢出玩家错误");
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
        Console.WriteLine($"正在启动{Version}");
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
        BF1GenShow.Init();

        Delay.Start();
        robot.Start();

        NettyClient.Key = Config.Netty.Key;

        Assembly myAssem = Assembly.GetExecutingAssembly();
        var list = myAssem.GetManifestResourceNames();

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
                    await BF1GenShow.GenState(data, GameType.BF1);
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
                    await BF1GenShow.GenServers(data, GameType.BF1, name);
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
                    await BF1GenShow.GenWeapons(data, GameType.BF1, null);
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
                    await BF1GenShow.GenVehicles(data, GameType.BF1);
                }
                else if (arg[0] == "tools")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.GetState();
                        Console.WriteLine(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "tools1")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.CheckState();
                        Console.WriteLine(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "tools2")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.GetServerInfo();
                        Console.WriteLine(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "tools3")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.GetServerScore();
                        Console.WriteLine(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "tools4")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.GetServerMap();
                        Console.WriteLine(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "score")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.GetServerScore();
                        if (res == null)
                        {
                            Console.WriteLine("生成错误");
                            break;
                        }
                        await BF1GenShow.GenScore(res);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "kick")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.KickPlayer(arg[1], arg[2]);
                        if (res == null)
                        {
                            Console.WriteLine("错误");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "switch")
                {
                    try
                    {
                        if (!NettyClient.IsConnect)
                        {
                            Console.WriteLine("服管工具未连接，正在尝试连接");
                            await NettyClient.Start(Config.Netty.IP, Config.Netty.Port);
                        }

                        var res = await NettyClient.SwitchMap(int.Parse(arg[1]));
                        if (res == null)
                        {
                            Console.WriteLine("错误");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (arg[0] == "matches")
                {
                    if (arg.Length < 2)
                    {
                        Console.WriteLine("错误的参数");
                        continue;
                    }

                    var name = arg[1];
                    var data = await HttpUtils.GetMatches(GameType.BF1, name);
                    if (data == null)
                    {
                        Console.WriteLine("获取错误");
                        continue;
                    }

                    var data1 = await HttpUtils.GetStats(GameType.BF1, name, "pc");
                    if (data1 == null)
                    {
                        Console.WriteLine("获取错误");
                        continue;
                    }
                    await BF1GenShow.GenMatches(data, data1, GameType.BF1);
                }
            }
        }
    }

    public static void Close()
    {
        robot.Stop();
        Delay.Stop();
        HttpUtils.Stop();
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
            BF1MatchesHead = "#bf1m",
            GroupHeads = new(),
            Groups = new(),
            LogGroups = new(),
            ServerLock = new(),
            Netty = new()
            {
                IP = "127.0.0.1",
                Port = 23232,
                Key = 0,
                Score = "#score",
                Kick = "#kick",
                Maps = "#maps",
                Switch = "#switch",
                State = "#state",
                Image = "#image",
                Admins = new()
            }
        }, Local + "config.json");
    }
}
