﻿using System.Collections.Generic;
using System.IO;

namespace BotBattlefield
{
    public record ConfigObj
    {
        public ConfigRobot robot { get; set; }
       
        public List<long> Groups { get; set; }

        public string BF1Head { get; set; }
        public string BF1ServerHead { get; set; }
        public string BF1WeaponHead { get; set; }
        public string BF1VehicleHead { get; set; }
        public Dictionary<long, string> ServerLock { get; set; }
        public string Help { get; set; }
        public int CheckDelay { get; set; }
        public ConfigProxy Proxy { get; set; }
    }
    public record ConfigProxy
    { 
        public bool Enable { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }
    public record ConfigRobot
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public long QQ { get; set; }
        public bool Auto { get; set; }
        public int Time { get; set; }
    }
}
