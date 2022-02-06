using System.Collections.Concurrent;

namespace BotBattlefield
{
    public class Delay
    {
        private static readonly ConcurrentDictionary<string, int> players = new();

        private static Timer timer;

        private static void Run(object? state)
        {
            List<string> remove = new();
            foreach (var item in players)
            {
                players.TryUpdate(item.Key, item.Value-1, item.Value);
                if (players[item.Key] <= 0)
                {
                    remove.Add(item.Key);
                }
            }

            foreach (var item in remove)
            {
                players.TryRemove(item, out var temp);
            }
        }
        public static void Start()
        {
            timer = new(Run, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        public static void Stop() 
        {
            timer.Dispose();
        }
        public static bool Check(string name)
        {
            return players.ContainsKey(name);
        }
        public static void AddDelay(string name) 
        {
            players.TryAdd(name, BotMain.Config.CheckDelay);
        }
    }
}
