using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotBattlefield
{
    public record BF1StateObj
    {
        public string avatar;
        public string userName;
        public long id;
        public int rank;
        public string rankImg;
        public string rankName;
        public float skill;
        public float scorePerMinute;
        public float killsPerMinute;
        public string winPercent;
        public string bestClass;
        public string accuracy;
        public string headshots;
        public string timePlayed;
        public long secondsPlayed;
        public float killDeath;
        public float infantryKillDeath;
        public float infantryKillsPerMinute;
        public long kills;
        public long deaths;
        public long wins;
        public long loses;
        public float longestHeadShot;
        public long revives;
        public long dogtagsTaken;
        public long highestKillStreak;
        public long roundsPlayed;
        public long awardScore;
        public long bonusScore;
        public long squadScore;
        public long currentRankProgress;
        public long totalRankProgress;
        public long avengerKills;
        public long saviorKills;
        public long headShots;
        public long heals;
        public long repairs;
        public long killAssists;
    }
}
