using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotBattlefield;

class Utils
{
    public const string RankResourcesName = "BotBattlefield.Resources.Rank";
    public const string WeaponsResourcesName = "BotBattlefield.Resources.Weapons";
    public const string VehiclesResourcesName = "BotBattlefield.Resources.Vehicles";
    public const string MapsResourcesName = "BotBattlefield.Resources.Maps";
    public const string TeamsResourcesName = "BotBattlefield.Resources.Teams";

    public static Stream? GetRankImg(int rank) 
    {
        Assembly myAssem = Assembly.GetExecutingAssembly();

        if (rank >= 1 && rank <= 9)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.1-9.png");
        else if (rank >= 10 && rank <= 19)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.10-19.png");
        else if (rank >= 20 && rank <= 29)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.20-29.png");
        else if (rank >= 30 && rank <= 39)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.30-39.png");
        else if (rank >= 40 && rank <= 49)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.40-49.png");
        else if (rank >= 50 && rank <= 59)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.50-59.png");
        else if (rank >= 60 && rank <= 69)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.60-69.png");
        else if (rank >= 70 && rank <= 79)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.70-79.png");
        else if (rank >= 80 && rank <= 89)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.80-89.png");
        else if (rank >= 90 && rank <= 99)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.90-99.png");
        else if (rank >= 100 && rank <= 109)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.110-119.png");
        else if (rank >= 110 && rank <= 119)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.110-129.png");
        else if (rank >= 120 && rank <= 129)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.120-129.png");
        else if (rank >= 130 && rank <= 139)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.130-139.png");
        else if (rank >= 140 && rank <= 149)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.140-149.png");
        else if (rank == 150)
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.150.png");
        else
            return myAssem.GetManifestResourceStream($"{RankResourcesName}.0.png");
    }

    public static Stream GetWeaponImg(BF1Weapon weapon) 
    {
        string name = $"{weapon.type.Replace(" ", "_").Replace("/", "_").Replace("-", "_")}.{weapon.weaponName.Replace("/", "_")}.png";
        Assembly myAssem = Assembly.GetExecutingAssembly();

        return myAssem.GetManifestResourceStream($"{WeaponsResourcesName}.{name}");
    }

    public static Stream GetVehiclesImg(BF1Vehicle vehicle)
    {
        string name = $"{vehicle.type.Replace(" ", "_")}.{vehicle.vehicleName.Replace("/", "_")}.png";
        Assembly myAssem = Assembly.GetExecutingAssembly();

        return myAssem.GetManifestResourceStream($"{VehiclesResourcesName}.{name}");
    }

    public static Stream GetMapImg(string map)
    {
        string name = map.Replace(":", "_") + ".png";
        Assembly myAssem = Assembly.GetExecutingAssembly();

        return myAssem.GetManifestResourceStream($"{MapsResourcesName}.{name}");
    }

    public static Stream GetTeamImg(string team)
    {
        string name = team + ".png";
        Assembly myAssem = Assembly.GetExecutingAssembly();

        return myAssem.GetManifestResourceStream($"{TeamsResourcesName}.{name}");
    }

    /// <summary>
    /// 去掉字符串中的数字
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string RemoveNumber(string key)
    {
        return System.Text.RegularExpressions.Regex.Replace(key, @"\d", "");
    }

    public static string GetString(string a, string b, string c = null)
    {
        int x = a.IndexOf(b) + b.Length;
        int y;
        if (c != null)
        {
            y = a.IndexOf(c, x);
            if (a[y - 1] == '"')
            {
                y = a.IndexOf(c, y + 1);
            }
            if (y - x <= 0)
                return a;
            else
                return a.Substring(x, y - x);
        }
        else
            return a.Substring(x);
    }

    public static Image ZoomImage(Image bitmap, int destHeight, int destWidth)
    {
        try
        {
            int width = 0, height = 0;
            //按比例缩放             
            int sourWidth = bitmap.Width;
            int sourHeight = bitmap.Height;
            if (sourHeight > destHeight || sourWidth > destWidth)
            {
                if (sourWidth * destHeight > sourHeight * destWidth)
                {
                    width = destWidth;
                    height = destWidth * sourHeight / sourWidth;
                }
                else
                {
                    height = destHeight;
                    width = sourWidth * destHeight / sourHeight;
                }
            }
            else
            {
                width = sourWidth;
                height = sourHeight;
            }
            bitmap.Mutate((a) =>
            {
                a.Resize(width, height);
            });
            return bitmap;
        }
        catch
        {
            return bitmap;
        }
    }
}
