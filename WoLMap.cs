using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritBoard
{
    public class WoLSpirit
    {
        // Hash value for spirit name
        public ulong spirit_id;
        public ulong battle_id;
        public ulong aux_val;

        // Type of spirit
        public ulong type;

        public bool bFighter { get => this.type == WoLSpace.SPACE_TYPE_FIGHTER; }

        public WoLSpirit(ulong spirit_id, ulong battle_id, ulong type)
        {
            this.spirit_id = spirit_id;
            this.battle_id = battle_id;
            this.type = type;
        }
    }

    public class WoLSpace
    {
        public const ulong SPACE_TYPE_SPIRIT = 71090895625;
        public const ulong SPACE_TYPE_FIGHTER = 76299953489;
        public const ulong SPACE_TYPE_MASTER = 71269287840;
        public const ulong SPACE_TYPE_BUILD = 67095396655;
        public const ulong SPACE_TYPE_BOSS = 63058946956;
        public const ulong SPACE_TYPE_CHEST = 62536968360;
        public const ulong SPACE_TYPE_SPACE = 64623784654;

        // The index of this space in its respective map
        public int index;

        // Hash value for spirit name
        public ulong spirit_id;
        public ulong battle_id;
        public ulong aux_val;

        // Type of spirit (fighter or normal)
        public ulong type;

        public WoLMap owningMap;

        public WoLSpace(int index, ulong spirit_id, ulong battle_id, ulong type, WoLMap owner)
        {
            this.index = index;
            this.spirit_id = spirit_id;
            this.battle_id = battle_id;
            this.type = type;
            this.owningMap = owner;
        }


        public void SetSpirit(WoLSpirit spirit)
        {
            this.spirit_id = spirit.spirit_id;
            this.battle_id = spirit.battle_id;
            this.type = spirit.type;
        }
    }


    public class WoLMap
    {
        public static int WoLSpiritsMinThreshold = 564;

        // The name of the map
        public string name;

        // List of all the spaces in the map
        public Dictionary<int, WoLSpace> map_spaces = new Dictionary<int, WoLSpace>();

        public WoLMap(string name)
        {
            this.name = name;
        }
    }


    public class WoLObstacle
    {
        public List<ulong> clear_spirits = new List<ulong>();

        public static ulong GetHashFromIndex(int Index)
        {
            switch (Index)
            {
                case 0:
                    return 39773839969;
                case 1:
                    return 42341364699;
                case 2:
                    return 41552626509;
                case 3:
                    return 39508137710;
                case 4: 
                    return 39826581112;
                case 5:
                    return 42359338946;
                case 6:
                    return 41537701716;
                case 7:
                    return 39651384005;
            }

            return 39403955126;
        }
    }
}



public class WoLRandomizerSaveData
{
    public bool bUseSeed;
    public int Seed;
    public bool bReplaceChests;
    public bool bRandomizeBosses;
    public bool bRandomizeMasters;
    public bool bDisableSummons;
    public bool bHazardKey;
    public bool bDataOrg;
    public bool bDLCAuto;
    public bool bSupport;
    public bool bLegend;
    public bool bRetainChest;
    public bool bLegendFinal;
    public bool bRandomQuiz;
    public bool bRandomBoss;
    public int Mode;
    public int RankLimit;
    public int SphereCount;

    public bool bPlant;
    public bool bJoker;
    public bool bHero;
    public bool bBanjo;
    public bool bTerry;
    public bool bByleth;
    public bool bMinMin;
    public bool bSteve;
    public bool bSephiroth;
    public bool bPyra;
    public bool bKazuya;
    public bool bSora;
}