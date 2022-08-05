using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotBattlefield;

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

public record BF1Team 
{
    public string image;
    public string key;
    public string name;
}

public record BF1Server
{
    public string prefix;
    public int playerAmount;
    public int maxPlayers;
    public int inQue;
    public string serverInfo;
    public string url;
    public string mode;
    public string currentMap;
    public string ownerId;
    public string region;
    public string platform;
    public string smallMode;
    public Dictionary<string, BF1Team> teams;
    public bool official;
    public string gameId;
}

public record BF1ServerObj
{
    public List<BF1Server> servers;
}

public class BF1Weapon
{
    public string weaponName;
    public string type;
    public string image;
    public long timeEquipped;
    public long kills;
    public float killsPerMinute;
    public long headshotKills;
    public string headshots;
    public long shotsFired;
    public long shotsHit;
    public string accuracy;
    public float hitVKills;
}

public record BF1WeaponsObj
{
    public string avatar;
    public string userName;
    public long id;
    public List<BF1Weapon> weapons;
}

public class BF1Vehicle
{
    public string vehicleName;
    public string type;
    public string image;
    public long kills;
    public float killsPerMinute;
    public long destroyed;
    public long timeIn;
}

public record BF1VehiclesObj
{
    public string avatar;
    public string userName;
    public long id;
    public List<BF1Vehicle> vehicles;
}

public record MatchesItem
{
    public string map { get; set; }
    public string type { get; set; }
    public string time { get; set; }
    public string servername { get; set; }
    public string playtime { get; set; }

    public string Score { get; set; }
    public string Kills { get; set; }
    public string Deaths { get; set; }
    public string Assists { get; set; }
    public string KD { get; set; }
}