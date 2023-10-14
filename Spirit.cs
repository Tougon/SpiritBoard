using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paracobNET;

namespace SpiritBoard
{
    public static class SpiritParameterNames
    {
        #region Spirit Params
        public const ulong SAVE_NO = 33164199940;
        public const ulong SPIRIT_ID = 53427023962;
        public const ulong NAME_ID = 31962143958;
        public const ulong FIXED_NO = 35148020683;
        public const ulong IS_BCAT = 31214490639;
        public const ulong IS_DLC = 29545205103;
        public const ulong DIRECTORY_ID = 52287506079;
        public const ulong TYPE = 19543250729;
        public const ulong UI_SERIES_ID = 51821371585;
        public const ulong RANK = 19469560037;
        public const ulong SLOT_NUM = 35676224899;
        public const ulong ABILITY_ID = 45098653874;
        public const ulong ATTR = 17255244564;
        public const ulong EXP_LVL_MAX = 43454034016;
        public const ulong EXP_UP_RATE = 47825954359;
        public const ulong BASE_ATTACK = 48043701586;
        public const ulong MAX_ATTACK = 46449946034;
        public const ulong BASE_DEFENSE = 55560064017;
        public const ulong MAX_DEFENSE = 48575721265;
        public const ulong GROW_TYPE = 39938080628;
        public const ulong PERSONALITY = 51033116846;
        public const ulong EVOLVE_SRC = 45413989649;
        public const ulong SUPER_ABILITY = 58112018064;
        public const ulong IS_BOARD_APPEAR = 68196686155;
        public const ulong FIGHTER_CONDITIONS = 77329634968;
        public const ulong APPEAR_CONDITIONS = 73204674956;
        public const ulong x14ef76fcd7 = 89916898519;
        public const ulong x1531b0c6f0 = 91027982064;
        public const ulong x16db57210b = 98169200907;
        public const ulong REWARD_CAPACITY = 64531758247;
        public const ulong BATTLE_EXP = 43113832815;
        public const ulong SUMMON_SP = 40381716419;
        public const ulong SUMMON_ITEM_1 = 54730785672;
        public const ulong SUMMON_ITEM_1_NUM = 72282460580;
        public const ulong SUMMON_ITEM_2 = 52197904946;
        public const ulong SUMMON_ITEM_2_NUM = 71202417524;
        public const ulong SUMMON_ITEM_3 = 52885709476;
        public const ulong SUMMON_ITEM_3_NUM = 71649100484;
        public const ulong SUMMON_ITEM_4 = 55001970439;
        public const ulong SUMMON_ITEM_4_NUM = 69201714900;
        public const ulong SUMMON_ITEM_5 = 54649178001;
        public const ulong SUMMON_ITEM_5_NUM = 69287704420;
        public const ulong GAME_TITLE = 54942300082;
        public const ulong SHOP_SALES_TYPE = 64612882394;
        public const ulong SHOP_PRICE = 43182356619;
        public const ulong REMATCH = 74944547869;
        public const ulong x13656d7462 = 83306050658;
        public const ulong CHECK_ID = 36248454631;
        public const ulong x115044521b = 74361098779;
        #endregion

        #region Spirit Battle Params
        public const ulong BATTLE_ID = 42034472729;
        public const ulong BATTLE_TYPE = 50931012799;
        public const ulong BATTLE_TIME_SEC = 66223301671;
        public const ulong BASIC_INIT_DAMAGE = 75037786441;
        public const ulong BASIC_INIT_HP = 58914828870;
        public const ulong BASIC_STOCK = 49677531388;
        public const ulong UI_STAGE_ID = 47837208762;
        public const ulong STAGE_TYPE = 44259039420;
        public const ulong x18e536d4f7 = 106924791031;
        public const ulong STAGE_BGM = 41596818533;
        public const ulong STAGE_GIMMICK = 56055259123;
        public const ulong STAGE_ATTR = 46282779777;
        public const ulong FLOOR_PLACE_ID = 62811072776;
        public const ulong ITEM_TABLE = 46650004630;
        public const ulong ITEM_LEVEL = 45907564483;
        public const ulong RESULT_TYPE = 47729429607;
        public const ulong EVENT1_TYPE = 49964578868;
        public const ulong EVENT1_LABEL = 53377760414;
        public const ulong EVENT1_START_TIME = 74516818281;
        public const ulong EVENT1_RANGE_TIME = 74966201746;
        public const ulong EVENT1_COUNT = 55414220820;
        public const ulong EVENT1_DAMAGE = 58666625225;
        public const ulong EVENT2_TYPE = 47857699482;
        public const ulong EVENT2_LABEL = 53089920515;
        public const ulong EVENT2_START_TIME = 73787423641;
        public const ulong EVENT2_RANGE_TIME = 73078122338;
        public const ulong EVENT2_COUNT = 55148409481;
        public const ulong EVENT2_DAMAGE = 56476479274;
        public const ulong EVENT3_TYPE = 51268442431;
        public const ulong EVENT3_LABEL = 55735020983;
        public const ulong EVENT3_START_TIME = 76063176694;
        public const ulong EVENT3_RANGE_TIME = 75571849997;
        public const ulong EVENT3_COUNT = 53442705725;
        public const ulong EVENT3_DAMAGE = 59775692724;
        // Hint 1
        public const ulong x0d41ef8328 = 56940790568;
        // Hint 2
        public const ulong AW_FLAP_DELAY = 57854312429;
        // Hint 3
        public const ulong x0d6f19abae = 57698528174;
        public const ulong POWER_SKILL_1 = 106724335473;
        public const ulong POWER_SKILL_2 = 104158023371;
        public const ulong RECOMMENDED_SKILL_1 = 78812878826;
        public const ulong RECOMMENDED_SKILL_2 = 80540354128;
        public const ulong RECOMMENDED_SKILL_3 = 80389306054;
        public const ulong RECOMMENDED_SKILL_4 = 78013522789;
        public const ulong RECOMMENDED_SKILL_5 = 78902252531;
        public const ulong RECOMMENDED_SKILL_6 = 80664429129;
        public const ulong RECOMMENDED_SKILL_7 = 80278893279;
        public const ulong RECOMMENDED_SKILL_8 = 77850586958;
        public const ulong RECOMMENDED_SKILL_9 = 78773657560;
        public const ulong RECOMMENDED_SKILL_10 = 84630391120;
        public const ulong RECOMMENDED_SKILL_11 = 84881865158;
        public const ulong RECOMMENDED_SKILL_12 = 83119777916;
        public const ulong RECOMMENDED_SKILL_13 = 82364864746;
        public const ulong UNRECOMMENDED_SKILL_1 = 94242151002;
        public const ulong UNRECOMMENDED_SKILL_2 = 91944143840;
        public const ulong UNRECOMMENDED_SKILL_3 = 90719353718;
        public const ulong UNRECOMMENDED_SKILL_4 = 92361715413;
        public const ulong UNRECOMMENDED_SKILL_5 = 94324186691;
        public const ulong x0ff8afd14f = 68596781391;
        public const ulong BATTLE_POWER = 54537572370;
        #endregion

        #region Fighter Params
        public const ulong STOCK = 22736688736;
        public const ulong HP = 11760337516;
        public const ulong ENTRY_TYPE = 43637288762;
        public const ulong FIRST_APPEAR = 53896991786;
        public const ulong APPEAR_RULE_TIME = 68778365675;
        public const ulong APPEAR_RULE_COUNT = 76048751751;
        public const ulong FIGHTER_KIND = 52950921394;
        public const ulong COLOR = 23191767273;
        public const ulong MII_HAT_ID = 43230825124;
        public const ulong MII_BODY_ID = 50063036678;
        public const ulong MII_COLOR = 41993618733;
        public const ulong MII_VOICE = 39840568831;
        public const ulong MII_SP_N = 34880360283;
        public const ulong MII_SP_S = 36441056130;
        public const ulong MII_SP_HI = 39939215748;
        public const ulong MII_SP_LW = 42193906147;
        public const ulong CPU_LV = 28141679897;
        public const ulong CPU_TYPE = 36961838151;
        public const ulong CPU_SUB_TYPE = 54389308830;
        public const ulong CPU_ITEM_PICK_UP = 71994691201;
        public const ulong CORPS = 24875628699;
        public const ulong x0f2077926c = 64969216620;
        public const ulong INIT_DAMAGE = 47823317398;
        public const ulong SUB_RULE = 35992253197;
        public const ulong SCALE = 25438856580;
        public const ulong FLY_RATE = 35827135730;
        public const ulong INVALID_DROP = 54188195418;
        public const ulong ENABLE_CHARGE_FINAL = 84207028511;
        public const ulong SPIRIT_NAME = 50180791925;
        public const ulong ATTACK = 26973580603;
        public const ulong DEFENSE = 33749857653;
        public const ulong ABILITY1 = 37254225118;
        public const ulong ABILITY2 = 35258305892;
        public const ulong ABILITY3 = 35475963378;
        public const ulong ABILITY_PERSONAL = 72745440525;
        #endregion

        #region Aesthetic Params
        public const ulong LAYOUT_ID = 85344946810;
        public const ulong ART_CENTER_X = 81324093633;
        public const ulong ART_CENTER_Y = 79864528983;
        public const ulong ART_SCALE = 55000750714;
        public const ulong ART_STAND_CENTER_X = 118656921893;
        public const ulong ART_STAND_CENTER_Y = 119580115379;
        public const ulong ART_STAND_SCALE = 92765258277;
        public const ulong ART_BOARD_X = 125700157464;
        public const ulong ART_BOARD_Y = 125415391374;
        public const ulong ART_BOARD_SCALE = 99883486986;
        public const ulong ART_SELECT_X = 121990021584;
        public const ulong ART_SELECT_Y = 120530350406;
        public const ulong ART_SELECT_SCALE = 97675030038;
        public const ulong EFFECT_COUNT = 43057268577;
        public const ulong EFFECT_0_X = 64255039256;
        public const ulong EFFECT_0_Y = 62325335950;
        public const ulong EFFECT_1_X = 64225574191;
        public const ulong EFFECT_1_Y = 62329671097;
        public const ulong EFFECT_2_X = 64255039256;
        public const ulong EFFECT_2_Y = 62300484576;
        public const ulong EFFECT_3_X = 64284043585;
        public const ulong EFFECT_3_Y = 62288009687;
        public const ulong EFFECT_4_X = 64205340612;
        public const ulong EFFECT_4_Y = 62376152914;
        public const ulong EFFECT_5_X = 64209409523;
        public const ulong EFFECT_5_Y = 62346945893;
        public const ulong EFFECT_6_X = 64179891114;
        public const ulong EFFECT_6_Y = 62384790332;
        public const ulong EFFECT_7_X = 64167149981;
        public const ulong EFFECT_7_Y = 62405882123;
        public const ulong EFFECT_8_X = 64356697760;
        public const ulong EFFECT_8_Y = 62494242358;
        public const ulong EFFECT_9_X = 64327462039;
        public const ulong EFFECT_9_Y = 62498282497;
        public const ulong EFFECT_10_X = 67624186335;
        public const ulong EFFECT_10_Y = 67808264521;
        public const ulong EFFECT_11_X = 67636648936;
        public const ulong EFFECT_11_Y = 67787451262;
        public const ulong EFFECT_12_X = 67598804401;
        public const ulong EFFECT_12_Y = 67816969511;
        public const ulong EFFECT_13_X = 67594456966;
        public const ulong EFFECT_13_Y = 67846455056;
        public const ulong EFFECT_14_X = 67540785411;
        public const ulong EFFECT_14_Y = 67892783509;
        #endregion
    }


    [Serializable]
    public class Spirit : IComparable<Spirit>
    {
        public const int MAX_FIGHTERS = 10;

        // Used to track iteration of the Spirit class, incremented if major changes occur
        public int version = 7;

        // Index of spirit within the spirit db
        public int index = -1;

        // Display name in the MSBT
        public string display_name;
        public bool displayingAltBattle = false;

        // Spirit ID in save data
        public ushort save_no;
        // Hash value for spirit name
        public ulong spirit_id;
        // Name to pull from MSBT
        public string name_id;
        // Unknown purpose
        public ushort fixed_no;
        // Number in the spirit list
        public ushort directory_id;
        // Indicates if the spirit exists within BCAT, or event data
        public bool is_bcat;
        // Indicates if spirit was an event spirit or included with any challenger pack
        public bool is_dlc;
        // Type of spirit (fighter, master, "attack," support, etc.)
        public ulong type;
        // Spirit series id
        public ulong series_id;
        // Spirit rank
        public ulong rank;
        // Number of slots
        public byte slots;
        // Spirit ability
        public ulong ability_id;
        // Spirit attribute (attack, defense, grab, support)
        public ulong spirit_attr;
        // Total experience points
        public uint exp_lvl_max;
        // Level up rate
        public float exp_up_rate;
        // Base attack and max attack
        public short base_attack;
        public short max_attack;
        // Base defense and max defense
        public short base_defense;
        public short max_defense;
        // Spirit growth type, effect unknown as every spirit uses grow_normal
        public ulong grow_type;
        // Unknown effect
        public byte personality;
        // Spirit that this spirit evolves from
        public ulong evolve_src;
        // Spirit ability, used only on evolved spirits
        public ulong super_ability;
        // Does this spirit appear on the spirit board
        public bool is_board_appear;
        // Spirit will only appear if the given fighter is unlocked, used for DLC spirits
        public ulong fighter_conditions;
        // Determines which group of spirits this spirit is tied to for the spirit board
        public ulong appear_conditions;
        // Amount of spirit points awarded
        public uint reward_capacity;
        // Amount of exp awarded
        public uint battle_exp;
        // Amount of sp needed to summon
        public uint summon_sp;
        // Game title to use for spirit, overriding generic franchise name (0x0ccad0f7b2)
        public ulong game_title;
        // Indicates if spirit can be bought in the shop (on_sale, limited, not_for_sale)
        public ulong shop_sales_type;
        // Price of spirit in the shop
        public uint shop_price;
        // Is rematchable (0x11730b0c1d)
        public bool rematch;

        #region Summon Params
        /* * *
         * These params control the data used for summoning spirits, including the spirits required
         * to summon, and the quanitity needed. Use none in most cases.
         * * */

        public ulong summon_id1;
        public byte summon_quantity1;
        public ulong summon_id2;
        public byte summon_quantity2;
        public ulong summon_id3;
        public byte summon_quantity3;
        public ulong summon_id4;
        public byte summon_quantity4;
        public ulong summon_id5;
        public byte summon_quantity5;
        #endregion

        #region DLC Alternate Battle Params
        /* * *
         * These values are used for spirits like Cuphead and Aerith that changed based on
         * whether or not the player has purchased the appropriate Mii Costumes, as these values are
         * only present for these battles. I'm unsure if anything can be done with them.
         * * */
        // The item type that must be purchased (type_mii_hat, type_mii_body)
        public ulong x13656d7462;
        // The id of the item to check for, Ryo Sakazaki simply uses the id "ryo"
        public ulong check_id;
        // The id of the alternate battle
        public ulong x115044521b;
        #endregion

        #region Unlabled Params
        public uint x14ef76fcd7;
        public uint x1531b0c6f0;
        public float x16db57210b;
        #endregion

        #region Editor Only
        public bool Custom = false;
        #endregion

        public SpiritBattle battle;
        public SpiritBattle alternateBattle;
        public SpiritAesthetics aesthetics;


        public Spirit()
        {
            is_bcat = false;
            is_dlc = false;

            type = 81665422107;
            series_id = 58671107356;
            rank = 26332195044;
            spirit_attr = 74193544331;
            exp_lvl_max = 6000;
            exp_up_rate = 1.04f;
            grow_type = 49396441367;
            personality = 4;
            ability_id = 19320013007;
            super_ability = 19320013007;
            evolve_src = 8403017505;
            is_board_appear = true;
            fighter_conditions = 68014307960;
            appear_conditions = 61968333762;
            x14ef76fcd7 = 5;
            x1531b0c6f0 = 5;
            x16db57210b = 50;
            reward_capacity = 1072;
            battle_exp = 100;
            summon_id1 = 19320013007;
            summon_id2 = 19320013007;
            summon_id3 = 19320013007;
            summon_id4 = 19320013007;
            summon_id5 = 19320013007;
            shop_sales_type = 32952601179;
            shop_price = 300;
            game_title = 73502405852;

            fixed_no = 0;
        }

        public Spirit(ParamStruct paramStruct)
        {
            // There are 49 values per spirit;
            var values = paramStruct.Nodes;

            if(values.Count != 49)
            {
                throw new InvalidOperationException("Spirit cannot be parsed.");
            }
            else
            {
                save_no = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.SAVE_NO]).Value);
                spirit_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SPIRIT_ID]).Value);
                name_id = Convert.ToString(((ParamValue)values[SpiritParameterNames.NAME_ID]).Value);
                fixed_no = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.FIXED_NO]).Value);
                is_bcat = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.IS_BCAT]).Value);
                is_dlc = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.IS_DLC]).Value);
                directory_id = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.DIRECTORY_ID]).Value);
                type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.TYPE]).Value);
                series_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UI_SERIES_ID]).Value);
                rank = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RANK]).Value);
                slots = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SLOT_NUM]).Value);
                ability_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ABILITY_ID]).Value);
                spirit_attr = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ATTR]).Value);
                exp_lvl_max = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.EXP_LVL_MAX]).Value);
                exp_up_rate = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.EXP_UP_RATE]).Value);
                base_attack = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.BASE_ATTACK]).Value);
                max_attack = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.MAX_ATTACK]).Value);
                base_defense = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.BASE_DEFENSE]).Value);
                max_defense = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.MAX_DEFENSE]).Value);
                grow_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.GROW_TYPE]).Value);
                personality = Convert.ToByte(((ParamValue)values[SpiritParameterNames.PERSONALITY]).Value);
                evolve_src = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVOLVE_SRC]).Value);
                super_ability = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUPER_ABILITY]).Value);
                is_board_appear = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.IS_BOARD_APPEAR]).Value);
                fighter_conditions = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.FIGHTER_CONDITIONS]).Value);
                appear_conditions = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.APPEAR_CONDITIONS]).Value);
                x14ef76fcd7 = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.x14ef76fcd7]).Value);
                x1531b0c6f0 = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.x1531b0c6f0]).Value);
                x16db57210b = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.x16db57210b]).Value);
                reward_capacity = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.REWARD_CAPACITY]).Value);
                battle_exp = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.BATTLE_EXP]).Value);
                summon_sp = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.SUMMON_SP]).Value);
                summon_id1 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_1]).Value);
                summon_quantity1 = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_1_NUM]).Value);
                summon_id2 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_2]).Value);
                summon_quantity2 = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_2_NUM]).Value);
                summon_id3 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_3]).Value);
                summon_quantity3 = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_3_NUM]).Value);
                summon_id4 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_4]).Value);
                summon_quantity4 = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_4_NUM]).Value);
                summon_id5 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_5]).Value);
                summon_quantity5 = Convert.ToByte(((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_5_NUM]).Value);
                game_title = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.GAME_TITLE]).Value);
                shop_sales_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SHOP_SALES_TYPE]).Value);
                shop_price = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.SHOP_PRICE]).Value);
                rematch = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.REMATCH]).Value);
                x13656d7462 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.x13656d7462]).Value);
                check_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.CHECK_ID]).Value);
                x115044521b = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.x115044521b]).Value);
            }
        }

        public int CompareTo(Spirit other)
        {
            return directory_id.CompareTo(other.directory_id);
        }

        public Spirit Copy()
        {
            Spirit result = (Spirit)this.MemberwiseClone();
            result.display_name = String.Copy(this.display_name);
            result.name_id = String.Copy(this.name_id);
            result.battle = this.battle.Copy();
            if(alternateBattle != null)
                result.alternateBattle = this.alternateBattle.Copy();

            return result;
        }


        public static int RankToStarValue(ulong rank)
        {
            switch (rank)
            {
                case 26332195044:
                    return 1;
                case 19574448332:
                    return 2;
                case 16196097462:
                    return 3;
                case 29098685350:
                    return 4;
            }

            return 1;
        }


        public int GetSpiritType()
        {
            // Fighter spirits are irrelevant here
            if(type == 86992126058)
            {
                return -1;
            }

            int kind = 10000;

            switch (type)
            {
                case 89039743027:
                    kind = 20000;
                    break;
                case 83372998134:
                    kind = 30000;
                    break;
            }

            kind += (RankToStarValue(rank) * 1000);

            if(type == 81665422107)
            {
                // Originally 0 slots was separate but created some 0 categories.
                kind += slots == 0 ? 100 : (slots * 100);
                // Makes TGN Type too broad, risky to use
                //kind += ((ability_id != 19320013007 || super_ability != 19320013007) ? 1 : 0);
            }

            // Makes TGN Type too broad, risky to use
            //if (evolve_src != 19320013007)
                //kind += 10;

            return kind;
        }
    }


    [Serializable]
    public class SpiritBattle
    {
        // Index of battle within the battle db
        public int index = -1;

        // ID of the spirit this battle corresponds to
        public ulong battle_id;
        // Type of battle
        public ulong battle_type;
        // Time of the battle in seconds
        public ushort battle_time_sec;
        // Initial damage (player)
        public ushort basic_init_damage;
        // Initial hp (player)
        public ushort basic_init_hp;
        // Number of stocks (player)
        public byte basic_stock;
        // Stage ID
        public ulong stage_id;
        // The type of stage (normal, FD, BF)
        public ulong stage_type;
        // Music ID
        public ulong stage_bgm;
        // Whether or not hazards are on
        public bool stage_gimmick;
        // Type of environment gimmick
        public ulong stage_attr;
        // Location of the floor (unconfirmed)
        public ulong floor_place_id;
        // Types of items allowed
        public ulong item_table;
        // Item frequency
        public ulong item_level;
        // Win condition
        public ulong result_type;
        // Unknown purpose
        public bool aw_flap_delay;
        // Battle power
        public uint battle_power;

        #region Event Params
        /* * *
         * Controls the events that occur during the match
         * Type - The event type
         * Label - Who the event targets (player/enemy)
         * Start Time - How long it takes for the event to begin (seconds)
         * Range Time - Duration of the event (Use -1 for infinite)
         * Count - How many times the event occurs
         * Damage - Damage threshold the event activates at
         * * */
        public ulong event1_type;
        public ulong event1_label;
        public int event1_start_time;
        public int event1_range_time;
        public byte event1_count;
        public ushort event1_damage;

        public ulong event2_type;
        public ulong event2_label;
        public int event2_start_time;
        public int event2_range_time;
        public byte event2_count;
        public ushort event2_damage;

        public ulong event3_type;
        public ulong event3_label;
        public int event3_start_time;
        public int event3_range_time;
        public byte event3_count;
        public ushort event3_damage;
        #endregion

        #region Skill Params
        // Skill that has the biggest impact on the battle, can be none (0x18d9441f71)
        public ulong auto_win_skill;
        // Skills that can help, but do not negate the fight's gimmicks outright (13 max)
        public ulong[] recommended_skills = new ulong[13];
        // Skills that do not help (5 max)
        public ulong[] un_recommended_skills = new ulong[5];
        #endregion

        #region Unlabled Params
        public sbyte x18e536d4f7; // Likely for stage tier. It's 6 for Kapp'n
        public bool x0d41ef8328;
        public bool x0d6f19abae;
        public ulong x18404d4ecb;
        public ulong x0ff8afd14f;
        #endregion

        public List<SpiritBattleFighter> fighters = new List<SpiritBattleFighter>();
        public List<int> fighterDBIndexes = new List<int>();


        public SpiritBattle()
        {

        }


        public SpiritBattle(ParamStruct paramStruct)
        {
            // There are 59 values per spirit;
            var values = paramStruct.Nodes;

            if(values.Count < 59)
            {
                throw new InvalidOperationException("Spirit Battle cannot be parsed.");
            }
            else
            {
                battle_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.BATTLE_ID]).Value);
                battle_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.BATTLE_TYPE]).Value);
                battle_time_sec = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.BATTLE_TIME_SEC]).Value);
                basic_init_damage = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.BASIC_INIT_DAMAGE]).Value);
                basic_init_hp = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.BASIC_INIT_HP]).Value);
                basic_stock = Convert.ToByte(((ParamValue)values[SpiritParameterNames.BASIC_STOCK]).Value);
                stage_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UI_STAGE_ID]).Value);
                stage_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.STAGE_TYPE]).Value);
                x18e536d4f7 = Convert.ToSByte(((ParamValue)values[SpiritParameterNames.x18e536d4f7]).Value);
                stage_bgm = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.STAGE_BGM]).Value);
                stage_gimmick = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.STAGE_GIMMICK]).Value);
                stage_attr = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.STAGE_ATTR]).Value);
                floor_place_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.FLOOR_PLACE_ID]).Value);
                item_table = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ITEM_TABLE]).Value);
                item_level = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ITEM_LEVEL]).Value);
                result_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RESULT_TYPE]).Value);
                event1_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT1_TYPE]).Value);
                event1_label = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT1_LABEL]).Value);
                event1_start_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT1_START_TIME]).Value);
                event1_range_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT1_RANGE_TIME]).Value);
                event1_count = Convert.ToByte(((ParamValue)values[SpiritParameterNames.EVENT1_COUNT]).Value);
                event1_damage = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.EVENT1_DAMAGE]).Value);
                event2_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT2_TYPE]).Value);
                event2_label = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT2_LABEL]).Value);
                event2_start_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT2_START_TIME]).Value);
                event2_range_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT2_RANGE_TIME]).Value);
                event2_count = Convert.ToByte(((ParamValue)values[SpiritParameterNames.EVENT2_COUNT]).Value);
                event2_damage = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.EVENT2_DAMAGE]).Value);
                event3_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT3_TYPE]).Value);
                event3_label = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.EVENT3_LABEL]).Value);
                event3_start_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT3_START_TIME]).Value);
                event3_range_time = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EVENT3_RANGE_TIME]).Value);
                event3_count = Convert.ToByte(((ParamValue)values[SpiritParameterNames.EVENT3_COUNT]).Value);
                event3_damage = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.EVENT3_DAMAGE]).Value);
                x0d41ef8328 = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.x0d41ef8328]).Value);
                aw_flap_delay = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.AW_FLAP_DELAY]).Value);
                if ((values).ContainsKey(SpiritParameterNames.x0d6f19abae))
                    x0d6f19abae = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.x0d6f19abae]).Value);
                else
                    x0d6f19abae = true;
                auto_win_skill = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.POWER_SKILL_1]).Value);
                x18404d4ecb = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.POWER_SKILL_2]).Value);
                recommended_skills[0] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_1]).Value);
                recommended_skills[1] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_2]).Value);
                recommended_skills[2] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_3]).Value);
                recommended_skills[3] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_4]).Value);
                recommended_skills[4] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_5]).Value);
                recommended_skills[5] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_6]).Value);
                recommended_skills[6] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_7]).Value);
                recommended_skills[7] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_8]).Value);
                recommended_skills[8] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_9]).Value);
                recommended_skills[9] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_10]).Value);
                recommended_skills[10] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_11]).Value);
                recommended_skills[11] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_12]).Value);
                recommended_skills[12] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_13]).Value);
                un_recommended_skills[0] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_1]).Value);
                un_recommended_skills[1] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_2]).Value);
                un_recommended_skills[2] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_3]).Value);
                un_recommended_skills[3] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_4]).Value);
                un_recommended_skills[4] = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_5]).Value);
                x0ff8afd14f = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.x0ff8afd14f]).Value);
                battle_power = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.BATTLE_POWER]).Value);
            }
        }


        public SpiritBattle Copy()
        {
            var fighters = new List<SpiritBattleFighter>();

            foreach (var fighter in this.fighters)
            {
                fighters.Add(fighter.Copy());
            }

            SpiritBattle result = (SpiritBattle)this.MemberwiseClone();
            result.fighters = fighters;
            return result;
        }
    }


    [Serializable]
    public class SpiritBattleFighter
    {
        // ID of the spirit/battle this fighter corresponds to
        public ulong battle_id;
        // Number of copies of the fighter to spawn
        public byte stock;
        // Fighter's HP
        public ushort hp;
        // Indicates if this is the "main" fighter
        public ulong entry_type;
        // Does this fighter appear at the beginning of the battle?
        public bool first_appear;
        // Spawns when X seconds have passed
        public ushort appear_rule_time;
        // Spawns when X Fighters have been defeated
        public ushort appear_rule_count;
        // Fighter ID
        public ulong fighter_kind;
        // ID of the fighter's color (zero indexed)
        public byte color;
        // ID of the Mii Hat to use (if fighter is a Mii)
        public ulong mii_hat_id;
        // ID of the Mii Costume to use (if fighter is a Mii)
        public ulong mii_body_id;
        // Color of the Mii
        public byte mii_color;
        // Mii soundbank to load
        public ulong mii_voice;
        // Mii specials (zero indexed)
        public byte mii_sp_n;
        public byte mii_sp_s;
        public byte mii_sp_hi;
        public byte mii_sp_lw;
        // CPU strength
        public byte cpu_lv;
        // CPU main behavior
        public ulong cpu_type;
        // CPU sub behavior
        public ulong cpu_sub_type;
        // Will CPU pick up items
        public bool cpu_item_pick_up;
        // Is this part of a mob fight?
        public bool corps;
        // The initial damage of the fighter
        public ushort init_damage;
        // Sub-rule (used for things like Gold mode, may have other effects)
        public ulong sub_rule;
        // Scale of the fighter
        public float scale;
        // Knockback rate, functions identically to the damage ratio setting in the rules
        public float fly_rate;
        // Unknown purpose
        public bool invalid_drop;
        // Enables FS meter for this fighter
        public bool enable_charge_final;
        // Unknown purpose, but my theory is this determines the spirit displayed on the fighter's damage portrait
        public ulong spirit_name;
        // Fighter's attack power
        public short attack;
        // Fighter's defense power
        public short defense;
        // Fighter's attribute, separate from the spirit's attribute
        public ulong attr;
        // Passive fighter abilities
        public ulong ability_1;
        public ulong ability_2;
        public ulong ability_3;
        public ulong ability_personal;

        #region Unlabled Params
        public bool x0f2077926c;
        #endregion


        public SpiritBattleFighter()
        {
            InitializeValues();
        }


        public SpiritBattleFighter(ulong battleID)
        {
            battle_id = battleID;
            InitializeValues();
        }


        public void InitializeValues()
        {
            stock = 1;
            hp = 150;
            entry_type = 37233412328;
            first_appear = true;
            fighter_kind = 63802951779;
            mii_hat_id = 67275564522;
            mii_body_id = 70328531327;
            mii_voice = 44330854164;
            cpu_lv = 51;
            cpu_type = 40440260678;
            cpu_sub_type = 19320013007;
            sub_rule = 51007861228;
            attr = 74193544331;
            spirit_name = 19320013007;
            fly_rate = 1;
            scale = 1;
            ability_1 = 19320013007;
            ability_2 = 19320013007;
            ability_3 = 19320013007;
            ability_personal = 19320013007;
        }


        public SpiritBattleFighter(ParamStruct paramStruct)
        {
            // There are 37 values per spirit;
            var values = paramStruct.Nodes;

            if (values.Count != 37)
            {
                throw new InvalidOperationException("Spirit Fighter cannot be parsed.");
            }
            else
            {
                battle_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.BATTLE_ID]).Value);
                stock = Convert.ToByte(((ParamValue)values[SpiritParameterNames.STOCK]).Value);
                hp = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.HP]).Value);
                entry_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ENTRY_TYPE]).Value);
                first_appear = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.FIRST_APPEAR]).Value);
                appear_rule_time = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.APPEAR_RULE_TIME]).Value);
                appear_rule_count = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.APPEAR_RULE_COUNT]).Value);
                fighter_kind = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.FIGHTER_KIND]).Value);
                color = Convert.ToByte(((ParamValue)values[SpiritParameterNames.COLOR]).Value);
                mii_hat_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.MII_HAT_ID]).Value);
                mii_body_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.MII_BODY_ID]).Value);
                mii_color = Convert.ToByte(((ParamValue)values[SpiritParameterNames.MII_COLOR]).Value);
                mii_voice = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.MII_VOICE]).Value);
                mii_sp_n = Convert.ToByte(((ParamValue)values[SpiritParameterNames.MII_SP_N]).Value);
                mii_sp_s = Convert.ToByte(((ParamValue)values[SpiritParameterNames.MII_SP_S]).Value);
                mii_sp_hi = Convert.ToByte(((ParamValue)values[SpiritParameterNames.MII_SP_HI]).Value);
                mii_sp_lw = Convert.ToByte(((ParamValue)values[SpiritParameterNames.MII_SP_LW]).Value);
                cpu_lv = Convert.ToByte(((ParamValue)values[SpiritParameterNames.CPU_LV]).Value);
                cpu_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.CPU_TYPE]).Value);
                cpu_sub_type = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.CPU_SUB_TYPE]).Value);
                cpu_item_pick_up = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.CPU_ITEM_PICK_UP]).Value);
                corps = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.CORPS]).Value);
                x0f2077926c = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.x0f2077926c]).Value);
                init_damage = Convert.ToUInt16(((ParamValue)values[SpiritParameterNames.INIT_DAMAGE]).Value);
                sub_rule = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SUB_RULE]).Value);
                scale = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.SCALE]).Value);
                fly_rate = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.FLY_RATE]).Value);
                invalid_drop = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.INVALID_DROP]).Value);
                enable_charge_final = Convert.ToBoolean(((ParamValue)values[SpiritParameterNames.ENABLE_CHARGE_FINAL]).Value);
                spirit_name = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.SPIRIT_NAME]).Value);
                attack = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.ATTACK]).Value);
                defense = Convert.ToInt16(((ParamValue)values[SpiritParameterNames.DEFENSE]).Value);
                attr = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ATTR]).Value);
                ability_1 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ABILITY1]).Value);
                ability_2 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ABILITY2]).Value);
                ability_3 = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ABILITY3]).Value);
                ability_personal = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.ABILITY_PERSONAL]).Value);
            }
        }


        public SpiritBattleFighter Copy()
        {
            return ((SpiritBattleFighter)this.MemberwiseClone());
        }
    }


    [Serializable]
    public class SpiritAesthetics
    {
        // Index of aesthetics within param file
        public int index = -1;

        public ulong layout_id;

        public float[] art_pos_x = new float[4];
        public float[] art_pos_y = new float[4];
        public float[] art_size = new float[4];

        public uint effect_count;
        public int[] effect_pos_x = new int[15];
        public int[] effect_pos_y = new int[15];


        public SpiritAesthetics()
        {

        }


        public SpiritAesthetics(ParamStruct paramStruct)
        {
            // There are 37 values per spirit;
            var values = paramStruct.Nodes;

            if (values.Count != 44)
            {
                throw new InvalidOperationException("Spirit aesthetics cannot be parsed.");
            }
            else
            {
                layout_id = Convert.ToUInt64(((ParamValue)values[SpiritParameterNames.LAYOUT_ID]).Value);
                art_pos_x[0] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_CENTER_X]).Value);
                art_pos_y[0] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_CENTER_Y]).Value);
                art_size[0] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_SCALE]).Value);
                art_pos_x[1] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_STAND_CENTER_X]).Value);
                art_pos_y[1] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_STAND_CENTER_Y]).Value);
                art_size[1] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_STAND_SCALE]).Value);
                art_pos_x[2] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_BOARD_X]).Value);
                art_pos_y[2] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_BOARD_Y]).Value);
                art_size[2] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_BOARD_SCALE]).Value);
                art_pos_x[3] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_SELECT_X]).Value);
                art_pos_y[3] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_SELECT_Y]).Value);
                art_size[3] = Convert.ToSingle(((ParamValue)values[SpiritParameterNames.ART_SELECT_SCALE]).Value);
                effect_count = Convert.ToUInt32(((ParamValue)values[SpiritParameterNames.EFFECT_COUNT]).Value);
                effect_pos_x[0] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_0_X]).Value);
                effect_pos_y[0] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_0_Y]).Value);
                effect_pos_x[1] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_1_X]).Value);
                effect_pos_y[1] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_1_Y]).Value);
                effect_pos_x[2] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_2_X]).Value);
                effect_pos_y[2] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_2_Y]).Value);
                effect_pos_x[3] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_3_X]).Value);
                effect_pos_y[3] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_3_Y]).Value);
                effect_pos_x[4] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_4_X]).Value);
                effect_pos_y[4] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_4_Y]).Value);
                effect_pos_x[5] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_5_X]).Value);
                effect_pos_y[5] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_5_Y]).Value);
                effect_pos_x[6] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_6_X]).Value);
                effect_pos_y[6] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_6_Y]).Value);
                effect_pos_x[7] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_7_X]).Value);
                effect_pos_y[7] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_7_Y]).Value);
                effect_pos_x[8] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_8_X]).Value);
                effect_pos_y[8] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_8_Y]).Value);
                effect_pos_x[9] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_9_X]).Value);
                effect_pos_y[9] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_9_Y]).Value);
                effect_pos_x[10] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_10_X]).Value);
                effect_pos_y[10] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_10_Y]).Value);
                effect_pos_x[11] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_11_X]).Value);
                effect_pos_y[11] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_11_Y]).Value);
                effect_pos_x[12] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_12_X]).Value);
                effect_pos_y[12] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_12_Y]).Value);
                effect_pos_x[13] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_13_X]).Value);
                effect_pos_y[13] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_13_Y]).Value);
                effect_pos_x[14] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_14_X]).Value);
                effect_pos_y[14] = Convert.ToInt32(((ParamValue)values[SpiritParameterNames.EFFECT_14_Y]).Value);
            }
        }


        public SpiritAesthetics Copy()
        {
            return ((SpiritAesthetics)this.MemberwiseClone());
        }
    }


    [System.Serializable]
    public class DLCSpiritBoard
    {
        public List<ulong> IncludedSpirits = new List<ulong>();
    }


    public class SpiritStatsAverage
    {
        // Total experience points
        public List<uint> exp_lvl_max = new List<uint>();
        // Level up rate
        public List<float> exp_up_rate = new List<float>();
        // Base attack and max attack
        public List<short> base_attack = new List<short>();
        public List<short> max_attack = new List<short>();
        // Base defense and max defense
        public List<short> base_defense = new List<short>();
        public List<short> max_defense = new List<short>();

        // Amount of exp awarded
        public List<uint> battle_exp = new List<uint>();
        // Amount of spirit points awarded
        public List<uint> reward_capacity = new List<uint>();


        public void AddSpirit(Spirit spirit, bool IsBoss)
        {
            exp_lvl_max.Add(spirit.exp_lvl_max);
            exp_up_rate.Add(spirit.exp_up_rate);
            base_attack.Add(spirit.base_attack);
            max_attack.Add(spirit.max_attack);
            base_defense.Add(spirit.base_defense);
            max_defense.Add(spirit.max_defense);

            if (!IsBoss)
                battle_exp.Add(spirit.battle_exp);
            
            if(!IsBoss)
                reward_capacity.Add(spirit.reward_capacity);

            exp_lvl_max.Sort();
            exp_up_rate.Sort();
            base_attack.Sort();
            max_attack.Sort();
            base_defense.Sort();
            max_defense.Sort();
            battle_exp.Sort();
            reward_capacity.Sort();
        }


        public uint GetRandomExpMax()
        {
            var rand = new Random();
            return Convert.ToUInt32(Math.Round((rand.NextDouble() * 
                (exp_lvl_max[exp_lvl_max.Count - 1] - exp_lvl_max[0])) + 
                exp_lvl_max[0]));
        }


        public float GetRandomLevelRate()
        {
            var rand = new Random();
            return Convert.ToSingle((rand.NextDouble() *
                (exp_up_rate[exp_up_rate.Count - 1] - exp_up_rate[0])) +
                exp_up_rate[0]);
        }


        public short GetRandomBaseAtk()
        {
            var rand = new Random();
            return Convert.ToInt16(Math.Round((rand.NextDouble() *
                (base_attack[base_attack.Count - 1] - base_attack[0])) +
                base_attack[0]));
        }


        public short GetRandomMaxAtk()
        {
            var rand = new Random();
            return Convert.ToInt16(Math.Round((rand.NextDouble() *
                (max_attack[base_attack.Count - 1] - max_attack[0])) +
                max_attack[0]));
        }


        public short GetRandomBaseDef()
        {
            var rand = new Random();
            return Convert.ToInt16(Math.Round((rand.NextDouble() *
                (base_defense[base_defense.Count - 1] - base_defense[0])) +
                base_defense[0]));
        }


        public short GetRandomMaxDef()
        {
            var rand = new Random();
            return Convert.ToInt16(Math.Round((rand.NextDouble() *
                (max_defense[base_defense.Count - 1] - max_defense[0])) +
                max_defense[0]));
        }


        public uint GetRandomEXPReward()
        {
            var rand = new Random();
            return Convert.ToUInt32(Math.Round((rand.NextDouble() *
                (battle_exp[battle_exp.Count - 1] - battle_exp[0])) +
                battle_exp[0]));
        }


        public uint GetRandomSPReward()
        {
            var rand = new Random();
            return Convert.ToUInt32(Math.Round((rand.NextDouble() *
                (reward_capacity[reward_capacity.Count - 1] - reward_capacity[0])) +
                reward_capacity[0]));
        }
    }

}