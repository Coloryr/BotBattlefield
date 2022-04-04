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

    public class WeaponType
    {
        public static readonly WeaponType Field_kit = new("特种", "Field kit"); 
        public static readonly WeaponType Lmg = new("机枪", "Lmg");
        public static readonly WeaponType Melee = new("近战", "Melee");
        public static readonly WeaponType Rifle = new("狙击枪", "Rifle");
        public static readonly WeaponType Gadget = new("装备", "Gadget");
        public static readonly WeaponType Self_loading_rifle = new("自动装填步枪", "Self-loading rifle");
        public static readonly WeaponType Grenade = new("手雷", "Grenade");
        public static readonly WeaponType Standard_issue_rifles = new("步枪", "Standard issue rifles");
        public static readonly WeaponType Shotgun = new("散弹枪", "Shotgun");
        public static readonly WeaponType Tanker_pilot = new ("驾驶员武器", "Tanker/pilot");
        public static readonly WeaponType Smg = new("冲锋枪", "Smg");
        public static readonly WeaponType Sidearm = new("手枪", "Sidearm");

        public static WeaponType GetType(string name) 
        {
            if (Field_kit.name == name)
                return Field_kit;
            else if (Lmg.name == name)
                return Lmg;
            else if (Melee.name == name)
                return Melee;
            else if (Rifle.name == name)
                return Rifle;
            else if (Gadget.name == name)
                return Gadget;
            else if (Self_loading_rifle.name == name)
                return Self_loading_rifle;
            else if (Grenade.name == name)
                return Grenade;
            else if (Standard_issue_rifles.name == name)
                return Standard_issue_rifles;
            else if (Shotgun.name == name)
                return Shotgun;
            else if (Tanker_pilot.name == name)
                return Tanker_pilot;
            else if (Smg.name == name)
                return Smg;
            else if (Sidearm.name == name)
                return Sidearm;

            return null;
        }

        public string name;
        public string typeName;

        public WeaponType(string name, string typeName)
        {
            this.name = name;
            this.typeName = typeName;
        }
    }
}
