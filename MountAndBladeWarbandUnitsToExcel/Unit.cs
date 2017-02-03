using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MountAndBladeWarbandUnitsToExcel
{
    class Unit
    {
        public int ID { get; private set; }
        public string TroopID { get; private set; }
        public string EngSingleName { get; private set; }
        public string EngMultiplyName { get; private set; }
        public string LocSingleName { get; set; }
        public string LocMultiplyName { get; set; }
        public bool isLocalized { get { return LocSingleName != null && LocMultiplyName != null; } }
        public byte Level { get; private set; }
        public int FactionID { get; private set; }
        public Gender Sex
        {
            get
            {
                var skin = Flags[Flag.Skin];
                if (skin > 1) return Gender.Other;
                return (Gender)skin;
            }
        }
        public int[] UpgradePath { get; private set; } = new int[2];
        public int[] ItemsIDs { get; private set; }
        public Dictionary<Skill, byte> Skills { get; private set; } = new Dictionary<Skill, byte>();
        public Dictionary<Proficiency, byte> Proficiencies { get; private set; } = new Dictionary<Proficiency, byte>();
        public Dictionary<Attribute, byte> Attributes { get; private set; } = new Dictionary<Attribute, byte>();
        public Dictionary<Flag, byte> Flags { get; private set; } = new Dictionary<Flag, byte>();

        public enum Skill
        {
            //First number
            Trade, //1
            Leadership, //16
            PrisonerManagement, //256
            Persuasion, //268 435 456

            //Second number
            Engineer, //1
            FirstAid, //16
            Surgery, //256
            WoundTreatment, //4 096
            InventoryManagement, //65 536
            Spotting, //1 048 576
            PathFinding, //16 777 216
            Tactics, //268 435 456

            //Third number
            Tracking, //1
            Trainer, //16
            Looting, //16 777 216
            HorseArchery, //268 435 456

            //Fourth number
            Riding, //1
            Athletics, //16
            Shield, //256
            WeaponMaster, //4 096
            Ironflesh, //65 536

            //Fifth number
            PowerDraw, //16
            PowerThrow, //256
            PowerStrike, //4 096
        }
        public enum Proficiency
        {
            OneHandWeapon,
            TwoHandWeapon,
            Polearms,
            Archery,
            Crossbows,
            Throwing,
            Firearms
        }
        public enum Attribute
        {
            Strength,
            Agility,
            Intelligence,
            Charisma
        }
        public enum Gender
        {
            Male,
            Female,
            Other
        }
        public enum Flag
        {
            Unmovable, //268 435 456
            Ranged, //67 108 864
            Shield, //33 554 432
            Horse, //16 777 216
            Gloves, //8 388 608
            Helmet, //4 194 304
            Armor, //2 097 152
            Boots, //1 048 576
            Merchant, //4 096
            Mounted, //1 024
            NoCapture, //256
            FallDead, //128
            Unkillable, //64
            Inactive, //32
            Hero, //16
            Skin //0-15
        }

        public Unit(int id, string[] txtUnitData)
        {
            ID = id;
            List<string[]> fs = new List<string[]>();
            foreach (var line in txtUnitData)
            {
                fs.Add(line.Replace("\r\n", "").Split(' '));
            }
            TroopID = fs[0][0];
            EngSingleName = fs[0][1].Replace('_', ' ');
            EngMultiplyName = fs[0][2].Replace('_', ' ');
            //Flags
            var flagsNum = Convert.ToInt32(fs[0][4]);
            foreach (var df in dividendsFlags)
            {
                var dr = App.GetDivRsltAndRest(flagsNum, df.Value);
                flagsNum = dr[1];
                Flags.Add(df.Key, (byte)dr[0]);
            }
            FactionID = Convert.ToInt32(fs[0][7]);
            UpgradePath[0] = Convert.ToInt32(fs[0][8]);
            UpgradePath[1] = Convert.ToInt32(fs[0][9]);
            //Items
            List<int> itms = new List<int>();
            foreach (var it in fs[1])
            {
                if (it.Equals("0") || it.Equals("")) continue;
                if (it.Equals("-1")) break;
                itms.Add(Convert.ToInt32(it));
            }
            ItemsIDs = itms.ToArray();
            //Attributes
            var attrs = Enum.GetValues(typeof(Attribute));
            int iattrs = 2;
            foreach (var attr in attrs)
            {
                Attributes.Add((Attribute)attr, (byte)Convert.ToInt16(fs[2][iattrs++]));
            }
            //Level
            Level = (byte)Convert.ToInt16(fs[2][6]);
            //Proficiencies
            var prfs = Enum.GetValues(typeof(Proficiency));
            int iprfs = 1;
            foreach (var prf in prfs)
            {
                Proficiencies.Add((Proficiency)prf, (byte)Convert.ToInt32(fs[3][iprfs++]));
            }
            //Skills
            for (int i = 0; i < dividendsSkills.Length; i++)
            {
                var dividend = Convert.ToInt64(fs[4][i]);
                foreach (var sk in dividendsSkills[i])
                {
                    var rd = App.GetDivRsltAndRest(dividend, sk.Value);
                    dividend = rd[1];
                    Skills.Add(sk.Key, (byte)rd[0]);
                }
            }
        }
        public void SetLocNames(Dictionary<string, string> locDict)
        {
            if (locDict.ContainsKey(TroopID))
                LocSingleName = locDict[TroopID];
            if (locDict.ContainsKey(TroopID + "_pl"))
                LocMultiplyName = locDict[TroopID + "_pl"];
        }
        public bool isCheckedFlag(Flag flag)
        {
            return Convert.ToBoolean(Flags[flag]);
        }
        //Static data
        private static readonly Dictionary<Skill, int>[] dividendsSkills = new Dictionary<Skill, int>[]
        {
            new Dictionary<Skill, int>()
            {
                { Skill.Persuasion, 268435456 },
                { Skill.PrisonerManagement, 256 },
                { Skill.Leadership, 16 },
                { Skill.Trade, 1 }
            },
            new Dictionary<Skill, int>()
            {
                { Skill.Tactics, 268435456 },
                { Skill.PathFinding, 16777216 },
                { Skill.Spotting, 1048576 },
                { Skill.InventoryManagement, 65536 },
                { Skill.WoundTreatment, 4096 },
                { Skill.Surgery, 256 },
                { Skill.FirstAid, 16 },
                { Skill.Engineer, 1 }
            },
            new Dictionary<Skill, int>()
            {
                { Skill.HorseArchery, 268435456 },
                { Skill.Looting, 16777216 },
                { Skill.Trainer, 16 },
                { Skill.Tracking, 1 }
            },
            new Dictionary<Skill, int>()
            {
                { Skill.Ironflesh, 65536 },
                { Skill.WeaponMaster, 4096 },
                { Skill.Shield, 256 },
                { Skill.Athletics, 16 },
                { Skill.Riding, 1 }
            },
            new Dictionary<Skill, int>()
            {
                { Skill.PowerStrike, 4096 },
                { Skill.PowerThrow, 256 },
                { Skill.PowerDraw, 16 }
            }
        };



        private static readonly Dictionary<Flag, int> dividendsFlags = new Dictionary<Flag, int>()
        {
            { Flag.Unmovable, 268435456 },
            { Flag.Ranged, 67108864 },
            { Flag.Shield,  33554432 },
            { Flag.Horse,  16777216 },
            { Flag.Gloves, 8388608 },
            { Flag.Helmet,  4194304 },
            { Flag.Armor,  2097152 },
            { Flag.Boots,  1048576 },
            { Flag.Merchant, 4096 },
            { Flag.Mounted,  1024 },
            { Flag.NoCapture,  256 },
            { Flag.FallDead,  128 },
            { Flag.Unkillable, 64 },
            { Flag.Inactive, 32 },
            { Flag.Hero, 16 },
            { Flag.Skin, 1 }
        };
    }
}
