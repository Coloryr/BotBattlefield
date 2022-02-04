using System.Collections.Generic;
using System.IO;

namespace BotBattlefield
{

    public record ConfigObj
    {
        public ConfigRobot robot { get; set; }
       
        public List<long> Groups { get; set; }

        public string Head { get; set; }
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
