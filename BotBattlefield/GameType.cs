using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotBattlefield
{
    public class GameType
    {
        public static readonly GameType BF2 = new GameType("bf2", "Battlefield 2");
        public static readonly GameType BF3 = new GameType("bf3", "Battlefield 3");
        public static readonly GameType BF4 = new GameType("bf4", "Battlefield 4");
        public static readonly GameType BF1 = new GameType("bf1", "Battlefield 1");
        public static readonly GameType BF5 = new GameType("bf5", "Battlefield 5");
        public static readonly GameType BF2042 = new GameType("bf2042", "Battlefield 2042");

        public string url;
        public string name;
        public GameType(string url, string name) 
        {
            this.url = url;
            this.name = name;
        }
    }
}
