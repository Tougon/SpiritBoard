using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using paracobNET;
using MsbtEditor;
using ImageMagick;
using System.Diagnostics;

namespace SpiritBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Point at which Event Spirits begin. Used for autonumbering.
        private const ushort MAX_EDITABLE_INDEX = 2500;
        private int version = 1;
        private int customSpiritsFirstIndex = 1514;
        private AppSaveData saveData;
        private bool DevMode = false;

        // Dictionary representing all base spirit data (before mods are added)
        private Dictionary<ulong, Spirit> baseSpirits = new Dictionary<ulong, Spirit>(1500);
        private Dictionary<int, SpiritButton> spiritButtons = new Dictionary<int, SpiritButton>(1500);
        private Dictionary<int, SpiritStatsAverage> spiritStats = new Dictionary<int, SpiritStatsAverage>();

        // Dictionary representing all default MSBT names
        private Dictionary<string, string> msbtNames = new Dictionary<string, string>(1500);
        private Dictionary<int, ulong> saveToSpiritID = new Dictionary<int, ulong>(1500);

        // Current spirit displayed
        private Spirit currentSpirit;
        private Dictionary<int, Spirit> modList = new Dictionary<int, Spirit>(1500);
        private Dictionary<int, Spirit> unsavedEdits = new Dictionary<int, Spirit>(1500);
        private bool handleChanges = true;
        private bool building = false;
        private bool bootFail = false;

        // World of Light data
        private List<WoLMap> WoLMaps = new List<WoLMap>();
        private Dictionary<ulong, List<WoLSpace>> WoLSpaces = new Dictionary<ulong, List<WoLSpace>>();
        private List<int> WoLSpirits = new List<int>();
        private List<int> DefWoLData = new List<int>();
        //private List<WoLSpirit> DefaultWoLSpirits = new List<WoLSpirit>();
        //private List<WoLSpirit> DefaultWoLFighters = new List<WoLSpirit>();

        private string CurrentPreviewSource = "";
        private BitmapSource CurrentPreview;
        private BitmapSource spirits_2;
        private Image CurrentFullPreviewImage;
        private Image CurrentHeadPreviewImage;
        private Image CurrentSpirits1PreviewImage;
        private List<Image> Stars;
        float CurrentImageScale = 1;
        float CurrentImageHeadScale = 1;

        public class StringPair
        {
            public string name;
            public string name2;
        }

        public MainWindow()
        {
            InitializeComponent();

            // Load saved data
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            // Get the directory
            string path = AppDomain.CurrentDomain.BaseDirectory + "Data";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(path + @"/mods.json"))
            {
                using (StreamReader sr = new StreamReader(path + @"/mods.json"))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    modList = serializer.Deserialize<Dictionary<int, Spirit>>(reader);
                }
            }

            if (File.Exists(path + @"/wollist.json"))
            {
                using (StreamReader sr = new StreamReader(path + @"/wollist.json"))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    WoLSpirits = serializer.Deserialize<List<int>>(reader);
                }
            }

            DevMode = File.Exists(path + @"/tugn.json");

            if (File.Exists(path + @"/randomizer.json"))
            {
                using (StreamReader sr = new StreamReader(path + @"/randomizer.json"))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    var RandoSave = serializer.Deserialize<WoLRandomizerSaveData>(reader);

                    Rand_Use_Seed.IsChecked = RandoSave.bUseSeed;
                    Rand_Seed.Value = RandoSave.Seed;
                    Rand_Replace_Chest.IsChecked = RandoSave.bReplaceChests;
                    Rand_ShuffleBosses.IsChecked = RandoSave.bRandomizeBosses;
                    Rand_ShuffleMasters.IsChecked = RandoSave.bRandomizeMasters;
                    Rand_Add_Hazard_Key.IsChecked = RandoSave.bHazardKey;
                    Rand_DifficultFighters.IsChecked = RandoSave.bDataOrg;
                    Rand_DisableSummons.IsChecked = RandoSave.bDisableSummons;
                    Rand_AutoDLC.IsChecked = RandoSave.bDLCAuto;
                    Rand_Final_Legend.IsChecked = RandoSave.bLegendFinal;
                    Rand_Quiz.IsChecked = RandoSave.bRandomQuiz;
                    Rand_Boss_Kill.IsChecked = RandoSave.bRandomBoss;
                    Rand_Type.SelectedIndex = RandoSave.Mode;
                    Rand_Prioritize_Support.IsChecked = RandoSave.bSupport;
                    Rand_Prioritize_Legend.IsChecked = RandoSave.bLegend;
                    Rand_Retain_Chest_Items.IsChecked = RandoSave.bRetainChest;
                    Rand_RankMin.SelectedIndex = RandoSave.RankLimit;
                    Rand_OrbAmount.Value = RandoSave.SphereCount;

                    Plant_Owned.IsChecked = RandoSave.bPlant;
                    Joker_Owned.IsChecked = RandoSave.bJoker;
                    Hero_Owned.IsChecked = RandoSave.bHero;
                    Banjo_Owned.IsChecked = RandoSave.bBanjo;
                    Terry_Owned.IsChecked = RandoSave.bTerry;
                    Byleth_Owned.IsChecked = RandoSave.bByleth;
                    MinMin_Owned.IsChecked = RandoSave.bMinMin;
                    Steve_Owned.IsChecked = RandoSave.bSteve;
                    Sephiroth_Owned.IsChecked = RandoSave.bSephiroth;
                    PyraMythra_Owned.IsChecked = RandoSave.bPyra;
                    Kazuya_Owned.IsChecked = RandoSave.bKazuya;
                    Sora_Owned.IsChecked = RandoSave.bSora;
                }
            }


            path = AppDomain.CurrentDomain.BaseDirectory + "Override";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                if (!Directory.Exists(path + "/Stage")) Directory.CreateDirectory(path + "/Stage");
                if (!Directory.Exists(path + "/Stock")) Directory.CreateDirectory(path + "/Stock");
            }

            path = AppDomain.CurrentDomain.BaseDirectory + "Images";

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            LoadApplicationData();

            if (bootFail) return;

            LoadSpiritParams();

            if (bootFail) return;

            // v2 Update - Makes sure all mods have the proper aesthetic data to make sure builds work.
            foreach (var mod in modList)
            {
                if((mod.Value.aesthetics == null || mod.Value.version < 3) &&
                    baseSpirits.ContainsKey(mod.Value.spirit_id))
                {
                    mod.Value.version = 3;
                    mod.Value.aesthetics = baseSpirits[mod.Value.spirit_id].aesthetics.Copy();
                }

                if(mod.Value.version < 4 && baseSpirits.ContainsKey(mod.Value.spirit_id))
                {
                    mod.Value.version = 4;
                    mod.Value.type = baseSpirits[mod.Value.spirit_id].type;
                }

                // 13.0.1 messes with the indexes of battles...again
                if (mod.Value.version < 7 && baseSpirits.ContainsKey(mod.Value.spirit_id))
                {
                    mod.Value.version = 7;

                    mod.Value.index = baseSpirits[mod.Value.spirit_id].index;
                    mod.Value.battle.index = baseSpirits[mod.Value.spirit_id].battle.index;
                    mod.Value.battle.fighterDBIndexes = baseSpirits[mod.Value.spirit_id].battle.fighterDBIndexes;

                    if (mod.Value.alternateBattle != null)
                    {
                        mod.Value.alternateBattle.index = baseSpirits[mod.Value.spirit_id].alternateBattle.index;
                        mod.Value.alternateBattle.fighterDBIndexes = baseSpirits[mod.Value.spirit_id].alternateBattle.fighterDBIndexes;
                    }

                    mod.Value.aesthetics.index = baseSpirits[mod.Value.spirit_id].aesthetics.index;
                }

                if(mod.Value.Custom)
                {
                    if (mod.Value.aesthetics.layout_id != mod.Value.spirit_id)
                        mod.Value.aesthetics.layout_id = mod.Value.spirit_id;
                }

                mod.Value.version = 8;

                if (!saveToSpiritID.ContainsKey(mod.Value.save_no))
                {
                    saveToSpiritID.Add(mod.Value.save_no, mod.Value.spirit_id);
                }

                if (!SpiritValueDisplay.Spirits.ContainsKey(mod.Value.spirit_id))
                {
                    mod.Value.Custom = true;
                    mod.Value.battle.index = -1;
                    mod.Value.aesthetics.index = -1;

                    for(int i=0; i<mod.Value.battle.fighterDBIndexes.Count; i++)
                    {
                        mod.Value.battle.fighterDBIndexes[i] = -1;
                    }

                    SpiritValueDisplay.Spirits.Add(mod.Value.spirit_id,
                        new SpiritValueDisplay.SpiritID(mod.Value.spirit_id, mod.Value.display_name));
                }
            }

            LoadMapParams();
            ImportDLCBoards();

            // Add controls and build the GUI
            AddApplicationControlSpiritTab();
            AddApplicationControlBattleTab();
            AddApplicationControl();

            // Default the Layout tab to 0. There's not really a good place for this.
            Image_Pos_Type_Desc_Block.Text = "Used for the Spirit Board thumbnails and the Party Screen.";
        }


        private void LoadApplicationData()
        {
            // Get the directory
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Cannot boot. Resources directory is missing.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            // Load in the CSV file representation of all spirit abilities
            if (!File.Exists(path + @"AppData\Abilities.csv"))
            {
                MessageBox.Show("Cannot boot. Abilities.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            var abilityReader = new StreamReader(path + @"AppData\Abilities.csv");

            while (!abilityReader.EndOfStream)
            {
                var line = abilityReader.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                SpiritValueDisplay.SpiritAbilities.Add(Convert.ToUInt64(values[0]), new SpiritValueDisplay.SpiritAbility
                    (Convert.ToUInt64(values[0]),values[1].Replace(@"""", ""), values[2].Replace(@"""", "")));
            }

            abilityReader.Close();


            // Load in the Spirit MSBT
            if (!File.Exists(path + "msg_spirits.MSBT"))
            {
                MessageBox.Show("Cannot boot. msg_spirits.MSBT is missing from Resources.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var spiritMSBT = new MSBT(path + "msg_spirits.MSBT");

            for (int i = 0; i < spiritMSBT.LBL1.Labels.Count; i++)
            {
                var bytes =
                    ((MsbtEditor.Label)spiritMSBT.LBL1.Labels[i]).String.Value;

                // spiritMSBT.LBL1.Labels[i].ToString() gets us the key, spi_ followed by name
                msbtNames.Add(spiritMSBT.LBL1.Labels[i].ToString(),
                    BytesToDisplayName(bytes));
            }

            // Search for application saved data
            string dataPath = AppDomain.CurrentDomain.BaseDirectory + "Data";
            string bgmPath = "";
            saveData = new AppSaveData();

            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            if (File.Exists(dataPath + @"\app_data.json"))
            {
                JsonSerializer deserializer = new JsonSerializer();
                deserializer.NullValueHandling = NullValueHandling.Ignore;

                using (StreamReader sr = new StreamReader(dataPath + @"\app_data.json"))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    saveData = deserializer.Deserialize<AppSaveData>(reader);
                    bgmPath = saveData.bgmPath;
                }
            }
            else
            {
                MessageBox.Show("Before we begin, please choose a directory to load BGM data from.\n" +
                    "If you use Sm5shMusic, it is recommended to choose ArcOutput/ui.\n\n"+
                    "If not, make sure your folder structure matches the following:\n" +
                    "[YOUR PATH]/message/msg_bgm+us_en\n" +
                    "[YOUR PATH]/message/msg_title+us_en.MSBT\n" +
                    "[YOUR PATH]/param/database/ui_bgm_db.prc", "Hold Up");

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = path;
                dialog.IsFolderPicker = true;
                bool selectedFolder = false;

                while (!selectedFolder)
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        selectedFolder = true;
                    }
                    else
                    {
                        MessageBox.Show("Please choose a BGM directory.", "Hold Up");
                    }
                }

                saveData.bgmPath = dialog.FileName;
                bgmPath = dialog.FileName;
            }

            // Load in the game titles
            if (!File.Exists(bgmPath + @"\message\msg_title+us_en.MSBT"))
            {
                MessageBox.Show("Cannot boot. msg_title+us_en.MSBT is missing from your target path.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var gameMSBT = new MSBT(bgmPath + @"\message\msg_title+us_en.MSBT");
            // Temp array so the names can be sorted alphabetically
            var gameTitles = new List<SpiritValueDisplay.SpiritGameTitle>();

            for (int i = 0; i < gameMSBT.LBL1.Labels.Count; i++)
            {
                string key = gameMSBT.LBL1.Labels[i].ToString();
                string game = key.Substring(4, key.Length - 4);

                if (game.Contains("append"))
                {
                    continue;
                }
                // Only add the game title to the list if it is a game title
                else if (game.Length < 6 || !game.Substring(0, 6).Equals("series"))
                {
                    string formattedTitle = ((MsbtEditor.Label)gameMSBT.LBL1.Labels[i]).String.ToString(gameMSBT.FileEncoding);

                    var hash = Hash40Util.StringToHash40("ui_gametitle_" + game);
                    gameTitles.Add(new SpiritValueDisplay.SpiritGameTitle(hash, formattedTitle));
                }
            }


            // 198118BD66 - ds_brain_age - Brain Age: Train Your Brain in Minutes a Day! Series
            // 1ED2BE13F0 - mario_golf_series - Mario Golf Series
            gameTitles.Add(new SpiritValueDisplay.SpiritGameTitle(109540064614,
                "Brain Age: Train Your Brain in Minutes a Day! Series"));
            gameTitles.Add(new SpiritValueDisplay.SpiritGameTitle(132384691184, "Mario Golf Series"));

            // Sort the titles and inject
            gameTitles.Sort();
            gameTitles.Insert(0, new SpiritValueDisplay.SpiritGameTitle(84604977966, "Event Spirits"));
            gameTitles.Insert(0, new SpiritValueDisplay.SpiritGameTitle(73502405852, "None"));

            foreach (var title in gameTitles)
            {
                SpiritValueDisplay.SpiritGameTitles.Add(title.id, title);
            }

            ParamFile ui_series_db = new ParamFile();
            ui_series_db.Open(bgmPath + @"\param\database\ui_series_db.prc");

            var seriesDB = ((ParamList)ui_series_db.Root.Nodes["db_root"]);

            foreach (var series in seriesDB.Nodes)
            {
                var data = ((ParamStruct)series).Nodes;
                ulong seriesID = Convert.ToUInt64(((ParamValue)data["ui_series_id"]).Value);

                if (!SpiritValueDisplay.SpiritSeries.ContainsKey(seriesID))
                {
                    string seriesName = Convert.ToString(((ParamValue)data["name_id"]).Value);

                    if(!seriesName.Equals("none") && !seriesName.Equals("random") && !seriesName.Equals("mymusic")
                        && !seriesName.Equals("all"))
                    {
                        var lbl = (MsbtEditor.Label)
                            gameMSBT.LBL1.Labels.Find((l) => ((MsbtEditor.Label)l).Name.Equals("tit_series_snd_" + seriesName));

                        if (lbl != null)
                        {
                            string result = BytesToDisplayName(lbl.String.Value);

                            SpiritValueDisplay.SpiritSeries.Add(seriesID,
                                new SpiritValueDisplay.SpiritSeriesID(seriesID, result, "ui_series_" + seriesName));
                        }
                    }
                }
            }


            // Load in BGM data
            if (!File.Exists(bgmPath + @"\param\database\ui_bgm_db.prc"))
            {
                MessageBox.Show("Cannot boot. ui_bgm_db.prc is missing from your target path.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            if (!File.Exists(bgmPath + @"\message\msg_bgm+us_en.MSBT"))
            {
                MessageBox.Show("Cannot boot. msg_bgm+us_en.MSBT is missing from your target path.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var bgmMSBT = new MSBT(bgmPath + @"\message\msg_bgm+us_en.MSBT");

            ParamFile ui_bgm_db = new ParamFile();
            ui_bgm_db.Open(bgmPath + @"\param\database\ui_bgm_db.prc");

            var bgmDB = ((ParamList)ui_bgm_db.Root.Nodes["db_root"]);
            List<SpiritValueDisplay.BattleBGM> bgmList = new List<SpiritValueDisplay.BattleBGM>(2000);

            foreach (var bgm in bgmDB.Nodes)
            {
                var data = ((ParamStruct)bgm).Nodes;

                string bgmName = Convert.ToString(((ParamValue)data["name_id"]).Value);

                var lbl = (MsbtEditor.Label)
                    bgmMSBT.LBL1.Labels.Find((l) => ((MsbtEditor.Label)l).Name.Equals("bgm_title_" + bgmName));

                if (lbl != null)
                {
                    ulong id = Convert.ToUInt64(((ParamValue)data["ui_bgm_id"]).Value);

                    string result = BytesToDisplayName(lbl.String.Value);

                    if (DevMode && result.Contains("ZZZZZ"))
                        continue;

                    ulong record = Convert.ToUInt64(((ParamValue)data["record_type"]).Value);

                    if (record == 60469123532)
                    {
                        result += " (Remix)";
                    }
                    else if (record == 80301084129)
                    {
                        result += " (New Remix)";
                    }

                    bgmList.Add(new SpiritValueDisplay.BattleBGM(id, result));
                }
            }

            bgmList.Sort();
            SpiritValueDisplay.BattleBGMIDs.Add(56413952268, new SpiritValueDisplay.BattleBGM(56413952268, "Random"));

            foreach(var bgm in bgmList)
            {
                SpiritValueDisplay.BattleBGMIDs.Add(bgm.id, bgm);
            }

            bgmList.Clear();


            // Save BGM Directory
            if (!File.Exists(dataPath + @"\app_data.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                // Export to JSON
                using (StreamWriter sw = new StreamWriter(dataPath + @"/app_data.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, saveData);
                }
            }


            // Load in the CSV file representation of all stage data
            if (!File.Exists(path + @"AppData\Stage ID.csv"))
            {
                MessageBox.Show("Cannot boot. Stage ID.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            var stageReader = new StreamReader(path + @"AppData\Stage ID.csv");

            while (!stageReader.EndOfStream)
            {
                var line = stageReader.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                var forms = values[2].Split('|');
                var names = values[3].Split('|');
                int[] formIDs = new int[forms.Length];
                string[] formNames = new string[forms.Length];

                for (int i = 0; i < forms.Length; i++)
                {
                    formIDs[i] = int.Parse(forms[i]);
                    formNames[i] = names[i];
                }

                SpiritValueDisplay.BattleStageIDs.Add(Convert.ToUInt64(values[0]), new SpiritValueDisplay.BattleStageID
                    (Convert.ToUInt64(values[0]), values[1].Replace(@"""", ""), formIDs, formNames));
            }

            stageReader.Close();


            // Load in the CSV file representation of all fighter data
            if (!File.Exists(path + @"AppData\Fighter ID.csv"))
            {
                MessageBox.Show("Cannot boot. Fighter ID.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            var fighterReader = new StreamReader(path + @"AppData\Fighter ID.csv");

            while (!fighterReader.EndOfStream)
            {
                var line = fighterReader.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                SpiritValueDisplay.FighterIDs.Add(Convert.ToUInt64(values[0]), new SpiritValueDisplay.FighterID
                    (Convert.ToUInt64(values[0]), values[1], values[2], Convert.ToBoolean(values[3])));
            }

            fighterReader.Close();


            // Load in the CSV file representation of all battle event data
            if (!File.Exists(path + @"AppData\Battle Events.csv"))
            {
                MessageBox.Show("Cannot boot. Battle Events.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            var eventReader = new StreamReader(path + @"AppData\Battle Events.csv");

            while (!eventReader.EndOfStream)
            {
                var line = eventReader.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                SpiritValueDisplay.BattleEvents.Add(Convert.ToUInt64(values[0]), new SpiritValueDisplay.BattleEvent
                    (Convert.ToUInt64(values[0]), values[1].Replace(@"""", ""), values[2].Replace(@"""", ""),
                    Convert.ToInt32(values[3])));
            }

            eventReader.Close();


            // Load in the CSV file representation of all event params
            if (!File.Exists(path + @"AppData\Battle Event Params.csv"))
            {
                MessageBox.Show("Cannot boot. Battle Event Params.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }
            eventReader = new StreamReader(path + @"AppData\Battle Event Params.csv");

            while (!eventReader.EndOfStream)
            {
                var line = eventReader.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                var ids = values[3].Split('|');

                foreach (var i in ids)
                {
                    ulong id = Convert.ToUInt64(i);

                    if (SpiritValueDisplay.BattleEvents.ContainsKey(id))
                    {
                        SpiritValueDisplay.BattleEvents[id].eventParams.Add(Convert.ToUInt64(values[0]),
                            new SpiritValueDisplay.BattleEventParam(Convert.ToUInt64(values[0]),
                            values[1].Replace(@"""", ""), values[2].Replace(@"""", "")));
                    }
                }
            }

            eventReader.Close();


            // Load in the CSV file representation of AI data
            if (!File.Exists(path + @"AppData\CPU Types.csv"))
            {
                MessageBox.Show("Cannot boot. CPU Types.csv is missing from Resources/AppData.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            var fighterBehaviors = new StreamReader(path + @"AppData\CPU Types.csv");

            while (!fighterBehaviors.EndOfStream)
            {
                var line = fighterBehaviors.ReadLine();
                // Some descriptions have commas, so we split with Regex
                var values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                SpiritValueDisplay.CPUBehaviors.Add(Convert.ToUInt64(values[1]), new SpiritValueDisplay.CPUBehavior
                    (Convert.ToUInt64(values[1]), values[0].Replace(@"""", "")));
            }

            fighterBehaviors.Close();
        }

        #region Spirit Editor Functions

        private void LoadSpiritParams()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";

            // Read the spirit params
            if (!File.Exists(path + @"ui_spirit_db.prc"))
            {
                MessageBox.Show("Cannot boot. ui_spirit_db.prc is missing from Resources.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            ParamFile ui_spirit_db = new ParamFile();
            ui_spirit_db.Open(path + @"ui_spirit_db.prc");

            var spiritDB = ((ParamList)ui_spirit_db.Root.Nodes["db_root"]);

            version = Convert.ToInt32(((ParamValue)ui_spirit_db.Root.Nodes["data_version"]).Value);

            // Spirit list is not sorted. We may want to sort it?
            SpiritValueDisplay.Spirits.Add(19320013007, new SpiritValueDisplay.SpiritID(19320013007, "None"));
            SpiritValueDisplay.Spirits.Add(44827135837, new SpiritValueDisplay.SpiritID(44827135837, "Any Attack Core"));
            SpiritValueDisplay.Spirits.Add(44095001115, new SpiritValueDisplay.SpiritID(44095001115, "Any Shield Core"));
            SpiritValueDisplay.Spirits.Add(43877750031, new SpiritValueDisplay.SpiritID(43877750031, "Any Grab Core"));
            SpiritValueDisplay.Spirits.Add(50718054494, new SpiritValueDisplay.SpiritID(50718054494, "Any Neutral Core"));
            SpiritValueDisplay.Spirits.Add(45518778546, new SpiritValueDisplay.SpiritID(45518778546, "Any Support Core"));
            SpiritValueDisplay.SummonSpirits.Add(44827135837, new SpiritValueDisplay.SpiritID(44827135837, "Any Attack Core"));
            SpiritValueDisplay.SummonSpirits.Add(44095001115, new SpiritValueDisplay.SpiritID(44095001115, "Any Shield Core"));
            SpiritValueDisplay.SummonSpirits.Add(43877750031, new SpiritValueDisplay.SpiritID(43877750031, "Any Grab Core"));
            SpiritValueDisplay.SummonSpirits.Add(50718054494, new SpiritValueDisplay.SpiritID(50718054494, "Any Neutral Core"));
            SpiritValueDisplay.SummonSpirits.Add(45518778546, new SpiritValueDisplay.SpiritID(45518778546, "Any Support Core"));

            Dictionary<ulong, Spirit> hasAlternateBattle = new Dictionary<ulong, Spirit>();
            List<Spirit> unsortedSpirits = new List<Spirit>(1500);
            List<SpiritValueDisplay.SpiritID> unsortedSpiritIDs = new List<SpiritValueDisplay.SpiritID>(1500);

            if (spiritDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < spiritDB.Nodes.Count; i++)
                {
                    var node = spiritDB.Nodes[i];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var spirit = new Spirit((ParamStruct)node);
                        spirit.index = i;
                        spirit.display_name = msbtNames["spi_" + spirit.name_id];

                        if(spirit.save_no >= customSpiritsFirstIndex)
                        {
                            customSpiritsFirstIndex = spirit.save_no + 1;
                        }

                        unsortedSpirits.Add(spirit);
                        unsortedSpiritIDs.Add(new SpiritValueDisplay.SpiritID(spirit.spirit_id, spirit.display_name));

                        if (!saveToSpiritID.ContainsKey(spirit.save_no))
                            saveToSpiritID.Add(spirit.save_no, spirit.spirit_id);

                        // Fix for YesWeDo edited spirits
                        if (spirit.x115044521b != 0 && spirit.x115044521b != 33451952054)
                        {
                            hasAlternateBattle.Add(spirit.x115044521b, spirit);
                        }
                    }
                }
            }

            unsortedSpirits.Sort();
            foreach (var spirit in unsortedSpirits)
                baseSpirits.Add(spirit.spirit_id, spirit);

            unsortedSpiritIDs.Sort();

            foreach(var spirit in unsortedSpiritIDs)
                SpiritValueDisplay.Spirits.Add(spirit.id, spirit);

            unsortedSpiritIDs.Clear();

            SpiritValueDisplay.Spirits.Add(8403017505, new SpiritValueDisplay.SpiritID(8403017505, "Nothing"));


            // Read the battle params
            if (!File.Exists(path + @"ui_spirits_battle_db.prc"))
            {
                MessageBox.Show("Cannot boot. ui_spirits_battle_db.prc is missing from Resources.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            ParamFile ui_spirit_battle_db = new ParamFile();
            ui_spirit_battle_db.Open(path + @"ui_spirits_battle_db.prc");

            var battleDB = ((ParamList)ui_spirit_battle_db.Root.Nodes["battle_data_tbl"]);
            var fighterDB = ((ParamList)ui_spirit_battle_db.Root.Nodes["fighter_data_tbl"]);

            if (battleDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < battleDB.Nodes.Count; i++)
                {
                    var node = battleDB.Nodes[i];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var battle = new SpiritBattle((ParamStruct)node);
                        battle.index = i;

                        if (baseSpirits.ContainsKey(battle.battle_id))
                        {
                            baseSpirits[battle.battle_id].battle = battle;
                        }
                        else if (hasAlternateBattle.ContainsKey(battle.battle_id))
                        {
                            hasAlternateBattle[battle.battle_id].alternateBattle = battle;
                        }
                    }
                }
            }

            // Read fighter params
            if (fighterDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < fighterDB.Nodes.Count; i++)
                {
                    var node = fighterDB.Nodes[i];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var fighter = new SpiritBattleFighter((ParamStruct)node);

                        if (baseSpirits.ContainsKey(fighter.battle_id) && baseSpirits[fighter.battle_id].battle != null)
                        {
                            baseSpirits[fighter.battle_id].battle.fighters.Add(fighter);
                            baseSpirits[fighter.battle_id].battle.fighterDBIndexes.Add(i);
                        }
                        else if (hasAlternateBattle.ContainsKey(fighter.battle_id))
                        {
                            hasAlternateBattle[fighter.battle_id].alternateBattle.fighters.Add(fighter);
                            hasAlternateBattle[fighter.battle_id].alternateBattle.fighterDBIndexes.Add(i);
                        }
                    }
                }
            }

            // Read the layout params
            if (!File.Exists(path + @"ui_spirit_layout_db.prc"))
            {
                MessageBox.Show("Cannot boot. ui_spirit_layout_db.prc is missing from Resources.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            ParamFile ui_spirit_layout_db = new ParamFile();
            ui_spirit_layout_db.Open(path + @"ui_spirit_layout_db.prc");

            var layoutDB = ((ParamList)ui_spirit_layout_db.Root.Nodes["db_root"]);

            if (layoutDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < layoutDB.Nodes.Count; i++)
                {
                    var node = layoutDB.Nodes[i];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var layout = new SpiritAesthetics((ParamStruct)node);
                        layout.index = i;

                        if (baseSpirits.ContainsKey(layout.layout_id))
                        {
                            baseSpirits[layout.layout_id].aesthetics = layout;
                        }
                    }
                }
            }
        }


        public void AddApplicationControl()
        {
            var brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 0.0));
            brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.25));
            brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.75));
            brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 1.0));

            bool bInitializeWoL = WoLSpirits.Count == 0;

            Spirit DefaultSpirit = null;

            // Add application controls
            foreach (var spirit in baseSpirits.Values)
            {
                if (DefaultSpirit == null)
                    DefaultSpirit = spirit;

                SpiritButton newBtn = new SpiritButton();

                newBtn.Content = modList.ContainsKey(spirit.index) ? 
                    $"{modList[spirit.index].display_name}" : 
                    spirit.display_name;
                if(modList.ContainsKey(spirit.index))
                    newBtn.FontWeight = FontWeights.Bold;
                newBtn.Name = "Button_" + spirit.name_id;
                newBtn.Background = brush;
                newBtn.spirit = spirit;
                newBtn.Click += (s, e) => {
                    DisplaySpirit(newBtn.spirit);
                };

                spiritButtons.Add(spirit.index, newBtn);
                SpiritList.Children.Add(newBtn);

                if(bInitializeWoL && spirit.type != 83372998134 && spirit.rematch)
                    WoLSpirits.Add(spirit.save_no);

                var tgnType = spirit.GetSpiritType();

                if (!spiritStats.ContainsKey(tgnType))
                {
                    spiritStats.Add(tgnType, new SpiritStatsAverage());
                }

                bool boss = false;

                foreach(var fighter in spirit.battle.fighters)
                {
                    if(fighter.entry_type == 42324224621)
                    {
                        boss = true;
                        break;
                    }
                }

                spiritStats[tgnType].AddSpirit(spirit, boss);
            }

            foreach (var spirit in modList.Values)
            {
                if (spirit.Custom)
                {
                    SpiritButton newBtn = new SpiritButton();

                    newBtn.Content = modList.ContainsKey(spirit.index) ?
                        $"{modList[spirit.index].display_name}" :
                        spirit.display_name;
                    if (modList.ContainsKey(spirit.index))
                        newBtn.FontWeight = FontWeights.Bold;
                    newBtn.Name = "Button_" + spirit.name_id;
                    newBtn.Background = brush;
                    newBtn.spirit = spirit;
                    newBtn.Click += (s, e) => {
                        DisplaySpirit(newBtn.spirit);
                    };

                    spiritButtons.Add(spirit.index, newBtn);
                    SpiritList.Children.Add(newBtn);

                    if (bInitializeWoL && spirit.type != 83372998134 && spirit.rematch)
                        WoLSpirits.Add(spirit.save_no);
                }
            }

            Alt_Battle_Button.Click += (s, e) => {
                var mod = GetMods(currentSpirit);

                mod.displayingAltBattle = !mod.displayingAltBattle;

                if (mod.displayingAltBattle)
                {
                    handleChanges = false;
                    DisplayBattle(mod.alternateBattle);
                    Alt_Battle_Button.Content = "Switch to Base Battle";
                    Battle_Header.Content = currentSpirit.display_name + " Battle (DLC Version)";
                    handleChanges = true;
                }
                else
                {
                    handleChanges = false;
                    DisplayBattle(mod.battle);
                    Alt_Battle_Button.Content = "Switch to DLC Battle";
                    Battle_Header.Content = currentSpirit.display_name + " Battle";
                    handleChanges = true;
                }
            };

            Spirit_Type.SelectionChanged += UpdateSpirit;
            Spirit_Rank.SelectionChanged += UpdateSpirit;
            Spirit_Attribute.SelectionChanged += UpdateSpirit;
            Spirit_Series.SelectionChanged += UpdateSpirit;
            Spirit_Ability.SelectionChanged += UpdateSpirit;
            Spirit_SuperAbility.SelectionChanged += UpdateSpirit;
            Spirit_BoardConditions.SelectionChanged += UpdateSpirit;
            Spirit_SaleType.SelectionChanged += UpdateSpirit;
            Spirit_Game_ID.SelectionChanged += UpdateSpirit;
            Spirit_Evolves_From.SelectionChanged += UpdateSpirit;
            Spirit_Core_1_ID.SelectionChanged += UpdateSpirit;
            Spirit_Core_2_ID.SelectionChanged += UpdateSpirit;
            Spirit_Core_3_ID.SelectionChanged += UpdateSpirit;
            Spirit_Core_4_ID.SelectionChanged += UpdateSpirit;
            Spirit_Core_5_ID.SelectionChanged += UpdateSpirit;

            Spirit_Name.TextChanged += UpdateSpirit;
            Spirit_Index.ValueChanged += UpdateSpirit;
            Spirit_Slots.ValueChanged += UpdateSpirit;

            Spirit_Base_Atk.ValueChanged += UpdateSpirit;
            Spirit_Base_Def.ValueChanged += UpdateSpirit;
            Spirit_Max_Atk.ValueChanged += UpdateSpirit;
            Spirit_Max_Def.ValueChanged += UpdateSpirit;
            Spirit_Exp.ValueChanged += UpdateSpirit;
            Spirit_Lvl_Rate.ValueChanged += UpdateSpirit;
            Spirit_Reward_EXP.ValueChanged += UpdateSpirit;
            Spirit_Reward_SP.ValueChanged += UpdateSpirit;
            Spirit_Price.ValueChanged += UpdateSpirit;
            Spirit_SummonSP.ValueChanged += UpdateSpirit;
            Spirit_Core_1_Quantity.ValueChanged += UpdateSpirit;
            Spirit_Core_2_Quantity.ValueChanged += UpdateSpirit;
            Spirit_Core_3_Quantity.ValueChanged += UpdateSpirit;
            Spirit_Core_4_Quantity.ValueChanged += UpdateSpirit;
            Spirit_Core_5_Quantity.ValueChanged += UpdateSpirit;

            Spirit_Is_Rematch.Click += UpdateSpirit;
            Spirit_OnBoard.Click += UpdateSpirit;
            Spirit_Hint1.Click += UpdateSpirit;
            Spirit_Hint2.Click += UpdateSpirit;
            Spirit_Hint3.Click += UpdateSpirit;

            Spirit_Is_In_WoL.Click += (s, e) => {
                var mod = GetMods(currentSpirit);

                if(Spirit_Is_In_WoL.IsChecked != null && Spirit_Is_In_WoL.IsChecked.Value)
                {
                    if (!WoLSpirits.Contains(mod.save_no))
                    {
                        WoLSpirits.Add(mod.save_no);
                    }
                }
                else
                {
                    if (WoLSpirits.Contains(mod.save_no))
                    {
                        WoLSpirits.Remove(mod.save_no);

                        if (WoLSpirits.Count < WoLMap.WoLSpiritsMinThreshold)
                        {
                            MessageBox.Show("Your pool of spirits is too small. The randomizer will not be balanced.",
                                "Hold Up");
                        }
                    }
                }
            };

            Battle_Type.SelectionChanged += UpdateSpirit;
            Battle_Win_Condition.SelectionChanged += UpdateSpirit;
            Battle_Power.ValueChanged += UpdateSpirit;
            Battle_Time.ValueChanged += UpdateSpirit;
            Battle_Stock.ValueChanged += UpdateSpirit;
            Battle_Init_HP.ValueChanged += UpdateSpirit;
            Battle_Init_Damage.ValueChanged += UpdateSpirit;

            Battle_Stage_ID.SelectionChanged += UpdateSpirit;
            Battle_Stage_Type.SelectionChanged += UpdateSpirit;
            Battle_Stage_Form.SelectionChanged += UpdateSpirit;
            Battle_Stage_Attr.SelectionChanged += UpdateSpirit;
            Battle_Floor.SelectionChanged += UpdateSpirit;
            Battle_Item_Table.SelectionChanged += UpdateSpirit;
            Battle_Item_Level.SelectionChanged += UpdateSpirit;
            Battle_BGM.SelectionChanged += UpdateSpirit;
            Battle_Hazards.Click += UpdateSpirit;

            Event_1_Type.SelectionChanged += UpdateSpirit;
            Event_1_Label.SelectionChanged += UpdateSpirit;
            Event_1_Start_Time.ValueChanged += UpdateSpirit;
            Event_1_Range_Time.ValueChanged += UpdateSpirit;
            Event_1_Count.ValueChanged += UpdateSpirit;
            Event_1_Damage.ValueChanged += UpdateSpirit;

            Event_2_Type.SelectionChanged += UpdateSpirit;
            Event_2_Label.SelectionChanged += UpdateSpirit;
            Event_2_Start_Time.ValueChanged += UpdateSpirit;
            Event_2_Range_Time.ValueChanged += UpdateSpirit;
            Event_2_Count.ValueChanged += UpdateSpirit;
            Event_2_Damage.ValueChanged += UpdateSpirit;

            Event_3_Type.SelectionChanged += UpdateSpirit;
            Event_3_Label.SelectionChanged += UpdateSpirit;
            Event_3_Start_Time.ValueChanged += UpdateSpirit;
            Event_3_Range_Time.ValueChanged += UpdateSpirit;
            Event_3_Count.ValueChanged += UpdateSpirit;
            Event_3_Damage.ValueChanged += UpdateSpirit;

            Battle_Recommended_Skill.SelectionChanged += UpdateSpirit;
            Battle_Unrecommended_Skill.SelectionChanged += UpdateSpirit;
            Battle_Auto_Win_Skill.SelectionChanged += UpdateSpirit;
            Battle_Auto_Win_Skill_2.SelectionChanged += UpdateSpirit;

            Image_X.ValueChanged += UpdateSpirit;
            Image_Y.ValueChanged += UpdateSpirit;
            Image_Scale.ValueChanged += UpdateSpirit;

            Image_Pos_Type.SelectionChanged += (s, e) => {

                handleChanges = false;

                var mod = GetMods(currentSpirit);
                var layout = mod.aesthetics;

                int imageIndex = Image_Pos_Type.SelectedIndex;

                Image_X.Value = Convert.ToDecimal(layout.art_pos_x[imageIndex]);
                Image_Y.Value = Convert.ToDecimal(layout.art_pos_y[imageIndex]);
                Image_Scale.Value = Convert.ToDecimal(layout.art_size[imageIndex]);

                switch (imageIndex)
                {
                    case 0:
                        Image_Pos_Type_Desc_Block.Text = "Used for the Spirit Board thumbnails and the Party Screen.";
                        break;
                    case 1:
                        Image_Pos_Type_Desc_Block.Text = "Used for the Gallery and Training Screens.";
                        break;
                    case 2:
                        Image_Pos_Type_Desc_Block.Text = "Unknown, but is usually equal to Gallery minus 50 on Y.";
                        break;
                    case 3:
                        Image_Pos_Type_Desc_Block.Text = "Used for the small portrait next to damage meters.";
                        break;
                }

                handleChanges = true;
            };

            Effect_Count.ValueChanged += UpdateSpirit;
            Effect_X.ValueChanged += UpdateSpirit;
            Effect_Y.ValueChanged += UpdateSpirit;

            Effect_Index.SelectionChanged += (s, e) => {

                handleChanges = false;

                var mod = GetMods(currentSpirit);
                var layout = mod.aesthetics;

                int effectIndex = Effect_Index.SelectedIndex;

                Effect_X.Value = layout.effect_pos_x[effectIndex];
                Effect_Y.Value = layout.effect_pos_y[effectIndex];

                handleChanges = true;
            };

            tabControl.SelectionChanged += UpdateRandomizer;

            Chara_1_Scale.ValueChanged += (s, e) =>
            {
                float scale = (float)Chara_1_Scale.Value;
                CurrentSpirits1PreviewImage.LayoutTransform = new ScaleTransform(scale, scale);
                var FullCenter = GetPreviewImageCenter(CurrentSpirits1PreviewImage,
                    scale, Spirit_Preview_Spirits_1_Canvas);
            };

            Reset_Board.Click += (s, e) =>
            {
                var Aesthetics = GetMods(currentSpirit).aesthetics;

                Aesthetics.art_pos_x[0] = 0;
                Aesthetics.art_pos_y[0] = 0;

                if (Image_Pos_Type.SelectedIndex == 0)
                {
                    handleChanges = false;
                    Image_X.Value = Convert.ToDecimal(0);
                    Image_Y.Value = Convert.ToDecimal(0);
                    handleChanges = true;
                }

                UpdateSpirit(s, e);
            };

            Reset_Main.Click += (s, e) =>
            {
                var Aesthetics = GetMods(currentSpirit).aesthetics;

                Aesthetics.art_pos_x[1] = 0;
                Aesthetics.art_pos_y[1] = 0;
                Aesthetics.art_pos_x[2] = 0;
                Aesthetics.art_pos_y[2] = -50;

                for(int i=0; i<15; i++)
                {
                    Aesthetics.effect_pos_x[i] = 0;
                    Aesthetics.effect_pos_y[i] = 0;
                }

                Effect_X.Value = 0;
                Effect_Y.Value = 0;

                if (Image_Pos_Type.SelectedIndex == 1)
                {
                    handleChanges = false;
                    Image_X.Value = Convert.ToDecimal(0);
                    Image_Y.Value = Convert.ToDecimal(0);
                    handleChanges = true;
                }

                UpdateSpirit(s, e);
            };

            Reset_Spirits_1.Click += (s, e) =>
            {
                Chara_1_Scale.Value = 1;
                var FullCenter_1 = GetPreviewImageCenter(CurrentSpirits1PreviewImage,
                    1, Spirit_Preview_Spirits_1_Canvas);
                Canvas.SetLeft(CurrentSpirits1PreviewImage, FullCenter_1.X);
                Canvas.SetTop(CurrentSpirits1PreviewImage, FullCenter_1.Y);
            };

            Randomize_Stats.Click += (s, e) =>
            {
                var Mods = GetMods(currentSpirit);
                var Stats = spiritStats[Mods.GetSpiritType()];

                handleChanges = false;

                Spirit_Exp.Value = (int)Stats.GetRandomExpMax();
                Spirit_Lvl_Rate.Value = (decimal)Stats.GetRandomLevelRate();
                Spirit_Base_Atk.Value = Stats.GetRandomBaseAtk();
                Spirit_Base_Def.Value = Stats.GetRandomBaseDef();
                Spirit_Max_Atk.Value = Stats.GetRandomMaxAtk();
                Spirit_Max_Def.Value = Stats.GetRandomMaxDef();
                Spirit_Reward_EXP.Value = (int)Stats.GetRandomEXPReward();
                Spirit_Reward_SP.Value = (int)Stats.GetRandomSPReward();

                handleChanges = true;
                UpdateSpirit(s, e);
            };

            Stars = new List<Image>();

            for (int i = 0; i < 15; i++)
            {
                Uri starURI = new Uri("/Icons/star.png", UriKind.Relative);
                var Star = new Image { Width = 9, Height = 9, Source = new BitmapImage(starURI) };
                Stars.Add(Star);
            }

            foreach (var Star in Stars)
            {
                Canvas.SetLeft(Star, -100);
                Canvas.SetRight(Star, 100);
                Spirit_Preview_Canvas.Children.Add(Star);
                Panel.SetZIndex(Star, 1);
            }

            // WoL items
            Rand_Build.Click += RandomizeMaps;
            Rand_Regen.Click += RegenerateSeed;

            DisplaySpirit(DefaultSpirit);
        }

        
        private void UpdateRandomizer(object sender, SelectionChangedEventArgs e)
        {
            Randomizer_Header_Desc.Content = $"Available spirits: {WoLSpirits.Count}/{WoLMap.WoLSpiritsMinThreshold}";
            Randomizer_Header_Desc.Foreground = WoLSpirits.Count < WoLMap.WoLSpiritsMinThreshold ? 
                Brushes.DarkRed : Brushes.Black;
        }


        public void AddApplicationControlSpiritTab()
        {
            Spirit_Type.SelectedValuePath = "id";
            Spirit_Type.DisplayMemberPath = "name";
            Spirit_Type.ItemsSource = SpiritValueDisplay.SpiritTypes.Values;


            Spirit_Rank.SelectedValuePath = "id";
            Spirit_Rank.DisplayMemberPath = "name";
            Spirit_Rank.ItemsSource = SpiritValueDisplay.SpiritRanks.Values;


            Spirit_Attribute.SelectedValuePath = "id";
            Spirit_Attribute.DisplayMemberPath = "name";
            Spirit_Attribute.ItemsSource = SpiritValueDisplay.SpiritAttributes.Values;


            Spirit_Series.SelectedValuePath = "id";
            Spirit_Series.DisplayMemberPath = "name";
            Spirit_Series.ItemsSource = SpiritValueDisplay.SpiritSeries.Values;


            Spirit_Ability.SelectedValuePath = "id";
            Spirit_Ability.DisplayMemberPath = "name";
            Spirit_Ability.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Spirit_SuperAbility.SelectedValuePath = "id";
            Spirit_SuperAbility.DisplayMemberPath = "name";
            Spirit_SuperAbility.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Spirit_BoardConditions.SelectedValuePath = "id";
            Spirit_BoardConditions.DisplayMemberPath = "name";
            Spirit_BoardConditions.ItemsSource = SpiritValueDisplay.SpiritBoardAppearConditions.Values;


            Spirit_SaleType.SelectedValuePath = "id";
            Spirit_SaleType.DisplayMemberPath = "name";
            Spirit_SaleType.ItemsSource = SpiritValueDisplay.SpiritSalesTypes.Values;


            Spirit_Game_ID.SelectedValuePath = "id";
            Spirit_Game_ID.DisplayMemberPath = "name";
            Spirit_Game_ID.ItemsSource = SpiritValueDisplay.SpiritGameTitles.Values;


            Spirit_Evolves_From.SelectedValuePath = "id";
            Spirit_Evolves_From.DisplayMemberPath = "name";
            Spirit_Core_1_ID.SelectedValuePath = "id";
            Spirit_Core_1_ID.DisplayMemberPath = "name";
            Spirit_Core_2_ID.SelectedValuePath = "id";
            Spirit_Core_2_ID.DisplayMemberPath = "name";
            Spirit_Core_3_ID.SelectedValuePath = "id";
            Spirit_Core_3_ID.DisplayMemberPath = "name";
            Spirit_Core_4_ID.SelectedValuePath = "id";
            Spirit_Core_4_ID.DisplayMemberPath = "name";
            Spirit_Core_5_ID.SelectedValuePath = "id";
            Spirit_Core_5_ID.DisplayMemberPath = "name";

            Spirit_Evolves_From.ItemsSource = SpiritValueDisplay.Spirits.Values;
            Spirit_Core_1_ID.ItemsSource = SpiritValueDisplay.Spirits.Values;
            Spirit_Core_2_ID.ItemsSource = SpiritValueDisplay.Spirits.Values;
            Spirit_Core_3_ID.ItemsSource = SpiritValueDisplay.Spirits.Values;
            Spirit_Core_4_ID.ItemsSource = SpiritValueDisplay.Spirits.Values;
            Spirit_Core_5_ID.ItemsSource = SpiritValueDisplay.Spirits.Values;

            Spirit_Evolves_From.SelectionChanged += (s, e) => {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Evolves_From.Items[Spirit_Evolves_From.SelectedIndex]).id;

                if (SpiritValueDisplay.SummonSpirits.ContainsKey(spiritID) || spiritID == 19320013007)
                    Spirit_Evolves_From.SelectedIndex = Spirit_Evolves_From.Items.Count - 1;
            };

            Spirit_Core_1_ID.SelectionChanged += (s, e) =>
            {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Core_1_ID.Items[Spirit_Core_1_ID.SelectedIndex]).id;

                if (spiritID == 8403017505)
                    Spirit_Core_1_ID.SelectedIndex = 0;
            };

            Spirit_Core_2_ID.SelectionChanged += (s, e) =>
            {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Core_2_ID.Items[Spirit_Core_2_ID.SelectedIndex]).id;

                if (spiritID == 8403017505)
                    Spirit_Core_2_ID.SelectedIndex = 0;
            };

            Spirit_Core_3_ID.SelectionChanged += (s, e) =>
            {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Core_3_ID.Items[Spirit_Core_3_ID.SelectedIndex]).id;

                if (spiritID == 8403017505)
                    Spirit_Core_3_ID.SelectedIndex = 0;
            };

            Spirit_Core_4_ID.SelectionChanged += (s, e) =>
            {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Core_4_ID.Items[Spirit_Core_4_ID.SelectedIndex]).id;

                if (spiritID == 8403017505)
                    Spirit_Core_4_ID.SelectedIndex = 0;
            };

            Spirit_Core_5_ID.SelectionChanged += (s, e) =>
            {
                var spiritID = ((SpiritValueDisplay.SpiritID)Spirit_Core_5_ID.Items[Spirit_Core_5_ID.SelectedIndex]).id;

                if (spiritID == 8403017505)
                    Spirit_Core_5_ID.SelectedIndex = 0;
            };

            Spirit_Ability.SelectionChanged += (s, e) => {
                var ability = ((SpiritValueDisplay.SpiritAbility)Spirit_Ability.Items[Spirit_Ability.SelectedIndex]);

                Spirit_Ability_Desc_Block.Text = ability.description;
            };

            Spirit_SuperAbility.SelectionChanged += (s, e) => {
                var ability = ((SpiritValueDisplay.SpiritAbility)Spirit_SuperAbility.Items[Spirit_SuperAbility.SelectedIndex]);

                Spirit_SuperAbility_Desc_Block.Text = ability.description;
            };

            Spirit_Series.SelectionChanged += (s, e) => {
                var series = ((SpiritValueDisplay.SpiritSeriesID)Spirit_Series.Items[Spirit_Series.SelectedIndex]).imageURL;
                string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/";

                if (File.Exists(series.Replace("/Icons/", localOverride)))
                {
                    Uri resourceUri = new Uri(series.Replace("/Icons/", localOverride), UriKind.Absolute);
                    Spirit_Series_Image.Source = new BitmapImage(resourceUri);
                }
                else
                {
                    Uri resourceUri = new Uri(series, UriKind.Relative);
                    Spirit_Series_Image.Source = new BitmapImage(resourceUri);
                }
            };

            Spirit_Rank.SelectionChanged += (s, e) => {
                var rank = ((SpiritValueDisplay.SpiritRank)Spirit_Rank.Items[Spirit_Rank.SelectedIndex]).imageName;

                Uri resourceUri = new Uri(rank, UriKind.Relative);
                Spirit_Rank_Image.Source = new BitmapImage(resourceUri);
            };

            Spirit_Attribute.SelectionChanged += (s, e) => {
                var attr = ((SpiritValueDisplay.SpiritAttribute)Spirit_Attribute.Items[Spirit_Attribute.SelectedIndex]);

                Uri resourceUri = new Uri(attr.imageName, UriKind.Relative);
                Spirit_Attr_Image.Source = new BitmapImage(resourceUri);
                Spirit_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
                Battle_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
                Layout_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
                Fighter_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
            };
        }


        public void AddApplicationControlBattleTab()
        {
            Battle_Win_Condition.SelectedValuePath = "id";
            Battle_Win_Condition.DisplayMemberPath = "name";
            Battle_Win_Condition.ItemsSource = SpiritValueDisplay.BattleWinConditions.Values;


            Battle_Type.SelectedValuePath = "id";
            Battle_Type.DisplayMemberPath = "name";
            Battle_Type.ItemsSource = SpiritValueDisplay.BattleTypes.Values;


            Battle_Stage_Type.SelectedValuePath = "id";
            Battle_Stage_Type.DisplayMemberPath = "name";
            Battle_Stage_Type.ItemsSource = SpiritValueDisplay.BattleStageTypes.Values;


            Battle_Stage_ID.SelectedValuePath = "id";
            Battle_Stage_ID.DisplayMemberPath = "name";
            Battle_Stage_ID.ItemsSource = SpiritValueDisplay.BattleStageIDs.Values;

            Battle_Stage_ID.SelectionChanged += (s, e) => {
                var stage = ((SpiritValueDisplay.BattleStageID)Battle_Stage_ID.Items[Battle_Stage_ID.SelectedIndex]);
                UpdateStageFormCombobox(stage);
                Battle_Stage_Form.SelectedIndex = 0;

                string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/Stage/";
                string stageName = stage.name.Replace(" ", "").ToLower() + ".png";

                if (File.Exists(localOverride + stageName))
                {
                    Uri resourceUri = new Uri(localOverride + stageName, UriKind.Absolute);
                    Stage_Preview.Source = new BitmapImage(resourceUri);
                }
                else
                {
                    Uri stageUri = new Uri("/Icons/Stage/" + stageName, UriKind.Relative);
                    Stage_Preview.Source = new BitmapImage(stageUri);
                }
            };


            Battle_Stage_Attr.SelectedValuePath = "id";
            Battle_Stage_Attr.DisplayMemberPath = "name";
            Battle_Stage_Attr.ItemsSource = SpiritValueDisplay.BattleStageAttributes.Values;


            Battle_Floor.SelectedValuePath = "id";
            Battle_Floor.DisplayMemberPath = "name";
            Battle_Floor.ItemsSource = SpiritValueDisplay.BattleFloorPlaces.Values;


            Battle_Item_Table.SelectedValuePath = "id";
            Battle_Item_Table.DisplayMemberPath = "name";
            Battle_Item_Table.ItemsSource = SpiritValueDisplay.BattleItemTables.Values;


            Battle_Item_Level.SelectedValuePath = "id";
            Battle_Item_Level.DisplayMemberPath = "name";
            Battle_Item_Level.ItemsSource = SpiritValueDisplay.BattleItemLevels.Values;


            Battle_Auto_Win_Skill.SelectedValuePath = "id";
            Battle_Auto_Win_Skill.DisplayMemberPath = "name";
            Battle_Auto_Win_Skill.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Battle_Auto_Win_Skill_2.SelectedValuePath = "id";
            Battle_Auto_Win_Skill_2.DisplayMemberPath = "name";
            Battle_Auto_Win_Skill_2.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Battle_Recommended_Skill.SelectedValuePath = "id";
            Battle_Recommended_Skill.DisplayMemberPath = "name";
            Battle_Recommended_Skill.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Battle_Unrecommended_Skill.SelectedValuePath = "id";
            Battle_Unrecommended_Skill.DisplayMemberPath = "name";
            Battle_Unrecommended_Skill.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;


            Battle_BGM.SelectedValuePath = "id";
            Battle_BGM.DisplayMemberPath = "name";
            Battle_BGM.ItemsSource = SpiritValueDisplay.BattleBGMIDs.Values;


            Event_1_Type.SelectedValuePath = "id";
            Event_1_Type.DisplayMemberPath = "name";
            Event_1_Type.ItemsSource = SpiritValueDisplay.BattleEvents.Values;

            Event_1_Type.SelectionChanged += (s, e) => {

                if (Event_1_Label.SelectedIndex == -1) return;

                var battleEvent = ((SpiritValueDisplay.BattleEvent)Event_1_Type.Items[Event_1_Type.SelectedIndex]);
                UpdateEventCombobox(Event_1_Label, battleEvent);

                if (battleEvent.eventParams.ContainsKey(battleEvent.id))
                {
                    Event_1_Label.SelectedIndex = Event_1_Label.Items.IndexOf(battleEvent.eventParams[battleEvent.id]);
                }
                else
                    Event_1_Label.SelectedIndex = 0;

                Event_1_Desc_Block.Text = battleEvent.description;
            };

            Event_1_Label.SelectedValuePath = "id";
            Event_1_Label.DisplayMemberPath = "name";

            Event_2_Type.SelectedValuePath = "id";
            Event_2_Type.DisplayMemberPath = "name";
            Event_2_Type.ItemsSource = SpiritValueDisplay.BattleEvents.Values;

            Event_2_Type.SelectionChanged += (s, e) => {

                if (Event_2_Label.SelectedIndex == -1) return;

                var battleEvent = ((SpiritValueDisplay.BattleEvent)Event_2_Type.Items[Event_2_Type.SelectedIndex]);
                UpdateEventCombobox(Event_2_Label, battleEvent);

                if (battleEvent.eventParams.ContainsKey(battleEvent.id))
                {
                    Event_2_Label.SelectedIndex = Event_2_Label.Items.IndexOf(battleEvent.eventParams[battleEvent.id]);
                }
                else
                    Event_2_Label.SelectedIndex = 0;

                Event_2_Desc_Block.Text = battleEvent.description;
            };

            Event_2_Label.SelectedValuePath = "id";
            Event_2_Label.DisplayMemberPath = "name";

            Event_3_Type.SelectedValuePath = "id";
            Event_3_Type.DisplayMemberPath = "name";
            Event_3_Type.ItemsSource = SpiritValueDisplay.BattleEvents.Values;

            Event_3_Type.SelectionChanged += (s, e) => {

                if (Event_3_Label.SelectedIndex == -1) return;

                var battleEvent = ((SpiritValueDisplay.BattleEvent)Event_3_Type.Items[Event_3_Type.SelectedIndex]);
                UpdateEventCombobox(Event_3_Label, battleEvent);

                if (battleEvent.eventParams.ContainsKey(battleEvent.id))
                {
                    Event_3_Label.SelectedIndex = Event_3_Label.Items.IndexOf(battleEvent.eventParams[battleEvent.id]);
                }
                else
                    Event_3_Label.SelectedIndex = 0;

                Event_3_Desc_Block.Text = battleEvent.description;
            };

            Event_3_Label.SelectedValuePath = "id";
            Event_3_Label.DisplayMemberPath = "name";

            Battle_Stage_Form.SelectedValuePath = "id";
            Battle_Stage_Form.DisplayMemberPath = "name";

            Battle_Recommended.SelectionChanged += (s, e) => {
                var mod = GetMods(currentSpirit);
                var battle = mod.displayingAltBattle ? mod.alternateBattle : mod.battle;

                Battle_Recommended_Skill.SelectedIndex = Battle_Recommended_Skill.Items.IndexOf
                    (SpiritValueDisplay.SpiritAbilities[battle.recommended_skills[Battle_Recommended.SelectedIndex]]);
            };

            Battle_Unrecommended.SelectionChanged += (s, e) => {
                var mod = GetMods(currentSpirit);
                var battle = mod.displayingAltBattle ? mod.alternateBattle : mod.battle;

                Battle_Unrecommended_Skill.SelectedIndex = Battle_Unrecommended_Skill.Items.IndexOf
                    (SpiritValueDisplay.SpiritAbilities[battle.un_recommended_skills[Battle_Unrecommended.SelectedIndex]]);
            };
        }


        public Spirit GetMods(Spirit spirit)
        {
            // Check for mods
            if (unsavedEdits.ContainsKey(spirit.index))
                return unsavedEdits[spirit.index];
            else if (modList.ContainsKey(spirit.index))
                return modList[spirit.index];

            return spirit;
        }

        public Spirit GetMods(ulong id, bool bCheckUnsaved = true)
        {
            if (bCheckUnsaved)
            {
                foreach (var spr in unsavedEdits.Values)
                {
                    if (spr.spirit_id == id)
                    {
                        return spr;
                    }
                }
            }

            foreach (var spr in modList.Values)
            {
                if (spr.spirit_id == id)
                {
                    return spr;
                }
            }

            if(baseSpirits.ContainsKey(id))
                return baseSpirits[id];

            return new Spirit();
        }

        public Spirit GetMods(string name, bool bCheckUnsaved = true)
        {
            foreach (var spr in modList.Values)
            {
                if (spr.display_name == name)
                {
                    return spr;
                }
            }

            foreach (var spr in baseSpirits.Values)
            {
                if (spr.display_name == name)
                {
                    return spr;
                }
            }

            MessageBox.Show($"No spirt named {name} exists.", "Hold up");

            return new Spirit();
        }

        /// <summary>
        /// Update the spirit board's display to the given spirit
        /// </summary>
        /// <param name="spirit">The spirit to display</param>
        public void DisplaySpirit(Spirit spirit)
        {
            Revert_Mod.Content = spirit.Custom ? "Delete" : "Revert";

            // Prevent changes from being processed while we update the display.
            handleChanges = false;
            currentSpirit = spirit;

            Battle_Header.Content = spirit.display_name + " Battle";

            spirit = GetMods(spirit);

            Spirit_InternalID_Label.Text = $"Internal ID:\n{spirit.name_id}";
            Spirit_Name.Text = spirit.display_name;
            Spirit_Index.Value = spirit.directory_id;
            Spirit_Type.SelectedIndex = Spirit_Type.Items.IndexOf(SpiritValueDisplay.SpiritTypes[spirit.type]);
            Spirit_Rank.SelectedIndex = Spirit_Rank.Items.IndexOf(SpiritValueDisplay.SpiritRanks[spirit.rank]);
            Spirit_Attribute.SelectedIndex = Spirit_Attribute.Items.IndexOf(SpiritValueDisplay.SpiritAttributes[spirit.spirit_attr]);

            if (SpiritValueDisplay.SpiritSeries.ContainsKey(spirit.series_id))
                Spirit_Series.SelectedIndex = Spirit_Series.Items.IndexOf(SpiritValueDisplay.SpiritSeries[spirit.series_id]);
            else
                Spirit_Series.SelectedIndex = 0;

            // Fix for YesWeDo edited spirits
            if (spirit.game_title == 0)
            {
                spirit.game_title = 73502405852;
            }

            if (spirit.game_title == 84604977966)
            {
                Spirit_Game_ID.SelectedIndex = 1;
            }
            else
            {
                if (SpiritValueDisplay.SpiritGameTitles.ContainsKey(spirit.game_title))
                    Spirit_Game_ID.SelectedIndex = Spirit_Game_ID.Items.IndexOf(SpiritValueDisplay.SpiritGameTitles[spirit.game_title]);
                else
                    Spirit_Game_ID.SelectedIndex = 0;
            }

            Spirit_Ability.SelectedIndex = Spirit_Ability.Items.IndexOf(SpiritValueDisplay.SpiritAbilities[spirit.ability_id]);
            Spirit_SuperAbility.SelectedIndex = Spirit_SuperAbility.Items.IndexOf(SpiritValueDisplay.SpiritAbilities[spirit.super_ability]);
            Spirit_BoardConditions.SelectedIndex = 
                Spirit_BoardConditions.Items.IndexOf(SpiritValueDisplay.SpiritBoardAppearConditions[spirit.appear_conditions]);
            Spirit_SaleType.SelectedIndex = Spirit_SaleType.Items.IndexOf(SpiritValueDisplay.SpiritSalesTypes[spirit.shop_sales_type]);
            Spirit_Slots.Value = spirit.slots;

            Spirit_Ability_Desc_Block.Text = (SpiritValueDisplay.SpiritAbilities[spirit.ability_id]).description;
            Spirit_SuperAbility_Desc_Block.Text = (SpiritValueDisplay.SpiritAbilities[spirit.super_ability]).description;

            Spirit_Base_Atk.Value = spirit.base_attack;
            Spirit_Base_Def.Value = spirit.base_defense;
            Spirit_Max_Atk.Value = spirit.max_attack;
            Spirit_Max_Def.Value = spirit.max_defense;
            Spirit_Exp.Value = (int)spirit.exp_lvl_max;
            Spirit_Lvl_Rate.Value = (decimal)spirit.exp_up_rate;
            Spirit_Reward_EXP.Value = (int)spirit.battle_exp;
            Spirit_Reward_SP.Value = (int)spirit.reward_capacity;
            Spirit_Is_Rematch.IsChecked = spirit.rematch;

            Spirit_OnBoard.IsChecked = spirit.is_board_appear;
            Spirit_Price.Value = (int)spirit.shop_price;

            Spirit_SummonSP.Value = (int)spirit.summon_sp;
            Spirit_Core_1_Quantity.Value = spirit.summon_quantity1;
            Spirit_Core_2_Quantity.Value = spirit.summon_quantity2;
            Spirit_Core_3_Quantity.Value = spirit.summon_quantity3;
            Spirit_Core_4_Quantity.Value = spirit.summon_quantity4;
            Spirit_Core_5_Quantity.Value = spirit.summon_quantity5;

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.summon_id1))
            {
                Spirit_Core_1_ID.SelectedIndex = Spirit_Core_1_ID.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.summon_id1]);
            }
            else
            {
                Spirit_Core_1_ID.SelectedIndex = 0;
            }

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.summon_id2))
            {
                Spirit_Core_2_ID.SelectedIndex = Spirit_Core_2_ID.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.summon_id2]);
            }
            else
            {
                Spirit_Core_2_ID.SelectedIndex = 0;
            }

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.summon_id3))
            {
                Spirit_Core_3_ID.SelectedIndex = Spirit_Core_3_ID.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.summon_id3]);
            }
            else
            {
                Spirit_Core_3_ID.SelectedIndex = 0;
            }

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.summon_id4))
            {
                Spirit_Core_4_ID.SelectedIndex = Spirit_Core_4_ID.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.summon_id4]);
            }
            else
            {
                Spirit_Core_4_ID.SelectedIndex = 0;
            }

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.summon_id5))
            {
                Spirit_Core_5_ID.SelectedIndex = Spirit_Core_5_ID.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.summon_id5]);
            }
            else
            {
                Spirit_Core_5_ID.SelectedIndex = 0;
            }

            if (SpiritValueDisplay.Spirits.ContainsKey(spirit.evolve_src))
            {
                Spirit_Evolves_From.SelectedIndex = Spirit_Evolves_From.Items.IndexOf(SpiritValueDisplay.Spirits[spirit.evolve_src]);
            }
            else
            {
                Spirit_Evolves_From.SelectedIndex = 0;
            }

            string series = "";

            if(SpiritValueDisplay.SpiritSeries.ContainsKey(spirit.series_id))
                series = SpiritValueDisplay.SpiritSeries[spirit.series_id].imageURL;

            string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/";

            if (File.Exists(series.Replace("/Icons/", localOverride)))
            {
                Uri resourceUri = new Uri(series.Replace("/Icons/", localOverride), UriKind.Absolute);
                Spirit_Series_Image.Source = new BitmapImage(resourceUri);
            }
            else
            {
                Uri seriesURI = new Uri(series, UriKind.Relative);
                Spirit_Series_Image.Source = new BitmapImage(seriesURI);
            }

            var attr = SpiritValueDisplay.SpiritAttributes[spirit.spirit_attr];
            Uri attrURI = new Uri(attr.imageName, UriKind.Relative);
            Spirit_Attr_Image.Source = new BitmapImage(attrURI);
            Spirit_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
            Battle_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
            Layout_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));
            Fighter_BG.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));

            if (spirit.displayingAltBattle)
            {
                DisplayBattle(spirit.alternateBattle);
                Alt_Battle_Button.Content = "Switch to Base Battle";
                Battle_Header.Content += " (DLC Version)";
            }
            else
            {
                DisplayBattle(spirit.battle);
                Alt_Battle_Button.Content = "Switch to DLC Battle";
            }

            DisplayLayout(spirit.aesthetics);

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + spirit.name_id + ".png") &&
                CurrentPreviewSource != AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + spirit.name_id + ".png")
            {
                LoadImage(AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + spirit.name_id + ".png");
            }
            else if (CurrentPreview != null)
                ApplyCurrentImageOffsets();

            handleChanges = true;
            Alt_Battle_Button.Visibility = spirit.alternateBattle != null ? Visibility.Visible : Visibility.Hidden;

            SetWoLToggle(spirit);
        }


        private void SetWoLToggle(Spirit spirit)
        {
            Spirit_Is_In_WoL.IsEnabled = true;

            if (spirit.type == 81665422107 || spirit.type == 89039743027)
            {
                if (baseSpirits.ContainsKey(spirit.spirit_id))
                {
                    if (SpiritValueDisplay.Bosses.Contains(baseSpirits[spirit.spirit_id].display_name))
                    {
                        Spirit_Is_In_WoL.IsEnabled = false;
                    }
                }
            }
            else
            {
                Spirit_Is_In_WoL.IsEnabled = false;
            }

            if (Spirit_Is_In_WoL.IsEnabled)
            {
                Spirit_Is_In_WoL.IsChecked = WoLSpirits.Contains(spirit.save_no);
            }
            else
            {
                Spirit_Is_In_WoL.IsChecked = false;

                if (WoLSpirits.Contains(spirit.save_no))
                {
                    WoLSpirits.Remove(spirit.save_no);

                    if (WoLSpirits.Count < WoLMap.WoLSpiritsMinThreshold)
                    {
                        MessageBox.Show("Your pool of spirits is too small. The randomizer will not be balanced.",
                            "Hold Up");
                    }
                }
            }
        }


        /// <summary>
        /// Updates the battle's display to the given battle
        /// </summary>
        /// <param name="battle">The given battle</param>
        public void DisplayBattle(SpiritBattle battle)
        {
            Battle_Type.SelectedIndex = Battle_Type.Items.IndexOf(SpiritValueDisplay.BattleTypes[battle.battle_type]);
            Battle_Win_Condition.SelectedIndex = Battle_Win_Condition.Items.IndexOf(SpiritValueDisplay.BattleWinConditions[battle.result_type]);
            Battle_Power.Value = Convert.ToInt32(battle.battle_power);
            Battle_Time.Value = Convert.ToInt32(battle.battle_time_sec);
            Battle_Stock.Value = Convert.ToInt32(battle.basic_stock);
            Battle_Init_HP.Value = Convert.ToInt32(battle.basic_init_hp);
            Battle_Init_Damage.Value = Convert.ToInt32(battle.basic_init_damage);

            Spirit_Hint1.IsChecked = battle.x0d41ef8328;
            Spirit_Hint2.IsChecked = battle.aw_flap_delay;
            Spirit_Hint3.IsChecked = battle.x0d6f19abae;

            var stage = SpiritValueDisplay.BattleStageIDs[battle.stage_id];
            Battle_Stage_ID.SelectedIndex = Battle_Stage_ID.Items.IndexOf(stage);
            UpdateStageFormCombobox(stage);

            if (stage.stageForms.ContainsKey(Convert.ToInt32(battle.x18e536d4f7)))
            {
                Battle_Stage_Form.SelectedIndex = 
                    Battle_Stage_Form.Items.IndexOf(stage.stageForms[Convert.ToInt32(battle.x18e536d4f7)]);
            }
            else
            {
                Battle_Stage_Form.SelectedIndex = 0;
            }

            string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/Stage/";
            string stageName = stage.name.Replace(" ", "").ToLower() + ".png";

            if (File.Exists(localOverride + stageName))
            {
                Uri resourceUri = new Uri(localOverride + stageName, UriKind.Absolute);
                Spirit_Series_Image.Source = new BitmapImage(resourceUri);
            }
            else
            {
                Uri stageUri = new Uri("/Icons/Stage/" + stageName, UriKind.Relative);
                Stage_Preview.Source = new BitmapImage(stageUri);
            }

            Battle_Hazards.IsChecked = battle.stage_gimmick;
            Battle_Stage_Type.SelectedIndex = Battle_Stage_Type.Items.IndexOf(SpiritValueDisplay.BattleStageTypes[battle.stage_type]);
            Battle_Stage_Attr.SelectedIndex = Battle_Stage_Attr.Items.IndexOf(SpiritValueDisplay.BattleStageAttributes[battle.stage_attr]);

            if (battle.floor_place_id == 11760337516) battle.floor_place_id = 32835731711;

            Battle_Floor.SelectedIndex = Battle_Floor.Items.IndexOf(SpiritValueDisplay.BattleFloorPlaces[battle.floor_place_id]);
            Battle_Item_Table.SelectedIndex = Battle_Item_Table.Items.IndexOf(SpiritValueDisplay.BattleItemTables[battle.item_table]);
            Battle_Item_Level.SelectedIndex = Battle_Item_Level.Items.IndexOf(SpiritValueDisplay.BattleItemLevels[battle.item_level]);

            // Temp so I can get an update live for KH. The music is stalling my progress.
            if (SpiritValueDisplay.BattleBGMIDs.ContainsKey(battle.stage_bgm))
            {
                Battle_BGM.SelectedIndex = Battle_BGM.Items.IndexOf(SpiritValueDisplay.BattleBGMIDs[battle.stage_bgm]);
            }
            else
            {
                Battle_BGM.SelectedIndex = 0;
                battle.stage_bgm = (Battle_BGM.Items[0] as SpiritValueDisplay.BattleBGM).id;
            }

            Event_1_Type.SelectedIndex = Event_1_Type.Items.IndexOf(SpiritValueDisplay.BattleEvents[battle.event1_type]);
            UpdateEventCombobox(Event_1_Label, SpiritValueDisplay.BattleEvents[battle.event1_type]);

            if (battle.event1_label == 0) battle.event1_label = 8403017505;

            if (SpiritValueDisplay.BattleEvents[battle.event1_type].eventParams.ContainsKey(battle.event1_label))
            {
                Event_1_Label.SelectedIndex = Event_1_Label.Items.IndexOf(
                    SpiritValueDisplay.BattleEvents[battle.event1_type].eventParams[battle.event1_label]);
            }
            else
            {
                Event_1_Label.SelectedIndex = 0;
            }

            Event_1_Count.Value = battle.event1_count;
            Event_1_Damage.Value = battle.event1_damage;
            Event_1_Start_Time.Value = battle.event1_start_time;
            Event_1_Range_Time.Value = battle.event1_range_time;
            Event_1_Desc_Block.Text = SpiritValueDisplay.BattleEvents[battle.event1_type].description;


            Event_2_Type.SelectedIndex = Event_2_Type.Items.IndexOf(SpiritValueDisplay.BattleEvents[battle.event2_type]);
            UpdateEventCombobox(Event_2_Label, SpiritValueDisplay.BattleEvents[battle.event2_type]);

            if (battle.event2_label == 0) battle.event2_label = 8403017505;

            Event_2_Label.SelectedIndex = Event_2_Label.Items.IndexOf(
                SpiritValueDisplay.BattleEvents[battle.event2_type].eventParams[battle.event2_label]);
            Event_2_Count.Value = battle.event2_count;
            Event_2_Damage.Value = battle.event2_damage;
            Event_2_Start_Time.Value = battle.event2_start_time;
            Event_2_Range_Time.Value = battle.event2_range_time;
            Event_2_Desc_Block.Text = SpiritValueDisplay.BattleEvents[battle.event2_type].description;

            Event_3_Type.SelectedIndex = Event_3_Type.Items.IndexOf(SpiritValueDisplay.BattleEvents[battle.event3_type]);
            UpdateEventCombobox(Event_3_Label, SpiritValueDisplay.BattleEvents[battle.event3_type]);

            if (battle.event3_label == 0) battle.event3_label = 8403017505;

            Event_3_Label.SelectedIndex = Event_3_Label.Items.IndexOf(
                SpiritValueDisplay.BattleEvents[battle.event3_type].eventParams[battle.event3_label]);
            Event_3_Count.Value = battle.event3_count;
            Event_3_Damage.Value = battle.event3_damage;
            Event_3_Start_Time.Value = battle.event3_start_time;
            Event_3_Range_Time.Value = battle.event3_range_time;
            Event_3_Desc_Block.Text = SpiritValueDisplay.BattleEvents[battle.event3_type].description;

            if(battle.auto_win_skill == 0)
            {
                battle.auto_win_skill = 19320013007;
            }

            if (battle.x18404d4ecb == 0)
            {
                battle.x18404d4ecb = 19320013007;
            }

            Battle_Auto_Win_Skill.SelectedIndex = Battle_Auto_Win_Skill.Items.IndexOf(SpiritValueDisplay.SpiritAbilities[battle.auto_win_skill]);
            Battle_Auto_Win_Skill_2.SelectedIndex = Battle_Auto_Win_Skill_2.Items.IndexOf(SpiritValueDisplay.SpiritAbilities[battle.x18404d4ecb]);
            Battle_Recommended_Skill.SelectedIndex = Battle_Recommended_Skill.Items.IndexOf
                (SpiritValueDisplay.SpiritAbilities[battle.recommended_skills[Battle_Recommended.SelectedIndex]]);
            Battle_Unrecommended_Skill.SelectedIndex = Battle_Unrecommended_Skill.Items.IndexOf
                (SpiritValueDisplay.SpiritAbilities[battle.un_recommended_skills[Battle_Unrecommended.SelectedIndex]]);


            Fighter_Tab_Control.Items.Clear();

            for (int i = 0; i < battle.fighters.Count; i++)
            {
                var fighterTab = GenerateFighterTab(i);
                SetFighterTabData(battle.fighters[i], fighterTab);
            }

            if(battle.fighters.Count < Spirit.MAX_FIGHTERS)
            {
                TabItem tabAdd = new TabItem();
                tabAdd.Name = "newfighter";
                tabAdd.Header = "+";
                Fighter_Tab_Control.Items.Add(tabAdd);
            }

            Fighter_Tab_Control.SelectedIndex = 0;
        }


        /// <summary>
        /// Updates the layout's display to the given layout data
        /// </summary>
        /// <param name="layout">The given layout data</param>
        public void DisplayLayout(SpiritAesthetics layout)
        {
            int imageIndex = Image_Pos_Type.SelectedIndex;

            Image_X.Value = Convert.ToDecimal(layout.art_pos_x[imageIndex]);
            Image_Y.Value = Convert.ToDecimal(layout.art_pos_y[imageIndex]);
            Image_Scale.Value = Convert.ToDecimal(layout.art_size[imageIndex]);

            int effectIndex = Effect_Index.SelectedIndex;

            if (layout.effect_count > 15)
                layout.effect_count = 15;

            Effect_Count.Value = Convert.ToInt32(layout.effect_count);
            Effect_X.Value = layout.effect_pos_x[effectIndex];
            Effect_Y.Value = layout.effect_pos_y[effectIndex];

            switch (imageIndex)
            {
                case 0:
                    Image_Pos_Type_Desc_Block.Text = "Used for the Spirit Board thumbnails and the Party Screen.";
                    break;
                case 1:
                    Image_Pos_Type_Desc_Block.Text = "Used for the Gallery and Training Screens.";
                    break;
                case 2:
                    Image_Pos_Type_Desc_Block.Text = "Unknown, but is usually Gallery -50 Y.";
                    break;
                case 3:
                    Image_Pos_Type_Desc_Block.Text = "Used for the small portrait next to damage meters.";
                    break;
            }
        }


        public FighterTab GenerateFighterTab(int index)
        {
            var item = new TabItem
            {
                Name = "fighter" + (index + 1),
                Header = "Fighter " + (index + 1),
                Content = new FighterTab()
            };

            item.HeaderTemplate = Fighter_Tab_Control.FindResource("FighterTabHeader") as DataTemplate;
            item.ApplyTemplate();

            var fighterTab = (FighterTab)item.Content;

            fighterTab.Fighter_Name.Content = item.Header;

            fighterTab.Fighter_ID.SelectedValuePath = "id";
            fighterTab.Fighter_ID.DisplayMemberPath = "name";
            fighterTab.Fighter_ID.ItemsSource = SpiritValueDisplay.FighterIDs.Values;

            fighterTab.Fighter_Mii_Hat_ID.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_Hat_ID.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_Hat_ID.ItemsSource = SpiritValueDisplay.MiiHats.Values;

            fighterTab.Fighter_Mii_Voice_ID.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_Voice_ID.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_Voice_ID.ItemsSource = SpiritValueDisplay.MiiVoices.Values;

            fighterTab.Fighter_Mii_Color_ID.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_Color_ID.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_Color_ID.ItemsSource = SpiritValueDisplay.MiiColors.Values;

            // List is set later based on selected fighter
            fighterTab.Fighter_Mii_Outfit_ID.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_Outfit_ID.DisplayMemberPath = "name";

            fighterTab.Fighter_Mii_NSpec.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_NSpec.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_SSpec.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_SSpec.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_USpec.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_USpec.DisplayMemberPath = "name";
            fighterTab.Fighter_Mii_DSpec.SelectedValuePath = "id";
            fighterTab.Fighter_Mii_DSpec.DisplayMemberPath = "name";

            fighterTab.Fighter_CPU_Attribute.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Attribute.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Attribute.ItemsSource = SpiritValueDisplay.SpiritAttributes.Values;

            fighterTab.Fighter_CPU_Spirit.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Spirit.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Spirit.ItemsSource = SpiritValueDisplay.Spirits.Values;

            fighterTab.Fighter_Spawn_Type.SelectedValuePath = "id";
            fighterTab.Fighter_Spawn_Type.DisplayMemberPath = "name";
            fighterTab.Fighter_Spawn_Type.ItemsSource = SpiritValueDisplay.EntryTypes.Values;

            fighterTab.Fighter_CPU_Ability_1.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Ability_1.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Ability_1.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;

            fighterTab.Fighter_CPU_Ability_2.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Ability_2.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Ability_2.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;

            fighterTab.Fighter_CPU_Ability_3.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Ability_3.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Ability_3.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;

            fighterTab.Fighter_CPU_Ability_Personal.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Ability_Personal.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Ability_Personal.ItemsSource = SpiritValueDisplay.SpiritAbilities.Values;

            fighterTab.Fighter_CPU_Type.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Type.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Type.ItemsSource = SpiritValueDisplay.CPUBehaviors.Values;

            fighterTab.Fighter_CPU_Subtype.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Subtype.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Subtype.ItemsSource = SpiritValueDisplay.CPUBehaviors.Values;

            fighterTab.Fighter_Color_Mii.SelectedValuePath = "id";
            fighterTab.Fighter_Color_Mii.DisplayMemberPath = "name";
            fighterTab.Fighter_Color_Mii.ItemsSource = SpiritValueDisplay.MiiFaces.Values;

            fighterTab.Fighter_CPU_Subrule.SelectedValuePath = "id";
            fighterTab.Fighter_CPU_Subrule.DisplayMemberPath = "name";
            fighterTab.Fighter_CPU_Subrule.ItemsSource = SpiritValueDisplay.CPUSubRules.Values;

            Fighter_Tab_Control.Items.Add(item);

            fighterTab.Fighter_ID.SelectionChanged += (s, e) =>
            {
                var fighter = ((SpiritValueDisplay.FighterID)fighterTab.Fighter_ID.SelectedItem);
                string costumeIndex = "0";

                if (SpiritValueDisplay.Miis.ContainsKey(fighter.id))
                {
                    fighterTab.Fighter_Color.Visibility = Visibility.Hidden;
                    fighterTab.Fighter_Color_Mii.Visibility = Visibility.Visible;

                    fighterTab.Fighter_Color_Mii.SelectedIndex = 0;

                    fighterTab.Fighter_Mii_Outfit_ID.IsEnabled = true;
                    fighterTab.Fighter_Mii_Outfit_ID.ItemsSource = SpiritValueDisplay.Miis[fighter.id].miiCostumes;
                    fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex = 0;

                    fighterTab.Fighter_Mii_Hat_ID.IsEnabled = true;
                    fighterTab.Fighter_Mii_Hat_ID.SelectedIndex = 0;

                    fighterTab.Fighter_Mii_Color_ID.IsEnabled = true;
                    fighterTab.Fighter_Mii_Color_ID.SelectedIndex = 0;

                    fighterTab.Fighter_Mii_Voice_ID.IsEnabled = true;
                    fighterTab.Fighter_Mii_Voice_ID.SelectedIndex = 0;

                    fighterTab.Fighter_Mii_NSpec.IsEnabled = true;
                    fighterTab.Fighter_Mii_NSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.id].neutralSpecial;
                    fighterTab.Fighter_Mii_NSpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_SSpec.IsEnabled = true;
                    fighterTab.Fighter_Mii_SSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.id].sideSpecial;
                    fighterTab.Fighter_Mii_SSpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_USpec.IsEnabled = true;
                    fighterTab.Fighter_Mii_USpec.ItemsSource = SpiritValueDisplay.Miis[fighter.id].upSpecial;
                    fighterTab.Fighter_Mii_USpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_DSpec.IsEnabled = true;
                    fighterTab.Fighter_Mii_DSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.id].downSpecial;
                    fighterTab.Fighter_Mii_DSpec.SelectedIndex = 0;
                }
                else
                {
                    fighterTab.Fighter_Color.Visibility = Visibility.Visible;
                    fighterTab.Fighter_Color_Mii.Visibility = Visibility.Hidden;

                    if (!SpiritValueDisplay.FighterIDs[fighter.id].hasAlts)
                    {
                        fighterTab.Fighter_Color.IsEnabled = false;
                        fighterTab.Fighter_Color.Value = 0;
                        costumeIndex = "0";
                    }
                    else
                    {
                        fighterTab.Fighter_Color.IsEnabled = true;
                        costumeIndex = fighterTab.Fighter_Color.Value.ToString();
                    }

                    fighterTab.Fighter_Mii_Hat_ID.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_Hat_ID.IsEnabled = false;
                    fighterTab.Fighter_Mii_Color_ID.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_Color_ID.IsEnabled = false;
                    fighterTab.Fighter_Mii_Voice_ID.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_Voice_ID.IsEnabled = false;
                    fighterTab.Fighter_Mii_Outfit_ID.ItemsSource = SpiritValueDisplay.NotMiis.Values;
                    fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_Outfit_ID.IsEnabled = false;

                    fighterTab.Fighter_Mii_NSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                    fighterTab.Fighter_Mii_NSpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_NSpec.IsEnabled = false;
                    fighterTab.Fighter_Mii_SSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                    fighterTab.Fighter_Mii_SSpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_SSpec.IsEnabled = false;
                    fighterTab.Fighter_Mii_USpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                    fighterTab.Fighter_Mii_USpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_USpec.IsEnabled = false;
                    fighterTab.Fighter_Mii_DSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                    fighterTab.Fighter_Mii_DSpec.SelectedIndex = 0;
                    fighterTab.Fighter_Mii_DSpec.IsEnabled = false;
                }

                string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/Stock/";
                string fighterName = "chara_2_" + fighter.internalID + "_0" + costumeIndex + ".png";

                if (File.Exists(localOverride + fighterName))
                {
                    Uri resourceUri = new Uri(localOverride + fighterName, UriKind.Absolute);
                    fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(resourceUri);
                }
                else
                {
                    Uri fighterUri = new Uri("/Icons/Stock/" + fighterName, UriKind.Relative);
                    fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(fighterUri);
                }
            };

            fighterTab.Fighter_Color.ValueChanged += (s, e) =>
            {
                var fighter = ((SpiritValueDisplay.FighterID)fighterTab.Fighter_ID.SelectedItem);
                string costumeIndex = fighter.hasAlts ? fighterTab.Fighter_Color.Value.ToString() : "0";

                string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/Stock/";
                string fighterName = "chara_2_" + fighter.internalID + "_0" + costumeIndex + ".png";


                if (File.Exists(localOverride + fighterName))
                {
                    Uri resourceUri = new Uri(localOverride + fighterName, UriKind.Absolute);
                    fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(resourceUri);
                }
                else
                {
                    Uri fighterUri = new Uri("/Icons/Stock/" + fighterName, UriKind.Relative);
                    fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(fighterUri);
                }
            };

            fighterTab.Fighter_CPU_Spirit.SelectionChanged += (s, e) => {
                var spiritID = ((SpiritValueDisplay.SpiritID)fighterTab.Fighter_CPU_Spirit.Items
                    [fighterTab.Fighter_CPU_Spirit.SelectedIndex]).id;

                if (SpiritValueDisplay.SummonSpirits.ContainsKey(spiritID) || spiritID == 8403017505)
                    fighterTab.Fighter_CPU_Spirit.SelectedIndex = 0;
            };

            fighterTab.Fighter_CPU_Attribute.SelectionChanged += (s, e) =>
            {
                var attr = ((SpiritValueDisplay.SpiritAttribute)fighterTab.Fighter_CPU_Attribute.
                    Items[fighterTab.Fighter_CPU_Attribute.SelectedIndex]);

                Uri attrUri = new Uri(attr.imageName, UriKind.Relative);
                fighterTab.Fighter_Attr_Image.Source = new BitmapImage(attrUri);
            };

            fighterTab.Fighter_CPU_Ability_1.SelectionChanged += (s, e) =>
            {
                if (fighterTab.Fighter_CPU_Ability_1.SelectedIndex == -1)
                {
                    fighterTab.Fighter_CPU_Ability_1.SelectedIndex = 0;
                }

                fighterTab.Spirit_Ability_1_Desc_Block.Text = ((SpiritValueDisplay.SpiritAbility)
                    fighterTab.Fighter_CPU_Ability_1.Items[fighterTab.Fighter_CPU_Ability_1.SelectedIndex]).description;
            };

            fighterTab.Fighter_CPU_Ability_2.SelectionChanged += (s, e) =>
            {
                if (fighterTab.Fighter_CPU_Ability_2.SelectedIndex == -1)
                {
                    fighterTab.Fighter_CPU_Ability_2.SelectedIndex = 0;
                }

                fighterTab.Spirit_Ability_2_Desc_Block.Text = ((SpiritValueDisplay.SpiritAbility)
                    fighterTab.Fighter_CPU_Ability_2.Items[fighterTab.Fighter_CPU_Ability_2.SelectedIndex]).description;
            };

            fighterTab.Fighter_CPU_Ability_3.SelectionChanged += (s, e) =>
            {
                if (fighterTab.Fighter_CPU_Ability_3.SelectedIndex == -1)
                {
                    fighterTab.Fighter_CPU_Ability_3.SelectedIndex = 0;
                }

                fighterTab.Spirit_Ability_3_Desc_Block.Text = ((SpiritValueDisplay.SpiritAbility)
                    fighterTab.Fighter_CPU_Ability_3.Items[fighterTab.Fighter_CPU_Ability_3.SelectedIndex]).description;
            };

            fighterTab.Fighter_CPU_Ability_Personal.SelectionChanged += (s, e) =>
            {
                if (fighterTab.Fighter_CPU_Ability_Personal.SelectedIndex == -1)
                {
                    fighterTab.Fighter_CPU_Ability_Personal.SelectedIndex = 0;
                }

                fighterTab.Spirit_Ability_P_Desc_Block.Text = ((SpiritValueDisplay.SpiritAbility)
                    fighterTab.Fighter_CPU_Ability_Personal.Items[fighterTab.Fighter_CPU_Ability_Personal.SelectedIndex]).description;
            };

            fighterTab.Fighter_ID.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Spawn_Type.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_Hat_ID.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_Outfit_ID.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_Voice_ID.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_Color_ID.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Color_Mii.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_NSpec.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_SSpec.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_USpec.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Mii_DSpec.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Type.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Subtype.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Attribute.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Spirit.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Ability_1.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Ability_2.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Ability_3.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Ability_Personal.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Subrule.SelectionChanged += UpdateSpirit;
            fighterTab.Fighter_Color.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_Spawn_Time.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_Spawn_Count.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_Damage.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_HP.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_Stock.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Level.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Attack.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Defense.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_CPU_Scale.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_Fly_Rate.ValueChanged += UpdateSpirit;
            fighterTab.Fighter_First_Appear.Click += UpdateSpirit;
            fighterTab.Fighter_Mob.Click += UpdateSpirit;
            fighterTab.Fighter_Ignore_Num_Col.Click += UpdateSpirit;
            fighterTab.Fighter_Can_Use_Item.Click += UpdateSpirit;
            fighterTab.Fighter_Can_Drop_Item.Click += UpdateSpirit;
            fighterTab.Fighter_Charge_FS.Click += UpdateSpirit;

            return fighterTab;
        }


        public void SetFighterTabData(SpiritBattleFighter fighter, FighterTab fighterTab)
        {
            var fighterData = SpiritValueDisplay.FighterIDs[fighter.fighter_kind];

            fighterTab.Fighter_ID.SelectedIndex =
                fighterTab.Fighter_ID.Items.IndexOf(fighterData);

            fighterTab.Fighter_Color.IsEnabled = fighterData.hasAlts;
            fighterTab.Fighter_Color_Mii.SelectedIndex = 0;
            string costumeIndex = "0";

            if (!fighterTab.Fighter_Color.IsEnabled) fighterTab.Fighter_Color.Value = 0;
            else fighterTab.Fighter_Color.Value = fighter.color;

            if (SpiritValueDisplay.Miis.ContainsKey(fighter.fighter_kind))
            {
                fighterTab.Fighter_Color.Visibility = Visibility.Hidden;
                fighterTab.Fighter_Color_Mii.Visibility = Visibility.Visible;

                fighterTab.Fighter_Color_Mii.SelectedIndex = fighter.color;

                fighterTab.Fighter_Mii_Outfit_ID.IsEnabled = true;
                fighterTab.Fighter_Mii_Outfit_ID.ItemsSource = SpiritValueDisplay.Miis[fighter.fighter_kind].miiCostumes;
                fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex = SpiritValueDisplay.Miis[fighter.fighter_kind].miiCostumes.
                    IndexOf(SpiritValueDisplay.Miis[fighter.fighter_kind].miiCostumes.Find((mo) => mo.id == fighter.mii_body_id));

                fighterTab.Fighter_Mii_Hat_ID.IsEnabled = true;

                if (SpiritValueDisplay.MiiHats.ContainsKey(fighter.mii_hat_id))
                {
                    fighterTab.Fighter_Mii_Hat_ID.SelectedIndex = fighterTab.Fighter_Mii_Hat_ID.Items.IndexOf
                       (SpiritValueDisplay.MiiHats[fighter.mii_hat_id]);
                }
                else
                {
                    fighterTab.Fighter_Mii_Hat_ID.SelectedIndex = 0;
                }

                fighterTab.Fighter_Mii_Color_ID.IsEnabled = true;
                fighterTab.Fighter_Mii_Color_ID.SelectedIndex = fighterTab.Fighter_Mii_Color_ID.Items.IndexOf
                    (SpiritValueDisplay.MiiColors[fighter.mii_color]);

                fighterTab.Fighter_Mii_Voice_ID.IsEnabled = true;

                if(fighter.mii_voice == 33451952054)
                {
                    fighter.mii_voice = 44330854164;
                }

                fighterTab.Fighter_Mii_Voice_ID.SelectedIndex = fighterTab.Fighter_Mii_Voice_ID.Items.IndexOf
                    (SpiritValueDisplay.MiiVoices[fighter.mii_voice]);

                fighterTab.Fighter_Mii_NSpec.IsEnabled = true;
                fighterTab.Fighter_Mii_NSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.fighter_kind].neutralSpecial;
                fighterTab.Fighter_Mii_NSpec.SelectedIndex = fighter.mii_sp_n;
                fighterTab.Fighter_Mii_SSpec.IsEnabled = true;
                fighterTab.Fighter_Mii_SSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.fighter_kind].sideSpecial;
                fighterTab.Fighter_Mii_SSpec.SelectedIndex = fighter.mii_sp_s;
                fighterTab.Fighter_Mii_USpec.IsEnabled = true;
                fighterTab.Fighter_Mii_USpec.ItemsSource = SpiritValueDisplay.Miis[fighter.fighter_kind].upSpecial;
                fighterTab.Fighter_Mii_USpec.SelectedIndex = fighter.mii_sp_hi;
                fighterTab.Fighter_Mii_DSpec.IsEnabled = true;
                fighterTab.Fighter_Mii_DSpec.ItemsSource = SpiritValueDisplay.Miis[fighter.fighter_kind].downSpecial;
                fighterTab.Fighter_Mii_DSpec.SelectedIndex = fighter.mii_sp_lw;
            }
            else
            {
                if (fighterData.hasAlts) { costumeIndex = fighterTab.Fighter_Color.Value.ToString(); }

                fighterTab.Fighter_Color.Visibility = Visibility.Visible;
                fighterTab.Fighter_Color_Mii.Visibility = Visibility.Hidden;

                fighterTab.Fighter_Mii_Hat_ID.SelectedIndex = 0;
                fighterTab.Fighter_Mii_Hat_ID.IsEnabled = false;
                fighterTab.Fighter_Mii_Color_ID.SelectedIndex = 0;
                fighterTab.Fighter_Mii_Color_ID.IsEnabled = false;
                fighterTab.Fighter_Mii_Voice_ID.SelectedIndex = 0;
                fighterTab.Fighter_Mii_Voice_ID.IsEnabled = false;
                fighterTab.Fighter_Mii_Outfit_ID.ItemsSource = SpiritValueDisplay.NotMiis.Values;
                fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex = 0;
                fighterTab.Fighter_Mii_Outfit_ID.IsEnabled = false;

                fighterTab.Fighter_Mii_NSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                fighterTab.Fighter_Mii_NSpec.SelectedIndex = 0;
                fighterTab.Fighter_Mii_NSpec.IsEnabled = false;
                fighterTab.Fighter_Mii_SSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                fighterTab.Fighter_Mii_SSpec.SelectedIndex = 0;
                fighterTab.Fighter_Mii_SSpec.IsEnabled = false;
                fighterTab.Fighter_Mii_USpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                fighterTab.Fighter_Mii_USpec.SelectedIndex = 0;
                fighterTab.Fighter_Mii_USpec.IsEnabled = false;
                fighterTab.Fighter_Mii_DSpec.ItemsSource = SpiritValueDisplay.NotMiiSpecial.Values;
                fighterTab.Fighter_Mii_DSpec.SelectedIndex = 0;
                fighterTab.Fighter_Mii_DSpec.IsEnabled = false;
            }

            string localOverride = AppDomain.CurrentDomain.BaseDirectory + "Override/Stock/";
            string fighterName = "chara_2_" + fighterData.internalID + "_0" + costumeIndex + ".png";

            if (File.Exists(localOverride + fighterName))
            {
                Uri resourceUri = new Uri(localOverride + fighterName, UriKind.Absolute);
                fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(resourceUri);
            }
            else
            {
                Uri fighterUri = new Uri("/Icons/Stock/" + fighterName, UriKind.Relative);
                fighterTab.Fighter_Fighter_Image.Source = new BitmapImage(fighterUri);
            }

            fighterTab.Fighter_Spawn_Count.Value = fighter.appear_rule_count;
            fighterTab.Fighter_Spawn_Time.Value = fighter.appear_rule_time;
            fighterTab.Fighter_Stock.Value = fighter.stock;
            fighterTab.Fighter_Damage.Value = fighter.init_damage;
            fighterTab.Fighter_HP.Value = fighter.hp;
            fighterTab.Fighter_First_Appear.IsChecked = fighter.first_appear;
            fighterTab.Fighter_Mob.IsChecked = fighter.corps;

            fighterTab.Fighter_CPU_Level.Value = fighter.cpu_lv;
            fighterTab.Fighter_CPU_Attack.Value = fighter.attack;
            fighterTab.Fighter_CPU_Defense.Value = fighter.defense;
            fighterTab.Fighter_CPU_Scale.Value = (decimal)fighter.scale;
            fighterTab.Fighter_Fly_Rate.Value = (decimal)fighter.fly_rate;
            fighterTab.Fighter_Can_Use_Item.IsChecked = fighter.cpu_item_pick_up;
            fighterTab.Fighter_Can_Drop_Item.IsChecked = !fighter.invalid_drop;
            fighterTab.Fighter_Charge_FS.IsChecked = fighter.enable_charge_final;

            fighterTab.Fighter_Spawn_Type.SelectedIndex = fighterTab.Fighter_Spawn_Type.Items.
                IndexOf(SpiritValueDisplay.EntryTypes[fighter.entry_type]);

            if (!SpiritValueDisplay.CPUBehaviors.ContainsKey(fighter.cpu_sub_type))
            {
                fighterTab.Fighter_CPU_Type.SelectedIndex = 0;
            }
            else
            {
                fighterTab.Fighter_CPU_Type.SelectedIndex = fighterTab.Fighter_CPU_Type.Items.
                    IndexOf(SpiritValueDisplay.CPUBehaviors[fighter.cpu_type]);
            }

            if (!SpiritValueDisplay.CPUBehaviors.ContainsKey(fighter.cpu_sub_type))
            {
                fighterTab.Fighter_CPU_Subtype.SelectedIndex = 0;
            }
            else
            {
                fighterTab.Fighter_CPU_Subtype.SelectedIndex = fighterTab.Fighter_CPU_Subtype.Items.
                    IndexOf(SpiritValueDisplay.CPUBehaviors[fighter.cpu_sub_type]);
            }

            if (!SpiritValueDisplay.SpiritAttributes.ContainsKey(fighter.attr))
                fighter.attr = 74193544331;

            var attr = SpiritValueDisplay.SpiritAttributes[fighter.attr];
            fighterTab.Fighter_CPU_Attribute.SelectedIndex = fighterTab.Fighter_CPU_Attribute.Items.
                IndexOf(attr);
            fighterTab.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));

            if (SpiritValueDisplay.Spirits.ContainsKey(fighter.spirit_name))
            {
                fighterTab.Fighter_CPU_Spirit.SelectedIndex = fighterTab.Fighter_CPU_Spirit.Items.
                    IndexOf(SpiritValueDisplay.Spirits[fighter.spirit_name]);
            }
            else
            {
                fighterTab.Fighter_CPU_Spirit.SelectedIndex = 0;
            }

            Uri attrUri = new Uri(attr.imageName, UriKind.Relative);
            fighterTab.Fighter_Attr_Image.Source = new BitmapImage(attrUri);

            fighterTab.Fighter_CPU_Ability_1.SelectedIndex = fighterTab.Fighter_CPU_Ability_1.Items.
                IndexOf(SpiritValueDisplay.SpiritAbilities[fighter.ability_1]);
            fighterTab.Spirit_Ability_1_Desc_Block.Text = SpiritValueDisplay.SpiritAbilities[fighter.ability_1].description;
            fighterTab.Fighter_CPU_Ability_2.SelectedIndex = fighterTab.Fighter_CPU_Ability_2.Items.
                IndexOf(SpiritValueDisplay.SpiritAbilities[fighter.ability_2]);
            fighterTab.Spirit_Ability_2_Desc_Block.Text = SpiritValueDisplay.SpiritAbilities[fighter.ability_2].description;
            fighterTab.Fighter_CPU_Ability_3.SelectedIndex = fighterTab.Fighter_CPU_Ability_3.Items.
                IndexOf(SpiritValueDisplay.SpiritAbilities[fighter.ability_3]);
            fighterTab.Spirit_Ability_3_Desc_Block.Text = SpiritValueDisplay.SpiritAbilities[fighter.ability_3].description;
            fighterTab.Fighter_CPU_Ability_Personal.SelectedIndex = fighterTab.Fighter_CPU_Ability_Personal.Items.
                IndexOf(SpiritValueDisplay.SpiritAbilities[fighter.ability_personal]);
            fighterTab.Spirit_Ability_P_Desc_Block.Text = SpiritValueDisplay.SpiritAbilities[fighter.ability_personal].description;

            if(fighter.sub_rule == 33451952054)
            {
                fighter.sub_rule = 51007861228;
            }

            fighterTab.Fighter_CPU_Subrule.SelectedIndex = fighterTab.Fighter_CPU_Subrule.Items.
                IndexOf(SpiritValueDisplay.CPUSubRules[fighter.sub_rule]);

            fighterTab.Fighter_Ignore_Num_Col.IsChecked = fighter.x0f2077926c;
        }


        public void DeleteFighter(object sender, RoutedEventArgs e)
        {
            string tabName = (sender as Button).CommandParameter.ToString();
            var item = Fighter_Tab_Control.Items.Cast<TabItem>().Where(i => i.Name.Equals(tabName)).SingleOrDefault();
            TabItem tab = item as TabItem;

            if(tab != null)
            {
                if(Fighter_Tab_Control.Items.Count == 1)
                {
                    MessageBox.Show("A battle must have at least one Fighter!", "Oops!");
                }
                else
                {
                    MessageBoxResult result = 
                        MessageBox.Show("Are you sure you wish to remove this Fighter?", "Hold Up", MessageBoxButton.YesNo);

                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        int selectionIndex = Fighter_Tab_Control.SelectedIndex;
                        int tabIndex = Fighter_Tab_Control.Items.IndexOf(item);

                        // If there are no unsaved edits to the current spirit, copy the spirit.
                        if (!unsavedEdits.ContainsKey(currentSpirit.index))
                            unsavedEdits.Add(currentSpirit.index, GetMods(currentSpirit).Copy());

                        // Grab the values to edit
                        var spirit = unsavedEdits[currentSpirit.index];
                        var battle = spirit.displayingAltBattle ? spirit.alternateBattle : spirit.battle;

                        // Remove the fighter and update the display
                        battle.fighters.RemoveAt(tabIndex);

                        Fighter_Tab_Control.Items.Clear();
                        handleChanges = false;
                        DisplayBattle(battle);
                        handleChanges = true;

                        UpdateSpirit(sender, e);
                        Fighter_Tab_Control.SelectedIndex =
                            selectionIndex >= battle.fighters.Count ? battle.fighters.Count - 1 : selectionIndex;
                    }
                }
            }
        }


        public void AddNewFighter(object sender, RoutedEventArgs e)
        {
            if (!handleChanges) return; 
            
            TabItem tab = Fighter_Tab_Control.SelectedItem as TabItem;

            if (tab != null && tab.Name.Equals("newfighter"))
            {
                // If there are no unsaved edits to the current spirit, copy the spirit.
                if (!unsavedEdits.ContainsKey(currentSpirit.index))
                    unsavedEdits.Add(currentSpirit.index, GetMods(currentSpirit));

                // Grab the values to edit
                var spirit = unsavedEdits[currentSpirit.index];
                var battle = spirit.displayingAltBattle ? spirit.alternateBattle : spirit.battle;

                battle.fighters.Add(new SpiritBattleFighter(battle.battle_id));

                Fighter_Tab_Control.Items.Clear();
                handleChanges = false;
                DisplayBattle(battle);
                handleChanges = true;

                UpdateSpirit(sender, e);
                Fighter_Tab_Control.SelectedIndex = battle.fighters.Count - 1;
            }
        }


        /// <summary>
        /// Updates the combobox for the stage form
        /// </summary>
        /// <param name="stage">The stage to derive data from</param>
        public void UpdateStageFormCombobox(SpiritValueDisplay.BattleStageID stage)
        {
            var forms = stage.stageForms;
            Battle_Stage_Form.ItemsSource = forms.Values;
        }


        /// <summary>
        /// Updates the combobox for the given event
        /// </summary>
        /// <param name="eventBox">The combobox to update</param>
        /// <param name="battleEvent">The event data to use</param>
        public void UpdateEventCombobox(ComboBox comboBox, SpiritValueDisplay.BattleEvent battleEvent)
        {
            var param = battleEvent.eventParams;
            comboBox.ItemsSource = param.Values;
        }


        /// <summary>
        /// Updates the edited values of the current spirit. Note that this does not save the values.
        /// </summary>
        public void UpdateSpirit(object sender, EventArgs e)
        {
            if (!handleChanges)
                return;

            // If there are no unsaved edits to the current spirit, copy the spirit.
            if (!unsavedEdits.ContainsKey(currentSpirit.index))
                unsavedEdits.Add(currentSpirit.index, GetMods(currentSpirit).Copy());

            // Update the spirit values
            var spiritVals = unsavedEdits[currentSpirit.index];
            spiritVals.display_name = Spirit_Name.Text;
            spiritVals.type = ((SpiritValueDisplay.SpiritType)Spirit_Type.Items[Spirit_Type.SelectedIndex]).id;
            spiritVals.spirit_attr = ((SpiritValueDisplay.SpiritAttribute)Spirit_Attribute.Items[Spirit_Attribute.SelectedIndex]).id;
            spiritVals.rank = ((SpiritValueDisplay.SpiritRank)Spirit_Rank.Items[Spirit_Rank.SelectedIndex]).id;
            spiritVals.slots = Convert.ToByte(Spirit_Slots.Value.Value);
            spiritVals.ability_id = ((SpiritValueDisplay.SpiritAbility)Spirit_Ability.Items[Spirit_Ability.SelectedIndex]).id;
            spiritVals.super_ability = ((SpiritValueDisplay.SpiritAbility)Spirit_SuperAbility.Items[Spirit_SuperAbility.SelectedIndex]).id;

            spiritVals.exp_lvl_max = Convert.ToUInt32(Spirit_Exp.Value);
            spiritVals.exp_up_rate = Convert.ToSingle(Spirit_Lvl_Rate.Value);
            spiritVals.base_attack = Convert.ToInt16(Spirit_Base_Atk.Value);
            spiritVals.max_attack = Convert.ToInt16(Spirit_Max_Atk.Value);
            spiritVals.base_defense = Convert.ToInt16(Spirit_Base_Def.Value);
            spiritVals.max_defense = Convert.ToInt16(Spirit_Max_Def.Value);
            spiritVals.battle_exp = Convert.ToUInt32(Spirit_Reward_EXP.Value);
            spiritVals.reward_capacity = Convert.ToUInt32(Spirit_Reward_SP.Value);

            spiritVals.series_id = ((SpiritValueDisplay.SpiritSeriesID)Spirit_Series.Items[Spirit_Series.SelectedIndex]).id;
            spiritVals.game_title = ((SpiritValueDisplay.SpiritGameTitle)Spirit_Game_ID.Items[Spirit_Game_ID.SelectedIndex]).id;
            spiritVals.appear_conditions = ((SpiritValueDisplay.SpiritAppearCondition)Spirit_BoardConditions.
                Items[Spirit_BoardConditions.SelectedIndex]).id;
            spiritVals.is_board_appear = Spirit_OnBoard.IsChecked.HasValue ? Spirit_OnBoard.IsChecked.Value : false;
            spiritVals.rematch = Spirit_Is_Rematch.IsChecked.HasValue ? Spirit_Is_Rematch.IsChecked.Value : false;

            if (Spirit_Index.Value.HasValue)
            {
                ushort org = spiritVals.directory_id;
                spiritVals.directory_id = Convert.ToUInt16(Spirit_Index.Value.Value);

                // If value is changed, attempt autonumber
                if(org != spiritVals.directory_id)
                    Autonumber(spiritVals, org, spiritVals.directory_id);
            }
            else
                spiritVals.directory_id = 0;

            spiritVals.evolve_src = ((SpiritValueDisplay.SpiritID)Spirit_Evolves_From.Items[Spirit_Evolves_From.SelectedIndex]).id;
            spiritVals.summon_id1 = ((SpiritValueDisplay.SpiritID)Spirit_Core_1_ID.Items[Spirit_Core_1_ID.SelectedIndex]).id;
            spiritVals.summon_id2 = ((SpiritValueDisplay.SpiritID)Spirit_Core_2_ID.Items[Spirit_Core_2_ID.SelectedIndex]).id;
            spiritVals.summon_id3 = ((SpiritValueDisplay.SpiritID)Spirit_Core_3_ID.Items[Spirit_Core_3_ID.SelectedIndex]).id;
            spiritVals.summon_id4 = ((SpiritValueDisplay.SpiritID)Spirit_Core_4_ID.Items[Spirit_Core_4_ID.SelectedIndex]).id;
            spiritVals.summon_id5 = ((SpiritValueDisplay.SpiritID)Spirit_Core_5_ID.Items[Spirit_Core_5_ID.SelectedIndex]).id;
            spiritVals.summon_quantity1 = Convert.ToByte(Spirit_Core_1_Quantity.Value);
            spiritVals.summon_quantity2 = Convert.ToByte(Spirit_Core_2_Quantity.Value);
            spiritVals.summon_quantity3 = Convert.ToByte(Spirit_Core_3_Quantity.Value);
            spiritVals.summon_quantity4 = Convert.ToByte(Spirit_Core_4_Quantity.Value);
            spiritVals.summon_quantity5 = Convert.ToByte(Spirit_Core_5_Quantity.Value);
            spiritVals.summon_sp = Convert.ToUInt32(Spirit_SummonSP.Value);
            spiritVals.shop_price = Convert.ToUInt32(Spirit_Price.Value);
            spiritVals.shop_sales_type = ((SpiritValueDisplay.SpiritSalesType)Spirit_SaleType.
                Items[Spirit_SaleType.SelectedIndex]).id;

            if(spiritVals.displayingAltBattle)
                UpdateBattle(spiritVals.alternateBattle);
            else
                UpdateBattle(spiritVals.battle);

            UpdateLayout(spiritVals.aesthetics);

            // Mark the spirit as edited
            spiritButtons[currentSpirit.index].FontStyle = FontStyles.Italic;
            spiritButtons[currentSpirit.index].Content = Spirit_Name.Text;

            SetWoLToggle(spiritVals);
        }


        public void Autonumber(Spirit target, ushort originalIndex, ushort newIndex)
        {
            if (target.Custom)
            {
                int dir = originalIndex > newIndex ? 1 : -1;

                foreach (var spirit in modList.Values)
                {
                    // Skip the target spirit
                    if (!spirit.Custom || spirit.spirit_id == target.spirit_id) continue;

                    var s = GetMods(spirit);

                    if (s.directory_id >= Math.Min(originalIndex, newIndex) &&
                        s.directory_id <= Math.Max(originalIndex, newIndex))
                    {
                        // Edit the spirit's number and add it to the mod list
                        if (!unsavedEdits.ContainsKey(s.index))
                            unsavedEdits.Add(s.index, s.Copy());

                        var newS = unsavedEdits[s.index];

                        int directory_id = (int)newS.directory_id;
                        directory_id += dir;
                        newS.directory_id = Convert.ToUInt16(directory_id);

                        spiritButtons[newS.index].FontStyle = FontStyles.Italic;
                    }
                }
                return;
            }

            if((baseSpirits[target.spirit_id].directory_id > MAX_EDITABLE_INDEX || newIndex > MAX_EDITABLE_INDEX))
            {
                return;
            }

            int direction = originalIndex > newIndex ? 1 : -1;

            foreach (var spirit in baseSpirits.Values)
            {
                // Skip the target spirit
                if (spirit.spirit_id == target.spirit_id) continue;

                var s = GetMods(spirit);

                // Modify the spirit's index IF:
                // 1.) The spirit's modded directory is greater than the max allowed
                // 2.) It is within the range of the original and new indexes
                if (s.directory_id <= MAX_EDITABLE_INDEX && s.directory_id >= Math.Min(originalIndex, newIndex) && 
                    s.directory_id <= Math.Max(originalIndex, newIndex))
                {
                    // Edit the spirit's number and add it to the mod list
                    if (!unsavedEdits.ContainsKey(s.index))
                        unsavedEdits.Add(s.index, s.Copy());

                    var newS = unsavedEdits[s.index];

                    int directory_id = (int)newS.directory_id;
                    directory_id += direction;
                    newS.directory_id = Convert.ToUInt16(directory_id);

                    spiritButtons[newS.index].FontStyle = FontStyles.Italic;
                }
            }
        }


        public void UpdateBattle(SpiritBattle battle)
        {
            battle.battle_type = ((SpiritValueDisplay.BattleType)Battle_Type.Items[Battle_Type.SelectedIndex]).id;
            battle.result_type = ((SpiritValueDisplay.BattleWinCondition)Battle_Win_Condition.Items[Battle_Win_Condition.SelectedIndex]).id;
            battle.battle_power = Convert.ToUInt32(Battle_Power.Value);
            battle.battle_time_sec = Convert.ToUInt16(Battle_Time.Value);
            battle.basic_stock = Convert.ToByte(Battle_Stock.Value);
            battle.basic_init_hp = Convert.ToUInt16(Battle_Init_HP.Value);
            battle.basic_init_damage = Convert.ToUInt16(Battle_Init_Damage.Value);

            battle.stage_id = ((SpiritValueDisplay.BattleStageID)Battle_Stage_ID.Items[Battle_Stage_ID.SelectedIndex]).id;
            battle.stage_type = ((SpiritValueDisplay.BattleStageType)Battle_Stage_Type.Items[Battle_Stage_Type.SelectedIndex]).id;
            if (Battle_Stage_Form.SelectedIndex == -1) Battle_Stage_Form.SelectedIndex = 0;
            battle.x18e536d4f7 = Convert.ToSByte(
                ((SpiritValueDisplay.BattleStageForm)Battle_Stage_Form.Items[Battle_Stage_Form.SelectedIndex]).id);
            battle.stage_attr = ((SpiritValueDisplay.BattleStageAttribute)Battle_Stage_Attr.Items[Battle_Stage_Attr.SelectedIndex]).id;
            battle.floor_place_id = ((SpiritValueDisplay.BattleFloorPlace)Battle_Floor.Items[Battle_Floor.SelectedIndex]).id;
            battle.item_table = ((SpiritValueDisplay.BattleItemTable)Battle_Item_Table.Items[Battle_Item_Table.SelectedIndex]).id;
            battle.item_level = ((SpiritValueDisplay.BattleItemLevel)Battle_Item_Level.Items[Battle_Item_Level.SelectedIndex]).id;
            battle.stage_bgm = ((SpiritValueDisplay.BattleBGM)Battle_BGM.Items[Battle_BGM.SelectedIndex]).id;
            battle.stage_gimmick = Battle_Hazards.IsChecked.HasValue ? Battle_Hazards.IsChecked.Value : false;

            battle.event1_type = ((SpiritValueDisplay.BattleEvent)Event_1_Type.Items[Event_1_Type.SelectedIndex]).id;
            if(Event_1_Label.SelectedIndex == -1) Event_1_Label.SelectedIndex = 0;
            battle.event1_label = ((SpiritValueDisplay.BattleEventParam)Event_1_Label.Items[Event_1_Label.SelectedIndex]).id;
            battle.event1_start_time = Convert.ToInt32(Event_1_Start_Time.Value);
            battle.event1_range_time = Convert.ToInt32(Event_1_Range_Time.Value);
            battle.event1_count = Convert.ToByte(Event_1_Count.Value);
            battle.event1_damage = Convert.ToUInt16(Event_1_Damage.Value);

            battle.event2_type = ((SpiritValueDisplay.BattleEvent)Event_2_Type.Items[Event_2_Type.SelectedIndex]).id;
            if (Event_2_Label.SelectedIndex == -1) Event_2_Label.SelectedIndex = 0;
            battle.event2_label = ((SpiritValueDisplay.BattleEventParam)Event_2_Label.Items[Event_2_Label.SelectedIndex]).id;
            battle.event2_start_time = Convert.ToInt32(Event_2_Start_Time.Value);
            battle.event2_range_time = Convert.ToInt32(Event_2_Range_Time.Value);
            battle.event2_count = Convert.ToByte(Event_2_Count.Value);
            battle.event2_damage = Convert.ToUInt16(Event_2_Damage.Value);

            battle.event3_type = ((SpiritValueDisplay.BattleEvent)Event_3_Type.Items[Event_3_Type.SelectedIndex]).id;
            if (Event_3_Label.SelectedIndex == -1) Event_3_Label.SelectedIndex = 0;
            battle.event3_label = ((SpiritValueDisplay.BattleEventParam)Event_3_Label.Items[Event_3_Label.SelectedIndex]).id;
            battle.event3_start_time = Convert.ToInt32(Event_3_Start_Time.Value);
            battle.event3_range_time = Convert.ToInt32(Event_3_Range_Time.Value);
            battle.event3_count = Convert.ToByte(Event_3_Count.Value);
            battle.event3_damage = Convert.ToUInt16(Event_3_Damage.Value);

            battle.auto_win_skill = ((SpiritValueDisplay.SpiritAbility)Battle_Auto_Win_Skill.Items[Battle_Auto_Win_Skill.SelectedIndex]).id;
            battle.x18404d4ecb = ((SpiritValueDisplay.SpiritAbility)Battle_Auto_Win_Skill_2.Items[Battle_Auto_Win_Skill_2.SelectedIndex]).id;
            battle.recommended_skills[Battle_Recommended.SelectedIndex] =
                ((SpiritValueDisplay.SpiritAbility)Battle_Recommended_Skill.Items[Battle_Recommended_Skill.SelectedIndex]).id;
            battle.un_recommended_skills[Battle_Unrecommended.SelectedIndex] =
                ((SpiritValueDisplay.SpiritAbility)Battle_Unrecommended_Skill.Items[Battle_Unrecommended_Skill.SelectedIndex]).id;

            battle.x0d41ef8328 = Spirit_Hint1.IsChecked.HasValue ? Spirit_Hint1.IsChecked.Value : false;
            battle.aw_flap_delay = Spirit_Hint2.IsChecked.HasValue ? Spirit_Hint2.IsChecked.Value : false;
            battle.x0d6f19abae = Spirit_Hint3.IsChecked.HasValue ? Spirit_Hint3.IsChecked.Value : false;

            for(int i=0; i<battle.fighters.Count; i++)
            {
                UpdateFighter(battle.fighters[i], (FighterTab)((TabItem)Fighter_Tab_Control.Items[i]).Content);
            }
        }


        public void UpdateLayout(SpiritAesthetics layout)
        {
            int imageIndex = Image_Pos_Type.SelectedIndex;

            layout.art_pos_x[imageIndex] = Convert.ToSingle(Image_X.Value);
            layout.art_pos_y[imageIndex] = Convert.ToSingle(Image_Y.Value);
            layout.art_size[imageIndex] = Convert.ToSingle(Image_Scale.Value);

            int effectIndex = Effect_Index.SelectedIndex;

            layout.effect_count = Convert.ToUInt32(Effect_Count.Value);
            layout.effect_pos_x[effectIndex] = Convert.ToInt32(Effect_X.Value);
            layout.effect_pos_y[effectIndex] = Convert.ToInt32(Effect_Y.Value);

            if (!string.IsNullOrEmpty(CurrentPreviewSource))
            {
                ApplyCurrentImageOffsets();
            }
        }


        public void UpdateFighter(SpiritBattleFighter fighter, FighterTab fighterTab)
        {
            var fighterData = SpiritValueDisplay.FighterIDs[fighter.fighter_kind];
            fighter.fighter_kind = ((SpiritValueDisplay.FighterID)fighterTab.Fighter_ID.Items[fighterTab.Fighter_ID.SelectedIndex]).id;

            if (fighterData.hasAlts || SpiritValueDisplay.Miis.ContainsKey(fighter.fighter_kind))
            {
                fighter.color = SpiritValueDisplay.Miis.ContainsKey(fighter.fighter_kind) ? Convert.ToByte(((SpiritValueDisplay.MiiData.MiiOutfit)
                    fighterTab.Fighter_Color_Mii.Items[fighterTab.Fighter_Color_Mii.SelectedIndex]).id) :
                    Convert.ToByte(fighterTab.Fighter_Color.Value);
            }
            else
            {
                fighter.color = 0;
            }

            fighter.entry_type = ((SpiritValueDisplay.CPUEntryType)fighterTab.Fighter_Spawn_Type.
                Items[fighterTab.Fighter_Spawn_Type.SelectedIndex]).id;
            fighter.appear_rule_time = Convert.ToUInt16(fighterTab.Fighter_Spawn_Time.Value);
            fighter.appear_rule_count = Convert.ToUInt16(fighterTab.Fighter_Spawn_Count.Value);
            fighter.first_appear = fighterTab.Fighter_First_Appear.IsChecked.HasValue ? fighterTab.Fighter_First_Appear.IsChecked.Value : false;
            fighter.corps = fighterTab.Fighter_Mob.IsChecked.HasValue ? fighterTab.Fighter_Mob.IsChecked.Value : false;
            fighter.stock = Convert.ToByte(fighterTab.Fighter_Stock.Value);
            fighter.hp = Convert.ToUInt16(fighterTab.Fighter_HP.Value);
            fighter.init_damage = Convert.ToByte(fighterTab.Fighter_Damage.Value);
            
            fighter.cpu_lv = Convert.ToByte(fighterTab.Fighter_CPU_Level.Value);
            fighter.cpu_type = ((SpiritValueDisplay.CPUBehavior)fighterTab.Fighter_CPU_Type.
                Items[fighterTab.Fighter_CPU_Type.SelectedIndex]).id;
            fighter.cpu_sub_type = ((SpiritValueDisplay.CPUBehavior)fighterTab.Fighter_CPU_Subtype.
                Items[fighterTab.Fighter_CPU_Subtype.SelectedIndex]).id;
            fighter.cpu_item_pick_up = fighterTab.Fighter_Can_Use_Item.IsChecked.HasValue ? 
                fighterTab.Fighter_Can_Use_Item.IsChecked.Value : false;
            fighter.invalid_drop = fighterTab.Fighter_Can_Drop_Item.IsChecked.HasValue ?
                !fighterTab.Fighter_Can_Drop_Item.IsChecked.Value : false;
            fighter.enable_charge_final = fighterTab.Fighter_Charge_FS.IsChecked.HasValue ? 
                fighterTab.Fighter_Charge_FS.IsChecked.Value : false;

            var attr = ((SpiritValueDisplay.SpiritAttribute)fighterTab.Fighter_CPU_Attribute.
                Items[fighterTab.Fighter_CPU_Attribute.SelectedIndex]);
            fighter.attr = attr.id;
            fighterTab.Background = new SolidColorBrush(Color.FromArgb(0xFF, attr.bgR, attr.bgG, attr.bgB));

            fighter.attack = Convert.ToInt16(fighterTab.Fighter_CPU_Attack.Value);
            fighter.defense = Convert.ToInt16(fighterTab.Fighter_CPU_Defense.Value);
            fighter.scale = Convert.ToSingle(fighterTab.Fighter_CPU_Scale.Value);
            fighter.fly_rate = Convert.ToSingle(fighterTab.Fighter_Fly_Rate.Value);
            fighter.spirit_name = ((SpiritValueDisplay.SpiritID)fighterTab.Fighter_CPU_Spirit.
                Items[fighterTab.Fighter_CPU_Spirit.SelectedIndex]).id;
            fighter.ability_1 = ((SpiritValueDisplay.SpiritAbility)fighterTab.Fighter_CPU_Ability_1.
                Items[fighterTab.Fighter_CPU_Ability_1.SelectedIndex]).id;
            fighter.ability_2 = ((SpiritValueDisplay.SpiritAbility)fighterTab.Fighter_CPU_Ability_2.
                Items[fighterTab.Fighter_CPU_Ability_2.SelectedIndex]).id;
            fighter.ability_3 = ((SpiritValueDisplay.SpiritAbility)fighterTab.Fighter_CPU_Ability_3.
                Items[fighterTab.Fighter_CPU_Ability_3.SelectedIndex]).id;
            fighter.ability_personal = ((SpiritValueDisplay.SpiritAbility)fighterTab.Fighter_CPU_Ability_Personal.
                Items[fighterTab.Fighter_CPU_Ability_Personal.SelectedIndex]).id;
            fighter.sub_rule = ((SpiritValueDisplay.CPUSubRule)fighterTab.Fighter_CPU_Subrule.
                Items[fighterTab.Fighter_CPU_Subrule.SelectedIndex]).id;

            if (fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex == -1) fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex = 0;
            fighter.mii_body_id = ((SpiritValueDisplay.MiiData.MiiOutfit)fighterTab.Fighter_Mii_Outfit_ID.
                Items[fighterTab.Fighter_Mii_Outfit_ID.SelectedIndex]).id;
            fighter.mii_hat_id = ((SpiritValueDisplay.MiiData.MiiOutfit)fighterTab.Fighter_Mii_Hat_ID.
                Items[fighterTab.Fighter_Mii_Hat_ID.SelectedIndex]).id;
            fighter.mii_voice = ((SpiritValueDisplay.MiiVoice)fighterTab.Fighter_Mii_Voice_ID.
                Items[fighterTab.Fighter_Mii_Voice_ID.SelectedIndex]).id;
            fighter.mii_color = Convert.ToByte(((SpiritValueDisplay.MiiData.MiiOutfit)fighterTab.Fighter_Mii_Color_ID.
                Items[fighterTab.Fighter_Mii_Color_ID.SelectedIndex]).id);
            if (fighterTab.Fighter_Mii_NSpec.SelectedIndex == -1) fighterTab.Fighter_Mii_NSpec.SelectedIndex = 0;
            fighter.mii_sp_n = Convert.ToByte(((SpiritValueDisplay.MiiData.MiiSpecial)fighterTab.Fighter_Mii_NSpec.
                Items[fighterTab.Fighter_Mii_NSpec.SelectedIndex]).id);
            if (fighterTab.Fighter_Mii_SSpec.SelectedIndex == -1) fighterTab.Fighter_Mii_SSpec.SelectedIndex = 0;
            fighter.mii_sp_s = Convert.ToByte(((SpiritValueDisplay.MiiData.MiiSpecial)fighterTab.Fighter_Mii_SSpec.
                Items[fighterTab.Fighter_Mii_SSpec.SelectedIndex]).id);
            if (fighterTab.Fighter_Mii_USpec.SelectedIndex == -1) fighterTab.Fighter_Mii_USpec.SelectedIndex = 0;
            fighter.mii_sp_hi = Convert.ToByte(((SpiritValueDisplay.MiiData.MiiSpecial)fighterTab.Fighter_Mii_USpec.
                Items[fighterTab.Fighter_Mii_USpec.SelectedIndex]).id);
            if (fighterTab.Fighter_Mii_DSpec.SelectedIndex == -1) fighterTab.Fighter_Mii_DSpec.SelectedIndex = 0;
            fighter.mii_sp_lw = Convert.ToByte(((SpiritValueDisplay.MiiData.MiiSpecial)fighterTab.Fighter_Mii_DSpec.
                Items[fighterTab.Fighter_Mii_DSpec.SelectedIndex]).id);

            fighter.x0f2077926c = fighterTab.Fighter_Ignore_Num_Col.IsChecked.HasValue ? 
                fighterTab.Fighter_Ignore_Num_Col.IsChecked.Value : false;
        }


        public string BytesToDisplayName(byte[] bytes)
        {
            string result = "";

            for (int i = 0; i < bytes.Length; i+=2)
            {
                if(i < bytes.Length - 8)
                {
                    // If we've reached the end of line 1, the string has been decoded
                    if (bytes[i] == 14 && bytes[i + 2] == 1 && bytes[i + 4] == 10)
                        break;

                    // If the sequence indicates Smash parentheses formatting, skip past
                    else if ((bytes[i] == 14 && bytes[i + 4] == 2 && bytes[i + 6] == 2 && bytes[i + 8] == 80) ||
                        (bytes[i] == 14 && bytes[i + 4] == 2 && bytes[i + 6] == 2 && bytes[i + 8] == 100))
                    {
                        i += 8;
                        continue;
                    }
                }

                // If the character is null, advance
                else if (bytes[i] == 0)
                    continue;

                // Decode the character and add
                result += Convert.ToChar((bytes[i+1] << 8) + bytes[i]);
            }

            return result;
        }


        public string BytesToName(byte[] bytes)
        {
            string result = "";

            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                // Decode the character and add
                result += Convert.ToChar((bytes[i + 1] << 8) + bytes[i]);
            }

            return result;
        }


        public byte[] NameToBytes(string name, bool bAddEscape = true)
        {
            string alpha = FormatAlphabeticalSortName(name);
            if (alpha.Length > 126)
                alpha = alpha.Substring(0, 126);

            List<byte> result = new List<byte>();

            // Keep track of parentheses
            int openPar = 0, closePar = 0;
            bool par = false;

            // Encode first line
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];

                if(i < name.Length - 1 && name[i+1] == '(' && !par)
                {
                    openPar++;
                    par = true;
                    // Add pre-open sequence
                    result.Add(14);
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    result.Add(2);
                    result.Add(0);
                    result.Add(2);
                    result.Add(0);
                    result.Add(80);
                    result.Add(0);
                }

                uint cVal = Convert.ToUInt32(c);
                byte b2 = Convert.ToByte(cVal >> 8);
                byte b1 = Convert.ToByte(cVal - (b2 << 8));

                result.Add(b1);
                result.Add(b2);
                
                // Check for ) and if ( preceded it
                if(c == ')' && closePar < openPar)
                {
                    closePar++;
                    par = false;
                    // Add post-close sequence
                    result.Add(14);
                    result.Add(0);
                    result.Add(0);
                    result.Add(0);
                    result.Add(2);
                    result.Add(0);
                    result.Add(2);
                    result.Add(0);
                    result.Add(100);
                    result.Add(0);
                }
            }

            // check if the number of open parentheses exceeds the number of close parentheses
            for(int i=closePar; i<openPar; i++)
            {
                result.Add(14);
                result.Add(0);
                result.Add(0);
                result.Add(0);
                result.Add(2);
                result.Add(0);
                result.Add(2);
                result.Add(0);
                result.Add(80);
                result.Add(0);
            }

            if (!bAddEscape)
            {
                result.Add(00);
                result.Add(00);
                return result.ToArray();
            }

            // Add end of line escape
            result.Add(14);
            result.Add(00);
            result.Add(01);
            result.Add(00);
            result.Add(10);
            result.Add(00);

            // Add alphanumeric header
            result.Add(Convert.ToByte((alpha.Length * 2) + 2));
            result.Add(00);
            result.Add(Convert.ToByte(alpha.Length * 2));
            result.Add(00);

            // Encode alphanumeric
            for (int i = 0; i < alpha.Length; i++)
            {
                char c = alpha[i];

                uint cVal = Convert.ToUInt32(c);
                byte b2 = Convert.ToByte(cVal >> 8);
                byte b1 = Convert.ToByte(cVal - (b2 << 8));

                result.Add(b1);
                result.Add(b2);
            }

            result.Add(00);
            result.Add(00);

            return result.ToArray();
        }


        private string FormatAlphabeticalSortName(string name)
        {
            name = name.ToLower();
            name = name.Replace("0", "zero");
            name = name.Replace("1", "one");
            name = name.Replace("2", "two");
            name = name.Replace("3", "three");
            name = name.Replace("4", "four");
            name = name.Replace("5", "five");
            name = name.Replace("6", "six");
            name = name.Replace("7", "seven");
            name = name.Replace("8", "eight");
            name = name.Replace("9", "nine");
            name = Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty);

            return name;
        }


        private void Spirit_Default_Click(object sender, RoutedEventArgs e)
        {
            if (currentSpirit.Custom)
            {
                DeleteNewSpirit(sender, e);
                return;
            }

            if (!unsavedEdits.ContainsKey(currentSpirit.index) && !modList.ContainsKey(currentSpirit.index))
            {
                MessageBox.Show(currentSpirit.display_name + " is unchanged.", "Oops!");
                return;
            }

            MessageBoxResult result =
                MessageBox.Show("This will revert " + currentSpirit.display_name + " to the default values.\n" +
                "If mods are saved, any changes to this Spirit will be lost.\nAre you sure?", 
                "Hold Up", MessageBoxButton.YesNo);

            if (result.Equals(MessageBoxResult.Yes))
            {
                // Remove the unsaved edits
                if(unsavedEdits.ContainsKey(currentSpirit.index))
                    unsavedEdits.Remove(currentSpirit.index);

                if (modList.ContainsKey(currentSpirit.index))
                {
                    // Revert all mods
                    modList.Remove(currentSpirit.index);
                }

                // Refresh the display
                DisplaySpirit(currentSpirit);
                spiritButtons[currentSpirit.index].Content = baseSpirits[currentSpirit.spirit_id].display_name;
                spiritButtons[currentSpirit.index].FontWeight = FontWeights.Normal;
            }
        }


        private void Spirit_Revert_Click(object sender, RoutedEventArgs e)
        {
            if (!unsavedEdits.ContainsKey(currentSpirit.index))
            {
                MessageBox.Show(currentSpirit.display_name + " is unchanged.", "Oops!");
                return;
            }

            MessageBoxResult result =
                MessageBox.Show("This will revert all unsaved changes to " + currentSpirit.display_name + ".\nAre you sure?",
                "Hold Up", MessageBoxButton.YesNo);

            if (result.Equals(MessageBoxResult.Yes))
            {
                // Remove the unsaved edits
                unsavedEdits.Remove(currentSpirit.index);

                // Refresh the display
                DisplaySpirit(currentSpirit);

                if (modList.ContainsKey(currentSpirit.index))
                    spiritButtons[currentSpirit.index].FontWeight = FontWeights.Bold;
                else
                    spiritButtons[currentSpirit.index].FontWeight = FontWeights.Normal;

                spiritButtons[currentSpirit.index].FontStyle = FontStyles.Normal;
                spiritButtons[currentSpirit.index].Content = GetMods(currentSpirit).display_name;
            }
        }


        private void Spirit_Commit_Click(object sender, RoutedEventArgs e)
        {
            foreach(var spirit in unsavedEdits.Values)
            {
                if (modList.ContainsKey(spirit.index))
                    modList[spirit.index] = spirit;
                else
                    modList.Add(spirit.index, spirit);

                spiritButtons[spirit.index].FontWeight = FontWeights.Bold;
                spiritButtons[spirit.index].FontStyle = FontStyles.Normal;
            }

            unsavedEdits.Clear();

            // Save a JSON representation of the spirit mods
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            // Get the directory
            string path = AppDomain.CurrentDomain.BaseDirectory + "Data";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Export to JSON
            using (StreamWriter sw = new StreamWriter(path + @"/mods.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, modList);
            }

            // Save a JSON representation of the World of Light list
            serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Export to JSON
            using (StreamWriter sw = new StreamWriter(path + @"/wollist.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, WoLSpirits);
            }

            MessageBox.Show("All mods saved.", "Save Successful");

            // Refresh the display
            DisplaySpirit(currentSpirit);
        }


        private void Stats_Info_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Stats"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Stats");
            }

            string formattedOutput = "";
            string spiritStats = "Name,Type,Rank,Attribute,Series,Game Title,Stage,Stage Type,BGM,Fighter,Fighter Color,\n";
            Dictionary<string, int> fighterCount = new Dictionary<string, int>();
            Dictionary<string, int> fighterMain = new Dictionary<string, int>();

            foreach(var fighter in SpiritValueDisplay.FighterIDs.Values)
            {
                fighterCount.Add(fighter.name, 0);
                fighterMain.Add(fighter.name, 0);
            }

            fighterCount.Add("Unknown", 0);
            fighterMain.Add("Unknown", 0);

            foreach(var button in spiritButtons.Values)
            {
                var mod = GetMods(button.spirit);

                if(mod.evolve_src != 8403017505)
                {
                    if (WoLSpirits.Contains(GetMods(mod.evolve_src).save_no))
                    {
                        formattedOutput += $"[EVOLVE] {mod.display_name} (";

                        if (SpiritValueDisplay.SpiritGameTitles[mod.game_title].name == "None")
                        {
                            formattedOutput += $"{SpiritValueDisplay.SpiritSeries[mod.series_id].name} Series)\n";
                        }
                        else
                        {
                            formattedOutput += $"{SpiritValueDisplay.SpiritGameTitles[mod.game_title].name})\n";
                        }
                    }

                    continue;
                }

                if(mod.type == 83372998134)
                {
                    formattedOutput += $"[MASTER] {mod.display_name} (";

                    if (SpiritValueDisplay.SpiritGameTitles[mod.game_title].name == "None")
                    {
                        formattedOutput += $"{SpiritValueDisplay.SpiritSeries[mod.series_id].name} Series)\n";
                    }
                    else
                    {
                        formattedOutput += $"{SpiritValueDisplay.SpiritGameTitles[mod.game_title].name})\n";
                    }
                }

                if (WoLSpirits.Contains(mod.save_no))
                {
                    formattedOutput += $"{mod.display_name} (";

                    if(SpiritValueDisplay.SpiritGameTitles[mod.game_title].name == "None")
                    {
                        formattedOutput += $"{SpiritValueDisplay.SpiritSeries[mod.series_id].name} Series)\n";
                    }
                    else
                    {
                        formattedOutput += $"{SpiritValueDisplay.SpiritGameTitles[mod.game_title].name})\n";
                    }

                    spiritStats += $"\"{mod.display_name}\"," +
                        $"{SpiritValueDisplay.SpiritTypes[mod.type].name}," +
                        $"{SpiritValueDisplay.SpiritRanks[mod.rank].name}," +
                        $"{SpiritValueDisplay.SpiritAttributes[mod.spirit_attr].name}," +
                        $"\"{SpiritValueDisplay.SpiritSeries[mod.series_id].name}\"," +
                        $"\"{SpiritValueDisplay.SpiritGameTitles[mod.game_title].name}\"," +
                        $"\"{SpiritValueDisplay.BattleStageIDs[mod.battle.stage_id].name}\"," +
                        $"\"{SpiritValueDisplay.BattleStageTypes[mod.battle.stage_type].name}\"," +
                        $"\"{SpiritValueDisplay.BattleBGMIDs[mod.battle.stage_bgm].name}\",";

                    foreach(var fighter in mod.battle.fighters)
                    {
                        var fighterData = SpiritValueDisplay.FighterIDs.ContainsKey(fighter.fighter_kind) ? 
                                SpiritValueDisplay.FighterIDs[fighter.fighter_kind] : null;
                        var fighterName = fighterData != null ? fighterData.name : "Unknown";

                        if (fighter.entry_type == 40473814347)
                        {
                            if (fighterName.Contains("Mii"))
                            {
                                spiritStats += $"{fighterName},,";
                            }
                            else
                            {
                                spiritStats += $"{fighterName},{(fighter.color + 1)},";
                            }

                            fighterMain[fighterName] += 1;
                        }

                        fighterCount[fighterName] += 1;
                    }

                    spiritStats += "\n";
                }
            }

            string fighterStats = "Fighter,Times Used,Times Main,\n";

            foreach (var fighter in SpiritValueDisplay.FighterIDs.Values)
            {
                fighterStats += $"{fighter.name},{fighterCount[fighter.name]},{fighterMain[fighter.name]},\n";
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Stats\spirit_stats.csv", 
                spiritStats.Replace("\0", string.Empty));
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Stats\fighter_stats.csv", 
                fighterStats.Replace("\0", string.Empty));
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Stats\spirit_list.csv",
                formattedOutput.Replace("\0", string.Empty));

            MessageBox.Show("Saved spirit stats to the /Stats directory.", "It's done.");
        }


        private void Open_Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Created by Tougon.\nimg2bntx ©2020 jam1garner\n" + 
                "All other images and assets © Nintendo", 
                "Application Info");
        }


        private void Spirit_Export_Click(object sender, RoutedEventArgs e)
        {
            var export = MessageBox.Show("Would you like to export all modified spirits?\n" +
                "Only saved mods will be exported.",
                "Hold Up", MessageBoxButton.YesNo);

            if(export == MessageBoxResult.No)
            {
                // Save a JSON representation of the spirit mods
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                // Get the directory
                string path = AppDomain.CurrentDomain.BaseDirectory + "Export";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                // Export to JSON
                using (StreamWriter sw = new StreamWriter(path + $@"/{currentSpirit.name_id}.tgns"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    var output = GetMods(currentSpirit).Copy();

                    if (output.index > 1512)
                    {
                        output.Custom = true;
                    }

                    serializer.Serialize(writer, output);
                }

                MessageBox.Show($"Exported {currentSpirit.name_id} to /Export.", "Done!");
            }
            else
            {
                foreach(var mod in modList)
                {
                    // Save a JSON representation of the spirit mods
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;

                    // Get the directory
                    string path = AppDomain.CurrentDomain.BaseDirectory + "Export";

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    // Export to JSON
                    using (StreamWriter sw = new StreamWriter(path + $@"/{mod.Value.name_id}.tgns"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        var output = GetMods(mod.Value).Copy();

                        if (output.index > 1512)
                        {
                            output.Custom = true;
                        }

                        serializer.Serialize(writer, output);
                    }
                }

                MessageBox.Show($"Exported all spirits to /Export.", "Done!");
            }
        }


        private void Spirit_Import_Click(object sender, RoutedEventArgs e)
        {
            var import = MessageBox.Show("Would you like to import all spirits\nfrom " +
                "the Import folder?",
                "Hold Up", MessageBoxButton.YesNo);

            string path = AppDomain.CurrentDomain.BaseDirectory + "Import";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (import.Equals(MessageBoxResult.No))
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = path;
                dialog.IsFolderPicker = false;
                bool selectedItem = false;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    selectedItem = true;
                }

                if (!selectedItem)
                {
                    return;
                }

                if (dialog.FileName.ToLower().EndsWith(".tgns"))
                {
                    if (File.Exists(dialog.FileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Ignore;

                        using (StreamReader sr = new StreamReader(dialog.FileName))
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var mod = serializer.Deserialize<Spirit>(reader);

                            ImportSpirit(mod, sender, e);

                            //Spirit_Commit_Click(sender, e);

                            //DisplaySpirit(mod);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selected file is not a .tngs file.", "Hold Up");
                }
            }
            else
            {
                var files = Directory.GetFiles(path);

                foreach(var file in files)
                {
                    if (!file.ToLower().EndsWith(".tgns"))
                        continue;

                    if (File.Exists(file))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Ignore;

                        using (StreamReader sr = new StreamReader(file))
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var mod = serializer.Deserialize<Spirit>(reader);

                            ImportSpirit(mod, sender, e);
                        }
                    }
                }

                Spirit_Commit_Click(sender, e);
            }
        }


        private void ImportSpirit(Spirit mod, object sender, RoutedEventArgs e)
        {
            if (mod.Custom)
            {
                // Check if a mod of the existing ID exists first.
                if (GetMods(mod.spirit_id).spirit_id == mod.spirit_id)
                {
                    var result = MessageBox.Show("A custom spirit already exists with the same ID.\n" +
                        "This will overwrite all edits done to " +
                        $"{GetMods(mod.spirit_id).display_name}\n" +
                        $"Is this okay?", "Hold Up", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        unsavedEdits[mod.index] = mod;
                        MessageBox.Show($"Mod imported at index {mod.index}.", "It's Done");

                        DisplaySpirit(mod);
                    }
                }
                else
                {
                    var index = GetNextValidSaveIndex();
                    mod.index = index;
                    mod.save_no = Convert.ToUInt16(index);
                    mod.directory_id = Convert.ToUInt16(index);

                    modList.Add(mod.index, mod);
                    SpiritValueDisplay.Spirits.Add(mod.spirit_id,
                        new SpiritValueDisplay.SpiritID(mod.spirit_id, mod.display_name));

                    // Refresh UI
                    var brush = new LinearGradientBrush();
                    brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 0.0));
                    brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.25));
                    brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.75));
                    brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 1.0));

                    SpiritButton newBtn = new SpiritButton();

                    newBtn.Content = modList.ContainsKey(mod.index) ?
                        $"{modList[mod.index].display_name}" :
                        mod.display_name;
                    newBtn.FontWeight = FontWeights.Bold;
                    newBtn.Name = "Button_" + mod.name_id;
                    newBtn.Background = brush;
                    newBtn.spirit = mod;
                    newBtn.Click += (s, ev) => {
                        DisplaySpirit(newBtn.spirit);
                    };

                    spiritButtons.Add(mod.index, newBtn);
                    SpiritList.Children.Add(newBtn);

                    if (!saveToSpiritID.ContainsKey(mod.save_no))
                    {
                        saveToSpiritID.Add(mod.save_no, mod.spirit_id);
                    }

                    MessageBox.Show($"Mod imported at index {mod.index}.", "It's Done");

                    Spirit_Commit_Click(sender, e);

                    DisplaySpirit(mod);
                }
            }
            else
            {
                var result = MessageBox.Show("This will overwrite all edits done to " +
                    $"{GetMods(mod.spirit_id).display_name}\n" +
                    $"Is this okay?\n" +
                    $"If you choose no, the spirit will be imported as a custom spirit.",
                    "Hold Up", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    unsavedEdits[mod.index] = mod;
                    MessageBox.Show($"Mod imported at index {mod.index}.", "It's Done");

                    DisplaySpirit(mod);
                }
                else if (result == MessageBoxResult.No)
                {
                    mod.Custom = true;

                    var index = GetNextValidSaveIndex();
                    mod.index = index;
                    mod.save_no = Convert.ToUInt16(index);
                    mod.directory_id = Convert.ToUInt16(index);

                    string ID = $"spirit{index}";
                    mod.name_id = ID;
                    mod.spirit_id = Hash40Util.StringToHash40(mod.name_id);

                    mod.battle.index = -1;
                    mod.battle.battle_id = mod.spirit_id;
                    mod.aesthetics.index = -1;

                    for(int i = 0; i<mod.battle.fighterDBIndexes.Count; i++)
                    {
                        mod.battle.fighterDBIndexes[i] = -1;
                    }

                    foreach(var fighter in mod.battle.fighters)
                    {
                        fighter.battle_id = mod.battle.battle_id;
                    }

                    MessageBox.Show($"The Internal ID of the imported spirit has been changed to {ID}." +
                        $"\nThis is necessary to avoid duplicate IDs.", "Just letting you know");

                    modList.Add(mod.index, mod);
                    SpiritValueDisplay.Spirits.Add(mod.spirit_id,
                        new SpiritValueDisplay.SpiritID(mod.spirit_id, mod.display_name));

                    // Refresh UI
                    var brush = new LinearGradientBrush();
                    brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 0.0));
                    brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.25));
                    brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.75));
                    brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 1.0));

                    SpiritButton newBtn = new SpiritButton();

                    newBtn.Content = modList.ContainsKey(mod.index) ?
                        $"{modList[mod.index].display_name}" :
                        mod.display_name;
                    newBtn.FontWeight = FontWeights.Bold;
                    newBtn.Name = "Button_" + mod.name_id;
                    newBtn.Background = brush;
                    newBtn.spirit = mod;
                    newBtn.Click += (s, ev) => {
                        DisplaySpirit(newBtn.spirit);
                    };

                    spiritButtons.Add(mod.index, newBtn);
                    SpiritList.Children.Add(newBtn);

                    if (!saveToSpiritID.ContainsKey(mod.save_no))
                    {
                        saveToSpiritID.Add(mod.save_no, mod.spirit_id);
                    }

                    MessageBox.Show($"Mod imported at index {mod.index}.", "It's Done");
                }
            }
        }


        private void Image_Import_Click(object sender, RoutedEventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = path;
            dialog.IsFolderPicker = false;
            bool selectedItem = false;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedItem = true;
            }

            if (!selectedItem)
            {
                return;
            }

            // NOTE: JPEG will be unsupported, I just need it to test with Hux Losing The Star Wars
            // HE LOST THE STAR WARS
            if (dialog.FileName.ToLower().EndsWith(".webp") || dialog.FileName.ToLower().EndsWith(".png"))
            {
                if (File.Exists(dialog.FileName))
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\corrected.png"))
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\corrected.png");
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\spirits_2.png"))
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\spirits_2.png");
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\webp_convert.png"))
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\webp_convert.png");
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\resized.png"))
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\resized.png");

                    LoadImage(dialog.FileName);
                }
            }
            else
            {
                CurrentPreviewSource = "";
                MessageBox.Show("Selected file is not a valid image file or is an unsupported format.", "Hold Up");
            }
        }


        private void LoadImage(string Path)
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Temp");

            // Get the scale factor for the image. 
            // This is never going to change so we should probably define at boot time.
            float ScaleFactorFull = (float)(Spirit_Preview_BG.Height / 720) * 3.1f;
            float ScaleFactorHead = (float)(Spirit_Preview_Head_BG.Height / 300) * 3.1f;

            // Load the image into Spirit Board.
            // Due to WPF taking resolution into account, we cannot use the image directly.
            // Instead, we make a copy and use that. Cache the image for later use.
            CurrentPreviewSource = Path;

            if (Path.ToLower().EndsWith("webp"))
            {
                var collection = new MagickImage(Path);
                collection.Write(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\webp_convert.png");
                collection.Dispose();

                var TempImage = new BitmapImage();
                TempImage.BeginInit();
                TempImage.CacheOption = BitmapCacheOption.OnLoad;
                TempImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\webp_convert.png");
                TempImage.EndInit();
                int stride = TempImage.PixelWidth * (TempImage.Format.BitsPerPixel / 8);
                byte[] data = new byte[stride * TempImage.PixelHeight];
                TempImage.CopyPixels(data, stride, 0);

                CurrentPreview = BitmapSource.Create(TempImage.PixelWidth, TempImage.PixelHeight, 300, 300,
                    TempImage.Format, TempImage.Palette, data, stride);
            }
            else
            {
                var TempImage = new BitmapImage();
                TempImage.BeginInit();
                TempImage.CacheOption = BitmapCacheOption.OnLoad;
                TempImage.UriSource = new Uri(Path);
                TempImage.EndInit();
                int stride = TempImage.PixelWidth * (TempImage.Format.BitsPerPixel / 8);
                byte[] data = new byte[stride * TempImage.PixelHeight];
                TempImage.CopyPixels(data, stride, 0);

                CurrentPreview = BitmapSource.Create(TempImage.PixelWidth, TempImage.PixelHeight, 300, 300,
                    TempImage.Format, TempImage.Palette, data, stride);
            }

            if(CurrentPreview.PixelWidth > 1280 || CurrentPreview.PixelHeight > 1280)
            {
                var result = MessageBox.Show("Your image is larger than Smash's screen resolution.\n" +
                    $"The image is {CurrentPreview.PixelWidth}x{CurrentPreview.PixelHeight}\n" +
                    $"This can be imported, but will result in extremely large file sizes.\n" +
                    $"Would you like to resize the image to fit?", "Hold Up", MessageBoxButton.YesNo);

                if(result == MessageBoxResult.Yes)
                {
                    float resize = CurrentPreview.PixelHeight > CurrentPreview.PixelWidth ?
                         1.0f / (CurrentPreview.PixelHeight / 1280.0f) : 
                         1.0f / (CurrentPreview.PixelWidth / 1280.0f);

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(CurrentPreview));

                    var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory +
                        @"Temp\resized.png", FileMode.Create);

                    encoder.Save(fileStream);
                    fileStream.Dispose();
                    fileStream.Close();

                    var correction = new MagickImage(AppDomain.CurrentDomain.BaseDirectory +
                        @"Temp\resized.png");
                    correction.Scale((int)(correction.Width * resize), (int)(correction.Height * resize));
                    correction.Write(AppDomain.CurrentDomain.BaseDirectory +
                        @"Temp\resized.png");
                    correction.Dispose();

                    var TempImage = new BitmapImage();
                    TempImage.BeginInit();
                    TempImage.CacheOption = BitmapCacheOption.OnLoad;
                    TempImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory +
                        @"Temp\resized.png");
                    TempImage.EndInit();
                    int stride = TempImage.PixelWidth * (TempImage.Format.BitsPerPixel / 8);
                    byte[] data = new byte[stride * TempImage.PixelHeight];
                    TempImage.CopyPixels(data, stride, 0);

                    CurrentPreview = BitmapSource.Create(TempImage.PixelWidth, TempImage.PixelHeight, 300, 300,
                        TempImage.Format, TempImage.Palette, data, stride);
                }

            }

            if (!CheckPixelBorder(CurrentPreview))
            {
                // Oh my god...
                // So we have to write the current preview, use ImageMagick to expand it, then load it again.
                // The things I do for you...
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(CurrentPreview));

                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\corrected.png", FileMode.Create);

                encoder.Save(fileStream);
                fileStream.Dispose();
                fileStream.Close();

                var correction = new MagickImage(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\corrected.png");
                correction.Extent(CurrentPreview.PixelWidth + 2, CurrentPreview.PixelHeight + 2,
                    Gravity.Center, new MagickColor(0, 0, 0, 0));
                correction.Write(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\corrected.png");
                correction.Dispose();

                var TempImage = new BitmapImage();
                TempImage.BeginInit();
                TempImage.CacheOption = BitmapCacheOption.OnLoad;
                TempImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\corrected.png");
                TempImage.EndInit();
                int stride = TempImage.PixelWidth * (TempImage.Format.BitsPerPixel / 8);
                byte[] data = new byte[stride * TempImage.PixelHeight];
                TempImage.CopyPixels(data, stride, 0);

                CurrentPreview = BitmapSource.Create(TempImage.PixelWidth, TempImage.PixelHeight, 300, 300,
                    TempImage.Format, TempImage.Palette, data, stride);
            }

            // Apply the scale transformation to the full image.
            // Note that the full preview is reduced by about 83.3% in game.
            var PreviewFull = new TransformedBitmap();
            PreviewFull.BeginInit();
            PreviewFull.Source = CurrentPreview;
            PreviewFull.Transform = new ScaleTransform(ScaleFactorFull * 0.833, ScaleFactorFull * 0.833);
            PreviewFull.EndInit();

            // Add the full image to the full canvas and center it.
            // The image must be defined dynamically.
            if (CurrentFullPreviewImage != null)
            {
                Spirit_Preview_Canvas.Children.Remove(CurrentFullPreviewImage);
            }

            CurrentFullPreviewImage = new Image { Width = PreviewFull.Width, Height = PreviewFull.Height, Source = PreviewFull };
            CurrentImageScale = 1.0f;
            var FullCenter = GetPreviewImageCenter(CurrentFullPreviewImage,
                CurrentImageScale, Spirit_Preview_Canvas);

            Canvas.SetLeft(CurrentFullPreviewImage, FullCenter.X);
            Canvas.SetTop(CurrentFullPreviewImage, FullCenter.Y);
            Spirit_Preview_Canvas.Children.Add(CurrentFullPreviewImage);

            // Apply the scale transformation to the head image.
            var PreviewHead = new TransformedBitmap();
            PreviewHead.BeginInit();
            PreviewHead.Source = CurrentPreview;
            PreviewHead.Transform = new ScaleTransform(ScaleFactorHead, ScaleFactorHead);
            PreviewHead.EndInit();

            // Add the head image to the head canvas and center it.
            // The image must be defined dynamically.
            if (CurrentHeadPreviewImage != null)
            {
                Spirit_Preview_Head_Canvas.Children.Remove(CurrentHeadPreviewImage);
            }

            CurrentHeadPreviewImage = new Image { Width = PreviewHead.Width, Height = PreviewHead.Height, Source = PreviewHead };
            CurrentImageHeadScale = 1.0f;
            var HeadCenter = GetPreviewImageCenter(CurrentHeadPreviewImage,
                CurrentImageHeadScale, Spirit_Preview_Head_Canvas);

            Canvas.SetLeft(CurrentHeadPreviewImage, HeadCenter.X);
            Canvas.SetTop(CurrentHeadPreviewImage, HeadCenter.Y);
            Spirit_Preview_Head_Canvas.Children.Add(CurrentHeadPreviewImage);

            ApplyCurrentImageOffsets();


            // Preview Spirits 0
            var Preview0 = new TransformedBitmap();
            Preview0.BeginInit();
            Preview0.Source = CurrentPreview;

            if (CurrentPreview.PixelHeight > 120 || CurrentPreview.PixelWidth > 120)
            {
                if (CurrentPreview.PixelHeight > CurrentPreview.PixelWidth)
                {
                    Preview0.Transform = new ScaleTransform(
                         120.0 / CurrentPreview.Height, 120.0 / CurrentPreview.Height);
                }
                else
                {
                    Preview0.Transform = new ScaleTransform(
                         120.0 / CurrentPreview.Width, 120.0 / CurrentPreview.Width);
                }
            }

            Preview0.EndInit();

            Spirit_Preview_0.Source = Preview0;
            if (!Spirit_Preview_Spirits_0_Canvas.Children.Contains(Spirit_Preview_0))
                Spirit_Preview_Spirits_0_Canvas.Children.Add(Spirit_Preview_0);


            // Preview Spirits 1
            if (Spirit_Preview_Spirits_1_Canvas.Children.Count > 0)
            {
                Spirit_Preview_Spirits_1_Canvas.Children.RemoveRange
                    (0, Spirit_Preview_Spirits_1_Canvas.Children.Count);
            }

            CurrentSpirits1PreviewImage = new Image
                { Width = PreviewHead.Width, Height = PreviewHead.Height, Source = PreviewHead };
            var FullCenter_1 = GetPreviewImageCenter(CurrentSpirits1PreviewImage,
                1, Spirit_Preview_Spirits_1_Canvas);

            Canvas.SetLeft(CurrentSpirits1PreviewImage, FullCenter_1.X);
            Canvas.SetTop(CurrentSpirits1PreviewImage, FullCenter_1.Y);
            Spirit_Preview_Spirits_1_Canvas.Children.Add(CurrentSpirits1PreviewImage);


            // Preview Spirits 2
            BitmapEncoder encoder_sp2 = new PngBitmapEncoder();
            encoder_sp2.Frames.Add(BitmapFrame.Create(CurrentPreview));

            var fileStream_sp2 = new FileStream(AppDomain.CurrentDomain.BaseDirectory +
                @"Temp\spirits_2.png", FileMode.Create);

            encoder_sp2.Save(fileStream_sp2);
            fileStream_sp2.Dispose();
            fileStream_sp2.Close();

            var collection_sp2 = new MagickImage(AppDomain.CurrentDomain.BaseDirectory +
                @"Temp\spirits_2.png");
            collection_sp2.Resize(CurrentPreview.PixelWidth / 2, CurrentPreview.PixelHeight / 2);

            var Pixels = collection_sp2.GetPixels();

            foreach (var pixel in Pixels)
            {
                if (pixel.GetChannel(3) != 0)
                {
                    pixel.SetChannel(0, 255);
                    pixel.SetChannel(1, 255);
                    pixel.SetChannel(2, 255);
                }
            }

            collection_sp2.GaussianBlur(2);
            collection_sp2.Extent((CurrentPreview.PixelWidth / 2) + 40, (CurrentPreview.PixelHeight / 2) + 40,
                Gravity.Center, new MagickColor(0, 0, 0, 0));
            collection_sp2.Write(AppDomain.CurrentDomain.BaseDirectory +
                @"Temp\spirits_2.png");
            collection_sp2.Dispose();

            var TempImage_sp2 = new BitmapImage();
            TempImage_sp2.BeginInit();
            TempImage_sp2.CacheOption = BitmapCacheOption.OnLoad;
            TempImage_sp2.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory +
                @"Temp\spirits_2.png");
            TempImage_sp2.EndInit();
            int stride_sp2 = TempImage_sp2.PixelWidth * (TempImage_sp2.Format.BitsPerPixel / 8);
            byte[] data_sp2 = new byte[stride_sp2 * TempImage_sp2.PixelHeight];
            TempImage_sp2.CopyPixels(data_sp2, stride_sp2, 0);

            spirits_2 = BitmapSource.Create(TempImage_sp2.PixelWidth, TempImage_sp2.PixelHeight, 300, 300,
                TempImage_sp2.Format, TempImage_sp2.Palette, data_sp2, stride_sp2);

            var Preview2 = new TransformedBitmap();
            Preview2.BeginInit();
            Preview2.Source = spirits_2;

            if (spirits_2.PixelHeight > 120 || spirits_2.PixelWidth > 120)
            {
                if (spirits_2.PixelHeight > spirits_2.PixelWidth)
                {
                    Preview2.Transform = new ScaleTransform(
                         120.0 / spirits_2.Height, 120.0 / spirits_2.Height);
                }
                else
                {
                    Preview2.Transform = new ScaleTransform(
                         120.0 / spirits_2.Width, 120.0 / spirits_2.Width);
                }
            }

            Preview2.EndInit();

            Spirit_Preview_2.Source = Preview2;
            if (!Spirit_Preview_Spirits_2_Canvas.Children.Contains(Spirit_Preview_2))
                Spirit_Preview_Spirits_2_Canvas.Children.Add(Spirit_Preview_2);

            Chara_1_Scale.Value = 1;
        }


        private bool CheckPixelBorder(BitmapSource InImage)
        {
            for(int y = 0; y < InImage.PixelHeight; y++)
            {
                if(y == 0 || y == InImage.PixelHeight - 1)
                {
                    for (int x = 0; x < InImage.PixelWidth; x++)
                    {
                        var Pixel = GetPixelColor(InImage, x, y);

                        if(Pixel.A != 0)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var Pixel = GetPixelColor(InImage, 0, y);

                    if (Pixel.A != 0)
                    {
                        return false;
                    }
                    else
                    {
                        Pixel = GetPixelColor(InImage, InImage.PixelWidth - 1, y);

                        if (Pixel.A != 0)
                        {
                            return false;
                        }
                    }

                }
            }

            return true;
        }


        private static Color GetPixelColor(BitmapSource bitmap, int x, int y)
        {
            Color color;
            var bytes = GetPixelBytes(bitmap, x, y);

            if (bitmap.Format == PixelFormats.Bgra32)
            {
                color = Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
            else if (bitmap.Format == PixelFormats.Bgr32)
            {
                color = Color.FromRgb(bytes[2], bytes[1], bytes[0]);
            }
            else
            {
                color = Colors.Black;
            }

            return color;
        }

        private static byte[] GetPixelBytes(BitmapSource bitmap, int x, int y)
        {
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(x, y, 1, 1);

            bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);
            return bytes;
        }

        int ExportType = 0;

        private void Spirits_0_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPreview != null)
            {
                ExportType = 0;
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Temp");

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(CurrentPreview));

                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + 
                    @"Temp\temp.png", FileMode.Create);

                encoder.Save(fileStream);
                fileStream.Dispose();
                fileStream.Close();

                bool bDLC = currentSpirit.index >= 1300 && currentSpirit.index < 1514;

                string path = bDLC ?
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace_patch\spirits\spirits_0\" :
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace\spirits\spirits_0\";
                Directory.CreateDirectory(path);

                string cmdLine = bDLC ? @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace_patch\spirits\spirits_0\" + 
                    "spirits_0_" + currentSpirit.name_id + ".bntx" :
                    @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace\spirits\spirits_0\" +
                    "spirits_0_" + currentSpirit.name_id + ".bntx";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                process.ErrorDataReceived += cmd_Error;
                process.Exited += cmd_Exit_ClearTemp;
                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine("cd " + AppDomain.CurrentDomain.BaseDirectory);
                process.StandardInput.WriteLine(cmdLine);
                process.StandardInput.WriteLine("Exit");
                process.WaitForExit();
            }
            else
            {
                MessageBox.Show("No image has been loaded.", "Hold Up");
            }
        }


        private void Spirits_1_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPreview != null)
            {
                ExportType = 1;
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Temp");

                RenderTargetBitmap rtb = new RenderTargetBitmap(1080, 1080, 127, 127, PixelFormats.Default);
                rtb.Render(CurrentSpirits1PreviewImage);

                var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, 160, 160));

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(crop));

                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\temp.png", FileMode.Create);

                encoder.Save(fileStream);
                fileStream.Close();

                bool bDLC = currentSpirit.index >= 1300 && currentSpirit.index < 1514;

                string path = bDLC ?
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace_patch\spirits\spirits_1\" :
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace\spirits\spirits_1\";
                Directory.CreateDirectory(path);

                string cmdLine = bDLC ? @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace_patch\spirits\spirits_1\" +
                    "spirits_1_" + currentSpirit.name_id + ".bntx" :
                    @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace\spirits\spirits_1\" +
                    "spirits_1_" + currentSpirit.name_id + ".bntx";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                process.ErrorDataReceived += cmd_Error;
                process.Exited += cmd_Exit_ClearTemp;
                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine("cd " + AppDomain.CurrentDomain.BaseDirectory);
                process.StandardInput.WriteLine(cmdLine);
                process.StandardInput.WriteLine("Exit");
                process.WaitForExit();
            }
            else
            {
                MessageBox.Show("No image has been loaded.", "Hold Up");
            }
        }


        private void Spirits_2_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPreview != null)
            {
                ExportType = 2;
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Temp");

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(spirits_2));

                var fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory +
                    @"Temp\temp.png", FileMode.Create);

                encoder.Save(fileStream);
                fileStream.Dispose();
                fileStream.Close();

                bool bDLC = currentSpirit.index >= 1300 && currentSpirit.index < 1514;

                string path = bDLC ?
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace_patch\spirits\spirits_2\" :
                    AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\ui\replace\spirits\spirits_2\";
                Directory.CreateDirectory(path);

                string cmdLine = bDLC ? @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace_patch\spirits\spirits_2\" +
                    "spirits_2_" + currentSpirit.name_id + ".bntx" :
                    @"img2bntx.exe Temp\temp.png ArcOutput\ui\replace\spirits\spirits_2\" +
                    "spirits_2_" + currentSpirit.name_id + ".bntx";

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;
                process.ErrorDataReceived += cmd_Error;
                process.Exited += cmd_Exit_ClearTemp;
                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine("cd " + AppDomain.CurrentDomain.BaseDirectory);
                process.StandardInput.WriteLine(cmdLine);
                process.StandardInput.WriteLine("Exit");
                process.WaitForExit();
            }
            else
            {
                MessageBox.Show("No image has been loaded.", "Hold Up");
            }
        }

        void cmd_Exit_ClearTemp(object sender, System.EventArgs e)
        {
            MessageBox.Show($"Successfully saved spirits_{ExportType} for " + currentSpirit.display_name + ".");

            if(ExportType == 0)
            {
                if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + currentSpirit.name_id + ".png"))
                {
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + currentSpirit.name_id + ".png");
                }

                File.Move(AppDomain.CurrentDomain.BaseDirectory + @"Temp\temp.png",
                    AppDomain.CurrentDomain.BaseDirectory + @"Images\spirits_0_" + currentSpirit.name_id + ".png");
            }
            else
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\temp.png");
            }
        }

        static void cmd_Error(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if(!string.IsNullOrEmpty(e.Data))
                MessageBox.Show("An error has occurred: " + e.Data);
        }


        private Vector GetPreviewImageCenter(Image Source, float Scale, Canvas RootCanvas)
        {
            return new Vector(((-Source.Width * Scale) / 2.0) + (RootCanvas.Width / 2.0),
                ((-Source.Height * Scale) / 2.0) + (RootCanvas.Height / 2.0));
        }


        private void ApplyCurrentImageOffsets()
        {
            var Offsets = GetMods(currentSpirit).aesthetics;

            // NOTE: Y values in Smash follow normal X/Y rules.
            // Because WFP aligns the mage relative to the top left, Y values must be inverted.

            // Set the full image
            CurrentImageScale = Offsets.art_size[1];
            CurrentFullPreviewImage.LayoutTransform = new ScaleTransform(CurrentImageScale, CurrentImageScale);
            var FullCenter = GetPreviewImageCenter(CurrentFullPreviewImage,
                CurrentImageScale, Spirit_Preview_Canvas);
            Canvas.SetLeft(CurrentFullPreviewImage, FullCenter.X + 
                ((Offsets.art_pos_x[1] * Spirit_Preview_Canvas.Height) / 720.0));
            Canvas.SetTop(CurrentFullPreviewImage, FullCenter.Y -
                ((Offsets.art_pos_y[1] * Spirit_Preview_Canvas.Height) / 720.0));

            // Set the star offsets
            for(int i=0; i<Stars.Count; i++)
            {
                var Star = Stars[i];
                var x = ((Offsets.effect_pos_x[i] / 720.0f) * Spirit_Preview_Canvas.Height) * Offsets.art_size[1] * 0.833;
                var y = ((Offsets.effect_pos_y[i] / 720.0f) * Spirit_Preview_Canvas.Height) * Offsets.art_size[1] * 0.833;
                bool bVisible = i < Offsets.effect_count;

                if (!bVisible)
                {
                    Canvas.SetLeft(Star, -100);
                    Canvas.SetRight(Star, 100);
                }
                else
                {
                    Canvas.SetLeft(Star, FullCenter.X +
                        ((Offsets.art_pos_x[1] * Spirit_Preview_Canvas.Height) / 720.0) + x - (Star.Width / 2));
                    Canvas.SetTop(Star, FullCenter.Y -
                        ((Offsets.art_pos_y[1] * Spirit_Preview_Canvas.Height) / 720.0) - y +
                        (CurrentFullPreviewImage.Height * Offsets.art_size[1]) - (Star.Height / 2));
                }
            }

            // Set the head image
            CurrentImageHeadScale = Offsets.art_size[0];
            CurrentHeadPreviewImage.LayoutTransform = new ScaleTransform(CurrentImageHeadScale, CurrentImageHeadScale);
            var HeadCenter = GetPreviewImageCenter(CurrentHeadPreviewImage,
                CurrentImageHeadScale, Spirit_Preview_Head_Canvas);
            Canvas.SetLeft(CurrentHeadPreviewImage, HeadCenter.X +
                ((Offsets.art_pos_x[0] * Spirit_Preview_Head_Canvas.Height) / 300.0));
            Canvas.SetTop(CurrentHeadPreviewImage, HeadCenter.Y -
                ((Offsets.art_pos_y[0] * Spirit_Preview_Head_Canvas.Height) / 300.0));
        }

        private Image DraggedImage;
        private Point MousePosition;
        private Vector InitialPosition;

        private void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = null;

            if(sender == Spirit_Preview_Canvas)
            {
                canvas = Spirit_Preview_Canvas;
            }
            else if(sender == Spirit_Preview_Head_Canvas)
            {
                canvas = Spirit_Preview_Head_Canvas;
            }
            else if(sender == Spirit_Preview_Spirits_1_Canvas)
            {
                canvas = Spirit_Preview_Spirits_1_Canvas;
            }
            
            var image = e.Source as Image;

            if (image != null && canvas != null && canvas.CaptureMouse())
            {
                MousePosition = e.GetPosition(canvas);
                DraggedImage = image;
                InitialPosition = new Vector(Canvas.GetLeft(DraggedImage), Canvas.GetTop(DraggedImage));
                //Panel.SetZIndex(DraggedImage, 1);
            }
        }

        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = null;
            int TargetIndex = 1;
            bool bHead = false;
            bool bSp1 = false;
            bool bUpdateSecond = false;

            if (sender == Spirit_Preview_Canvas)
            {
                canvas = Spirit_Preview_Canvas;
                bUpdateSecond = true;
            }
            else if (sender == Spirit_Preview_Head_Canvas)
            {
                canvas = Spirit_Preview_Head_Canvas;
                bHead = true;
                TargetIndex = 0;
            }
            else if(sender == Spirit_Preview_Spirits_1_Canvas)
            {
                canvas = Spirit_Preview_Spirits_1_Canvas;
                bSp1 = true;
            }

            if (DraggedImage != null && canvas != null)
            {
                if (Stars.Contains(DraggedImage))
                {
                    // TODO: move the stars
                    int StarIndex = Stars.IndexOf(DraggedImage);
                    var Aesthetics = GetMods(currentSpirit).aesthetics;

                    // Calculate diff and convert to pixels
                    Vector NewPosition = new Vector(Canvas.GetLeft(DraggedImage), Canvas.GetTop(DraggedImage));
                    Vector Diff = ((NewPosition - InitialPosition) * 720) / 120 / Aesthetics.art_size[1] / 0.833;

                    Aesthetics.effect_pos_x[StarIndex] += ((int)Diff.X);
                    Aesthetics.effect_pos_y[StarIndex] -= ((int)Diff.Y);

                    if(Effect_Index.SelectedIndex == StarIndex)
                    {
                        handleChanges = false;
                        Effect_X.Value += ((int)Diff.X);
                        Effect_Y.Value -= ((int)Diff.Y);
                        handleChanges = true;
                    }

                    UpdateSpirit(sender, e);
                }
                else
                {
                    if (!bSp1)
                    {
                        // Calculate diff and convert to pixels
                        Vector NewPosition = new Vector(Canvas.GetLeft(DraggedImage), Canvas.GetTop(DraggedImage));
                        Vector Diff = NewPosition - InitialPosition;

                        if (bHead)
                        {
                            Diff = (Diff * 300) / 120;
                        }
                        else
                        {
                            Diff = (Diff * 720) / 120;
                        }

                        var Aesthetics = GetMods(currentSpirit).aesthetics;

                        Aesthetics.art_pos_x[TargetIndex] += ((float)Diff.X);
                        Aesthetics.art_pos_y[TargetIndex] -= ((float)Diff.Y);

                        if (bUpdateSecond)
                        {
                            Aesthetics.art_pos_x[2] = Aesthetics.art_pos_x[1];
                            Aesthetics.art_pos_y[2] = Aesthetics.art_pos_y[1] - 50;
                        }

                        if (Image_Pos_Type.SelectedIndex == TargetIndex)
                        {
                            handleChanges = false;
                            Image_X.Value = Convert.ToDecimal(Aesthetics.art_pos_x[TargetIndex]);
                            Image_Y.Value = Convert.ToDecimal(Aesthetics.art_pos_y[TargetIndex]);
                            handleChanges = true;
                        }

                        UpdateSpirit(sender, e);
                    }
                }

                canvas.ReleaseMouseCapture();
                //Panel.SetZIndex(DraggedImage, 0);
                DraggedImage = null;
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (DraggedImage == null)
            {
                return;
            }

            Canvas canvas = null;

            if (sender == Spirit_Preview_Canvas)
            {
                canvas = Spirit_Preview_Canvas;
            }
            else if (sender == Spirit_Preview_Head_Canvas)
            {
                canvas = Spirit_Preview_Head_Canvas;
            }
            else if(sender == Spirit_Preview_Spirits_1_Canvas)
            {
                canvas = Spirit_Preview_Spirits_1_Canvas;
            }

            if (DraggedImage != null && canvas != null)
            {
                var position = e.GetPosition(canvas);
                var offset = position - MousePosition;
                MousePosition = position;

                Canvas.SetLeft(DraggedImage, Canvas.GetLeft(DraggedImage) + offset.X);
                Canvas.SetTop(DraggedImage, Canvas.GetTop(DraggedImage) + offset.Y);

                if(DraggedImage == CurrentFullPreviewImage)
                {
                    foreach(var Star in Stars)
                    {
                        Canvas.SetLeft(Star, Canvas.GetLeft(Star) + offset.X);
                        Canvas.SetTop(Star, Canvas.GetTop(Star) + offset.Y);
                    }
                }
            }
        }


        private void WoLSpirit_Import_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This will reset the World of Light spirit list.", "Note");

            string path = AppDomain.CurrentDomain.BaseDirectory;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = path;
            dialog.IsFolderPicker = false;
            bool selectedItem = false;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedItem = true;
            }

            if (!selectedItem)
            {
                return;
            }

            if (dialog.FileName.ToLower().EndsWith(".txt"))
            {
                if (File.Exists(dialog.FileName))
                {
                    WoLSpirits.Clear();

                    var linesRead = File.ReadLines(dialog.FileName);

                    foreach (var line in linesRead)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            // Get spirit by name
                            var spirit = GetMods(line);

                            if(spirit.save_no != 0 && !WoLSpirits.Contains(spirit.save_no))
                            {
                                if(spirit.evolve_src != 8403017505)
                                {
                                    MessageBox.Show($"{spirit.display_name} is an evolved spirit.\n" +
                                        $"This will work, but may not be intended.", "Just letting you know");
                                }

                                WoLSpirits.Add(spirit.save_no);
                                
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selected file is not a .txt file.", "Hold Up");
            }
        }

        private void Can_Save_Spirits(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = !building; }

        private void Execute_Save_Spirits(object sender, ExecutedRoutedEventArgs e) { Spirit_Commit_Click(sender, e); }

        bool bShowLogs = true;
        bool bDebugBoard = false;

        private void Spirit_Save_Debug_Param_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This will build all mods to ArcOutput, " +
                "\nbut will add all custom spirits to the debug board.\n" +
                "Is this okay?",
                "Hold Up", MessageBoxButton.YesNo);

            if (!result.Equals(MessageBoxResult.Yes))
            {
                return;
            }

            bDebugBoard = true;
            Spirit_Save_Param_Click(sender, e);
            bDebugBoard = false;
        }

        private void Set_Debug_Param_Click(object sender, RoutedEventArgs e)
        {
            var image = e.Source as Image;

            if (image != null)
            {
                string name = image.Name;

                switch (name)
                {
                    case "FP1_Icon":
                        saveData.DebugSpiritBoardID = 0;
                        MessageBox.Show("Set Debug Spirit Board to Persona.");
                        break;
                    case "FP2_Icon":
                        saveData.DebugSpiritBoardID = 1;
                        MessageBox.Show("Set Debug Spirit Board to DRAGON QUEST.");
                        break;
                    case "FP3_Icon":
                        saveData.DebugSpiritBoardID = 2;
                        MessageBox.Show("Set Debug Spirit Board to Banjo-Kazooie.");
                        break;
                    case "FP4_Icon":
                        saveData.DebugSpiritBoardID = 3;
                        MessageBox.Show("Set Debug Spirit Board to FATAL FURY.");
                        break;
                    case "FP5_Icon":
                        saveData.DebugSpiritBoardID = 4;
                        MessageBox.Show("Set Debug Spirit Board to Fire Emblem.");
                        break;
                    case "FP6_Icon":
                        saveData.DebugSpiritBoardID = 5;
                        MessageBox.Show("Set Debug Spirit Board to ARMS.");
                        break;
                    case "FP7_Icon":
                        saveData.DebugSpiritBoardID = 6;
                        MessageBox.Show("Set Debug Spirit Board to Minecraft.");
                        break;
                    case "FP8_Icon":
                        saveData.DebugSpiritBoardID = 7;
                        MessageBox.Show("Set Debug Spirit Board to FINAL FANTASY.");
                        break;
                    case "FP9_Icon":
                        saveData.DebugSpiritBoardID = 8;
                        MessageBox.Show("Set Debug Spirit Board to Xenoblade Chronicles.");
                        break;
                    case "FP10_Icon":
                        saveData.DebugSpiritBoardID = 9;
                        MessageBox.Show("Set Debug Spirit Board to Tekken.");
                        break;
                    case "FP11_Icon":
                        saveData.DebugSpiritBoardID = 10;
                        MessageBox.Show("Set Debug Spirit Board to KINGDOM HEARTS.");
                        break;
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            // Export to JSON
            using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"Data/app_data.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, saveData);
            }
        }

        private void Spirit_Save_Param_Click(object sender, RoutedEventArgs e)
        {
            building = true;

            string basePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\";
            
            // Load spirit params
            ParamFile ui_spirit_db = new ParamFile();
            ui_spirit_db.Open(basePath + "ui_spirit_db.prc");

            ParamFile ui_spirits_battle_db = new ParamFile();
            ui_spirits_battle_db.Open(basePath + "ui_spirits_battle_db.prc");

            ParamFile ui_spirit_layout_db = new ParamFile();
            ui_spirit_layout_db.Open(basePath + "ui_spirit_layout_db.prc");

            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
            if (!Directory.Exists(outputPath + @"\ui\param\database\")) 
                Directory.CreateDirectory(outputPath + @"\ui\param\database\");
            if (!Directory.Exists(outputPath + @"\ui\message\"))
                Directory.CreateDirectory(outputPath + @"\ui\message\");

            // Load spirit MSBT
            if (File.Exists(outputPath + @"\ui\message\msg_spirits.msbt"))
                File.Delete(outputPath + @"\ui\message\msg_spirits.msbt");
               
            File.Copy(basePath + "msg_spirits.MSBT", outputPath + @"\ui\message\msg_spirits.msbt");

            var spiritMSBT = new MSBT(outputPath + @"\ui\message\msg_spirits.msbt");
            var spiritList = ((ParamList)ui_spirit_db.Root.Nodes["db_root"]);

            var battleList = ((ParamList)ui_spirits_battle_db.Root.Nodes["battle_data_tbl"]);
            var fighterList = ((ParamList)ui_spirits_battle_db.Root.Nodes["fighter_data_tbl"]);
            var layoutList = ((ParamList)ui_spirit_layout_db.Root.Nodes["db_root"]);
            var deletedFighterList = new List<IParam>();

            // Get the most recent revision of each saved mod and all unsaved mods.
            var allMods = new List<Spirit>();

            foreach (var mod in modList.Values) allMods.Add(GetMods(mod));
            foreach (var mod in unsavedEdits.Values) allMods.Add(mod);

            bool bShowedWarning = false;
            backupIDs.Clear();

            if (spiritList.TypeKey.Equals(ParamType.list))
            {
                var list = spiritList;

                foreach (var mod in allMods)
                {
                    if(mod.Custom || mod.index > list.Nodes.Count)
                    {
                        var node = new ParamStruct(49);
                        var values = node.Nodes;

                        ushort save_no = mod.save_no;
                        if(save_no >= 2000 && DevMode)
                        {
                            save_no = System.Convert.ToUInt16(GetNextValidSaveIndex(true));

                            if(save_no >= 2000)
                            {
                                if (!bShowedWarning)
                                {
                                    MessageBox.Show($"Starting with {mod.display_name},\nthe total spirits " +
                                        $"exceed the game's current limit of 2,000.\n" +
                                        $"To prevent crashes, this and all following spirits will be ingnored.", "Hold up");

                                    bShowedWarning = true;
                                }

                                continue;
                            }

                            backupIDs.Add((int)save_no);
                        }

                        var v1 = new ParamValue(ParamType.@ushort, save_no);
                        values.Add(SpiritParameterNames.SAVE_NO, v1);
                        var v2 = new ParamValue(ParamType.hash40, mod.spirit_id);
                        values.Add(SpiritParameterNames.SPIRIT_ID, v2);
                        var v3 = new ParamValue(ParamType.@string, mod.name_id);
                        values.Add(SpiritParameterNames.NAME_ID, v3);
                        var v4 = new ParamValue(ParamType.@ushort, mod.fixed_no);
                        values.Add(SpiritParameterNames.FIXED_NO, v4);
                        var v5 = new ParamValue(ParamType.@bool, mod.is_bcat);
                        values.Add(SpiritParameterNames.IS_BCAT, v5);
                        var v6 = new ParamValue(ParamType.@bool, mod.is_dlc);
                        values.Add(SpiritParameterNames.IS_DLC, v6);
                        var v7 = new ParamValue(ParamType.@ushort, mod.directory_id);
                        values.Add(SpiritParameterNames.DIRECTORY_ID, v7);
                        var v8 = new ParamValue(ParamType.hash40, mod.type);
                        values.Add(SpiritParameterNames.TYPE, v8);
                        var v9 = new ParamValue(ParamType.hash40, mod.series_id);
                        values.Add(SpiritParameterNames.UI_SERIES_ID, v9);
                        var v9a = new ParamValue(ParamType.hash40, mod.rank);
                        values.Add(SpiritParameterNames.RANK, v9a);
                        var v10 = new ParamValue(ParamType.@byte, mod.slots);
                        values.Add(SpiritParameterNames.SLOT_NUM, v10);
                        var v11 = new ParamValue(ParamType.hash40, mod.ability_id);
                        values.Add(SpiritParameterNames.ABILITY_ID, v11);
                        var v12 = new ParamValue(ParamType.hash40, mod.spirit_attr);
                        values.Add(SpiritParameterNames.ATTR, v12);
                        var v13 = new ParamValue(ParamType.@uint, mod.exp_lvl_max);
                        values.Add(SpiritParameterNames.EXP_LVL_MAX, v13);
                        var v14 = new ParamValue(ParamType.@float, mod.exp_up_rate);
                        values.Add(SpiritParameterNames.EXP_UP_RATE, v14);
                        var v15 = new ParamValue(ParamType.@short, mod.base_attack);
                        values.Add(SpiritParameterNames.BASE_ATTACK, v15);
                        var v16 = new ParamValue(ParamType.@short, mod.max_attack);
                        values.Add(SpiritParameterNames.MAX_ATTACK, v16);
                        var v17 = new ParamValue(ParamType.@short, mod.base_defense);
                        values.Add(SpiritParameterNames.BASE_DEFENSE, v17);
                        var v18 = new ParamValue(ParamType.@short, mod.max_defense);
                        values.Add(SpiritParameterNames.MAX_DEFENSE, v18);
                        var v19 = new ParamValue(ParamType.hash40, mod.grow_type);
                        values.Add(SpiritParameterNames.GROW_TYPE, v19);
                        var v20 = new ParamValue(ParamType.@byte, mod.personality);
                        values.Add(SpiritParameterNames.PERSONALITY, v20);
                        var v21 = new ParamValue(ParamType.hash40, mod.evolve_src);
                        values.Add(SpiritParameterNames.EVOLVE_SRC, v21);
                        var v22 = new ParamValue(ParamType.hash40, mod.super_ability);
                        values.Add(SpiritParameterNames.SUPER_ABILITY, v22);
                        var v23 = new ParamValue(ParamType.@bool, mod.is_board_appear);
                        values.Add(SpiritParameterNames.IS_BOARD_APPEAR, v23);
                        var v24 = new ParamValue(ParamType.hash40, mod.fighter_conditions);
                        values.Add(SpiritParameterNames.FIGHTER_CONDITIONS, v24);
                        var v25 = new ParamValue(ParamType.hash40, mod.appear_conditions);
                        values.Add(SpiritParameterNames.APPEAR_CONDITIONS, v25);
                        var v26 = new ParamValue(ParamType.@uint, mod.x14ef76fcd7);
                        values.Add(SpiritParameterNames.x14ef76fcd7, v26);
                        var v27 = new ParamValue(ParamType.@uint, mod.x1531b0c6f0);
                        values.Add(SpiritParameterNames.x1531b0c6f0, v27);
                        var v28 = new ParamValue(ParamType.@float, mod.x16db57210b);
                        values.Add(SpiritParameterNames.x16db57210b, v28);
                        var v29 = new ParamValue(ParamType.@uint, mod.reward_capacity);
                        values.Add(SpiritParameterNames.REWARD_CAPACITY, v29);
                        var v30 = new ParamValue(ParamType.@uint, mod.battle_exp);
                        values.Add(SpiritParameterNames.BATTLE_EXP, v30);
                        var v31 = new ParamValue(ParamType.@uint, mod.summon_sp);
                        values.Add(SpiritParameterNames.SUMMON_SP, v31);
                        var v32 = new ParamValue(ParamType.hash40, mod.summon_id1);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_1, v32);
                        var v33 = new ParamValue(ParamType.@byte, mod.summon_quantity1);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_1_NUM, v33);
                        var v34 = new ParamValue(ParamType.hash40, mod.summon_id2);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_2, v34);
                        var v35 = new ParamValue(ParamType.@byte, mod.summon_quantity2);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_2_NUM, v35);
                        var v36 = new ParamValue(ParamType.hash40, mod.summon_id3);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_3, v36);
                        var v37 = new ParamValue(ParamType.@byte, mod.summon_quantity3);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_3_NUM, v37);
                        var v38 = new ParamValue(ParamType.hash40, mod.summon_id4);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_4, v38);
                        var v39 = new ParamValue(ParamType.@byte, mod.summon_quantity4);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_4_NUM, v39);
                        var v40 = new ParamValue(ParamType.hash40, mod.summon_id5);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_5, v40);
                        var v41 = new ParamValue(ParamType.@byte, mod.summon_quantity5);
                        values.Add(SpiritParameterNames.SUMMON_ITEM_5_NUM, v41);
                        var v42 = new ParamValue(ParamType.hash40, mod.game_title);
                        values.Add(SpiritParameterNames.GAME_TITLE, v42);
                        var v43 = new ParamValue(ParamType.hash40, mod.shop_sales_type);
                        values.Add(SpiritParameterNames.SHOP_SALES_TYPE, v43);
                        var v44 = new ParamValue(ParamType.@uint, mod.shop_price);
                        values.Add(SpiritParameterNames.SHOP_PRICE, v44);
                        var v45 = new ParamValue(ParamType.@bool, mod.rematch);
                        values.Add(SpiritParameterNames.REMATCH, v45);
                        var v46 = new ParamValue(ParamType.hash40, mod.x13656d7462);
                        values.Add(SpiritParameterNames.x13656d7462, v46);
                        var v47 = new ParamValue(ParamType.hash40, mod.check_id);
                        values.Add(SpiritParameterNames.CHECK_ID, v47);
                        var v48 = new ParamValue(ParamType.hash40, mod.x115044521b);
                        values.Add(SpiritParameterNames.x115044521b, v48);

                        spiritList.Nodes.Add(node);

                        var spiritName = spiritMSBT.LBL1.Labels.Find
                            ((lbl) => lbl.ToString().Equals("spi_" + mod.name_id));

                        if (spiritName != null)
                        {
                            var entry = ((MsbtEditor.Label)spiritName).String;
                            entry.Value = NameToBytes(mod.display_name);
                        }
                        else
                        {
                            spiritMSBT.AddLabel($"spi_{mod.name_id}");

                            spiritName = spiritMSBT.LBL1.Labels.Find
                                ((lbl) => lbl.ToString().Equals("spi_" + mod.name_id));
                            
                            var entry = ((MsbtEditor.Label)spiritName).String;
                            entry.Value = NameToBytes(mod.display_name);
                        }

                        OverwriteBattle(battleList, mod.battle, fighterList, deletedFighterList);
                        OverwriteLayout(layoutList, mod.aesthetics);
                    }
                    else
                    {
                        var node = list.Nodes[mod.index];

                        if (node.TypeKey.Equals(ParamType.@struct))
                        {
                            var values = ((ParamStruct)node).Nodes;

                            if (values.Count != 49)
                            {
                                throw new InvalidOperationException("Param is not a spirit.");
                            }
                            else
                            {
                                ulong spiritType = mod.type;
                                /*if (Force_Build_Support_To_Primary.IsChecked.Value && spiritType == 89039743027)
                                {
                                    spiritType = 81665422107;
                                }*/

                                // Overwrite param values
                                ((ParamValue)values[SpiritParameterNames.SAVE_NO]).Value = mod.save_no;
                                ((ParamValue)values[SpiritParameterNames.SPIRIT_ID]).Value = mod.spirit_id;
                                ((ParamValue)values[SpiritParameterNames.NAME_ID]).Value = mod.name_id;
                                ((ParamValue)values[SpiritParameterNames.FIXED_NO]).Value = mod.fixed_no;
                                ((ParamValue)values[SpiritParameterNames.IS_BCAT]).Value = mod.is_bcat;
                                ((ParamValue)values[SpiritParameterNames.IS_DLC]).Value = mod.is_dlc;
                                ((ParamValue)values[SpiritParameterNames.DIRECTORY_ID]).Value = mod.directory_id;
                                ((ParamValue)values[SpiritParameterNames.TYPE]).Value = spiritType;
                                ((ParamValue)values[SpiritParameterNames.UI_SERIES_ID]).Value = mod.series_id;
                                ((ParamValue)values[SpiritParameterNames.RANK]).Value = mod.rank;
                                ((ParamValue)values[SpiritParameterNames.SLOT_NUM]).Value = mod.slots;
                                ((ParamValue)values[SpiritParameterNames.ABILITY_ID]).Value = mod.ability_id;
                                ((ParamValue)values[SpiritParameterNames.ATTR]).Value = mod.spirit_attr;
                                ((ParamValue)values[SpiritParameterNames.EXP_LVL_MAX]).Value = mod.exp_lvl_max;
                                ((ParamValue)values[SpiritParameterNames.EXP_UP_RATE]).Value = mod.exp_up_rate;
                                ((ParamValue)values[SpiritParameterNames.BASE_ATTACK]).Value = mod.base_attack;
                                ((ParamValue)values[SpiritParameterNames.MAX_ATTACK]).Value = mod.max_attack;
                                ((ParamValue)values[SpiritParameterNames.BASE_DEFENSE]).Value = mod.base_defense;
                                ((ParamValue)values[SpiritParameterNames.MAX_DEFENSE]).Value = mod.max_defense;
                                ((ParamValue)values[SpiritParameterNames.GROW_TYPE]).Value = mod.grow_type;
                                ((ParamValue)values[SpiritParameterNames.PERSONALITY]).Value = mod.personality;
                                ((ParamValue)values[SpiritParameterNames.EVOLVE_SRC]).Value = mod.evolve_src;
                                ((ParamValue)values[SpiritParameterNames.SUPER_ABILITY]).Value = mod.super_ability;
                                ((ParamValue)values[SpiritParameterNames.IS_BOARD_APPEAR]).Value = mod.is_board_appear;
                                ((ParamValue)values[SpiritParameterNames.FIGHTER_CONDITIONS]).Value = mod.fighter_conditions;
                                ((ParamValue)values[SpiritParameterNames.APPEAR_CONDITIONS]).Value = mod.appear_conditions;
                                ((ParamValue)values[SpiritParameterNames.x14ef76fcd7]).Value = mod.x14ef76fcd7;
                                ((ParamValue)values[SpiritParameterNames.x1531b0c6f0]).Value = mod.x1531b0c6f0;
                                ((ParamValue)values[SpiritParameterNames.x16db57210b]).Value = mod.x16db57210b;
                                ((ParamValue)values[SpiritParameterNames.REWARD_CAPACITY]).Value = mod.reward_capacity;
                                ((ParamValue)values[SpiritParameterNames.BATTLE_EXP]).Value = mod.battle_exp;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_SP]).Value = mod.summon_sp;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_1]).Value = mod.summon_id1;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_2]).Value = mod.summon_id2;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_3]).Value = mod.summon_id3;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_4]).Value = mod.summon_id4;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_5]).Value = mod.summon_id5;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_1_NUM]).Value = mod.summon_quantity1;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_2_NUM]).Value = mod.summon_quantity2;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_3_NUM]).Value = mod.summon_quantity3;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_4_NUM]).Value = mod.summon_quantity4;
                                ((ParamValue)values[SpiritParameterNames.SUMMON_ITEM_5_NUM]).Value = mod.summon_quantity5;
                                ((ParamValue)values[SpiritParameterNames.GAME_TITLE]).Value = mod.game_title;
                                ((ParamValue)values[SpiritParameterNames.SHOP_SALES_TYPE]).Value = mod.shop_sales_type;
                                ((ParamValue)values[SpiritParameterNames.SHOP_PRICE]).Value = mod.shop_price;
                                ((ParamValue)values[SpiritParameterNames.REMATCH]).Value = mod.rematch;
                                ((ParamValue)values[SpiritParameterNames.x13656d7462]).Value = mod.x13656d7462;
                                ((ParamValue)values[SpiritParameterNames.CHECK_ID]).Value = mod.check_id;
                                ((ParamValue)values[SpiritParameterNames.x115044521b]).Value = mod.x115044521b;

                                // Update the MSBT
                                var spiritName = spiritMSBT.LBL1.Labels.Find
                                    ((lbl) => lbl.ToString().Equals("spi_" + mod.name_id));

                                if (spiritName != null)
                                {
                                    var entry = ((MsbtEditor.Label)spiritName).String;
                                    entry.Value = NameToBytes(mod.display_name);
                                }

                                // Fix for DLC Fighter Spirits battle tips
                                /*
                                if (mod.type == 86992126058)
                                {
                                    mod.battle.x0d6f19abae = false;
                                    mod.battle.aw_flap_delay = false;

                                    if (mod.alternateBattle != null)
                                    {
                                        mod.battle.x0d6f19abae = false;
                                        mod.battle.aw_flap_delay = false;
                                    }
                                }

                                if (mod.type != 86992126058 && baseSpirits[mod.spirit_id].type == 86992126058)
                                {
                                    mod.battle.x0d6f19abae = true;
                                    mod.battle.aw_flap_delay = true;

                                    if (mod.alternateBattle != null)
                                    {
                                        mod.battle.x0d6f19abae = true;
                                        mod.battle.aw_flap_delay = true;
                                    }
                                }*/

                                // Update the battles
                                OverwriteBattle(battleList, mod.battle, fighterList, deletedFighterList);
                                if (mod.alternateBattle != null) OverwriteBattle(battleList, mod.alternateBattle, fighterList, deletedFighterList);

                                // Test content, pls ignore
                                /*if (mod.type == 86992126058 && mod.battle.fighters[0].color == 0)
                                {
                                    //MessageBox.Show("Duplicating battle for " + mod.display_name);
                                    battleList.Nodes.Add(battleList.Nodes[mod.battle.index].Clone());

                                    //WE NEED TO CHANGE THE BATTLE ID OF THE FIGHTER AND ALSO CHANGE THE FIGHTER'S LEVEL AND POWER
                                    fighterList.Nodes.Add(fighterList.Nodes[mod.battle.fighterDBIndexes[0]].Clone());

                                    var EX_Battle = ((ParamStruct)(battleList.Nodes[battleList.Nodes.Count - 1])).Nodes;
                                    ((ParamValue)EX_Battle[SpiritParameterNames.BATTLE_ID]).Value =
                                        Hash40Util.StringToHash40("ex_battle_" +
                                        SpiritValueDisplay.FighterIDs[mod.battle.fighters[0].fighter_kind].name.ToLower().
                                        Replace(" ", "").Replace("é", "e").Replace(".", ""));
                                }*/

                                // Update the layout
                                OverwriteLayout(layoutList, mod.aesthetics);
                            }
                        }
                    }
                }
            }

            foreach (var node in deletedFighterList)
            {
                fighterList.Nodes.Remove(node);
            }

            ui_spirit_db.Save(outputPath + @"\ui\param\database\ui_spirit_db.prc");
            ui_spirits_battle_db.Save(outputPath + @"\ui\param\database\ui_spirits_battle_db.prc");
            ui_spirit_layout_db.Save(outputPath + @"\ui\param\database\ui_spirit_layout_db.prc");
            spiritMSBT.Save();

            BuildDLCSpirits();

            building = false;

            if(bShowLogs)
                MessageBox.Show("All mods exported to /ArcOutput.", "It's done.");
        }


        public void OverwriteBattle(ParamList battleList, SpiritBattle battle, ParamList fighterList, List<IParam> toRemove)
        {
            if (battle.index == -1)
            {
                var node = new ParamStruct(59);
                var values = node.Nodes;

                var v1 = new ParamValue(ParamType.hash40, battle.battle_id);
                values.Add(SpiritParameterNames.BATTLE_ID, v1);
                var v2 = new ParamValue(ParamType.hash40, battle.battle_type);
                values.Add(SpiritParameterNames.BATTLE_TYPE, v2);
                var v3 = new ParamValue(ParamType.@ushort, battle.battle_time_sec);
                values.Add(SpiritParameterNames.BATTLE_TIME_SEC, v3);
                var v4 = new ParamValue(ParamType.@ushort, battle.basic_init_damage);
                values.Add(SpiritParameterNames.BASIC_INIT_DAMAGE, v4);
                var v5 = new ParamValue(ParamType.@ushort, battle.basic_init_hp);
                values.Add(SpiritParameterNames.BASIC_INIT_HP, v5);
                var v6 = new ParamValue(ParamType.@byte, battle.basic_stock);
                values.Add(SpiritParameterNames.BASIC_STOCK, v6);
                var v7 = new ParamValue(ParamType.hash40, battle.stage_id);
                values.Add(SpiritParameterNames.UI_STAGE_ID, v7);
                var v8 = new ParamValue(ParamType.hash40, battle.stage_type);
                values.Add(SpiritParameterNames.STAGE_TYPE, v8);
                var v9 = new ParamValue(ParamType.@sbyte, battle.x18e536d4f7);
                values.Add(SpiritParameterNames.x18e536d4f7, v9);
                var v10 = new ParamValue(ParamType.hash40, battle.stage_bgm);
                values.Add(SpiritParameterNames.STAGE_BGM, v10);
                var v11 = new ParamValue(ParamType.@bool, battle.stage_gimmick);
                values.Add(SpiritParameterNames.STAGE_GIMMICK, v11);
                var v12 = new ParamValue(ParamType.hash40, battle.stage_attr);
                values.Add(SpiritParameterNames.STAGE_ATTR, v12);
                var v13 = new ParamValue(ParamType.hash40, battle.floor_place_id);
                values.Add(SpiritParameterNames.FLOOR_PLACE_ID, v13);
                var v14 = new ParamValue(ParamType.hash40, battle.item_table);
                values.Add(SpiritParameterNames.ITEM_TABLE, v14);
                var v15 = new ParamValue(ParamType.hash40, battle.item_level);
                values.Add(SpiritParameterNames.ITEM_LEVEL, v15);
                var v16 = new ParamValue(ParamType.hash40, battle.result_type);
                values.Add(SpiritParameterNames.RESULT_TYPE, v16);
                var v17 = new ParamValue(ParamType.hash40, battle.event1_type);
                values.Add(SpiritParameterNames.EVENT1_TYPE, v17);
                var v18 = new ParamValue(ParamType.hash40, battle.event1_label);
                values.Add(SpiritParameterNames.EVENT1_LABEL, v18);
                var v19 = new ParamValue(ParamType.@int, battle.event1_start_time);
                values.Add(SpiritParameterNames.EVENT1_START_TIME, v19);
                var v20 = new ParamValue(ParamType.@int, battle.event1_range_time);
                values.Add(SpiritParameterNames.EVENT1_RANGE_TIME, v20);
                var v21 = new ParamValue(ParamType.@byte, battle.event1_count);
                values.Add(SpiritParameterNames.EVENT1_COUNT, v21);
                var v22 = new ParamValue(ParamType.@ushort, battle.event1_damage);
                values.Add(SpiritParameterNames.EVENT1_DAMAGE, v22);
                var v23 = new ParamValue(ParamType.hash40, battle.event2_type);
                values.Add(SpiritParameterNames.EVENT2_TYPE, v23);
                var v24 = new ParamValue(ParamType.hash40, battle.event2_label);
                values.Add(SpiritParameterNames.EVENT2_LABEL, v24);
                var v25 = new ParamValue(ParamType.@int, battle.event2_start_time);
                values.Add(SpiritParameterNames.EVENT2_START_TIME, v25);
                var v26 = new ParamValue(ParamType.@int, battle.event2_range_time);
                values.Add(SpiritParameterNames.EVENT2_RANGE_TIME, v26);
                var v27 = new ParamValue(ParamType.@byte, battle.event2_count);
                values.Add(SpiritParameterNames.EVENT2_COUNT, v27);
                var v28 = new ParamValue(ParamType.@ushort, battle.event2_damage);
                values.Add(SpiritParameterNames.EVENT2_DAMAGE, v28);
                var v29 = new ParamValue(ParamType.hash40, battle.event3_type);
                values.Add(SpiritParameterNames.EVENT3_TYPE, v29);
                var v30 = new ParamValue(ParamType.hash40, battle.event3_label);
                values.Add(SpiritParameterNames.EVENT3_LABEL, v30);
                var v31 = new ParamValue(ParamType.@int, battle.event3_start_time);
                values.Add(SpiritParameterNames.EVENT3_START_TIME, v31);
                var v32 = new ParamValue(ParamType.@int, battle.event3_range_time);
                values.Add(SpiritParameterNames.EVENT3_RANGE_TIME, v32);
                var v33 = new ParamValue(ParamType.@byte, battle.event3_count);
                values.Add(SpiritParameterNames.EVENT3_COUNT, v33);
                var v34 = new ParamValue(ParamType.@ushort, battle.event3_damage);
                values.Add(SpiritParameterNames.EVENT3_DAMAGE, v34);
                var v35 = new ParamValue(ParamType.@bool, battle.x0d41ef8328);
                values.Add(SpiritParameterNames.x0d41ef8328, v35);
                var v36 = new ParamValue(ParamType.@bool, battle.aw_flap_delay);
                values.Add(SpiritParameterNames.AW_FLAP_DELAY, v36);
                var v37 = new ParamValue(ParamType.@bool, battle.x0d6f19abae);
                values.Add(SpiritParameterNames.x0d6f19abae, v37);
                var v38 = new ParamValue(ParamType.hash40, battle.auto_win_skill);
                values.Add(SpiritParameterNames.POWER_SKILL_1, v38);
                var v39 = new ParamValue(ParamType.hash40, battle.x18404d4ecb);
                values.Add(SpiritParameterNames.POWER_SKILL_2, v39);
                var v40 = new ParamValue(ParamType.hash40, battle.recommended_skills[0]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_1, v40);
                var v41 = new ParamValue(ParamType.hash40, battle.recommended_skills[1]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_2, v41);
                var v42 = new ParamValue(ParamType.hash40, battle.recommended_skills[2]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_3, v42);
                var v43 = new ParamValue(ParamType.hash40, battle.recommended_skills[3]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_4, v43);
                var v44 = new ParamValue(ParamType.hash40, battle.recommended_skills[4]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_5, v44);
                var v45 = new ParamValue(ParamType.hash40, battle.recommended_skills[5]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_6, v45);
                var v46 = new ParamValue(ParamType.hash40, battle.recommended_skills[6]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_7, v46);
                var v47 = new ParamValue(ParamType.hash40, battle.recommended_skills[7]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_8, v47);
                var v48 = new ParamValue(ParamType.hash40, battle.recommended_skills[8]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_9, v48);
                var v49 = new ParamValue(ParamType.hash40, battle.recommended_skills[9]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_10, v49);
                var v50 = new ParamValue(ParamType.hash40, battle.recommended_skills[10]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_11, v50);
                var v51 = new ParamValue(ParamType.hash40, battle.recommended_skills[11]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_12, v51);
                var v52 = new ParamValue(ParamType.hash40, battle.recommended_skills[12]);
                values.Add(SpiritParameterNames.RECOMMENDED_SKILL_13, v52);
                var v53 = new ParamValue(ParamType.hash40, battle.un_recommended_skills[0]);
                values.Add(SpiritParameterNames.UNRECOMMENDED_SKILL_1, v53);
                var v54 = new ParamValue(ParamType.hash40, battle.un_recommended_skills[1]);
                values.Add(SpiritParameterNames.UNRECOMMENDED_SKILL_2, v54);
                var v55 = new ParamValue(ParamType.hash40, battle.un_recommended_skills[2]);
                values.Add(SpiritParameterNames.UNRECOMMENDED_SKILL_3, v55);
                var v56 = new ParamValue(ParamType.hash40, battle.un_recommended_skills[3]);
                values.Add(SpiritParameterNames.UNRECOMMENDED_SKILL_4, v56);
                var v57 = new ParamValue(ParamType.hash40, battle.un_recommended_skills[4]);
                values.Add(SpiritParameterNames.UNRECOMMENDED_SKILL_5, v57);
                var v58 = new ParamValue(ParamType.hash40, battle.x0ff8afd14f);
                values.Add(SpiritParameterNames.x0ff8afd14f, v58);
                var v59 = new ParamValue(ParamType.@uint, battle.battle_power);
                values.Add(SpiritParameterNames.BATTLE_POWER, v59);

                battleList.Nodes.Add(node);

                // Add the fighters
                for (int i = 0; i < battle.fighters.Count; i++)
                {
                    OverwriteFighter(battle.fighters[i], fighterList, -1);
                }
            }
            else
            {
                // Get the node representing the battle in an editable form
                var node = battleList.Nodes[battle.index];

                if (node.TypeKey.Equals(ParamType.@struct))
                {
                    var values = ((ParamStruct)node).Nodes;

                    ((ParamValue)values[SpiritParameterNames.BATTLE_ID]).Value = battle.battle_id;
                    ((ParamValue)values[SpiritParameterNames.BATTLE_TYPE]).Value = battle.battle_type;
                    ((ParamValue)values[SpiritParameterNames.BATTLE_TIME_SEC]).Value = battle.battle_time_sec;
                    ((ParamValue)values[SpiritParameterNames.BASIC_INIT_DAMAGE]).Value = battle.basic_init_damage;
                    ((ParamValue)values[SpiritParameterNames.BASIC_INIT_HP]).Value = battle.basic_init_hp;
                    ((ParamValue)values[SpiritParameterNames.BASIC_STOCK]).Value = battle.basic_stock;
                    ((ParamValue)values[SpiritParameterNames.UI_STAGE_ID]).Value = battle.stage_id;
                    ((ParamValue)values[SpiritParameterNames.STAGE_TYPE]).Value = battle.stage_type;
                    ((ParamValue)values[SpiritParameterNames.x18e536d4f7]).Value = battle.x18e536d4f7;
                    ((ParamValue)values[SpiritParameterNames.STAGE_BGM]).Value = battle.stage_bgm;
                    ((ParamValue)values[SpiritParameterNames.STAGE_GIMMICK]).Value = battle.stage_gimmick;
                    ((ParamValue)values[SpiritParameterNames.STAGE_ATTR]).Value = battle.stage_attr;
                    ((ParamValue)values[SpiritParameterNames.FLOOR_PLACE_ID]).Value = battle.floor_place_id;
                    ((ParamValue)values[SpiritParameterNames.ITEM_TABLE]).Value = battle.item_table;
                    ((ParamValue)values[SpiritParameterNames.ITEM_LEVEL]).Value = battle.item_level;
                    ((ParamValue)values[SpiritParameterNames.RESULT_TYPE]).Value = battle.result_type;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_TYPE]).Value = battle.event1_type;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_LABEL]).Value = battle.event1_label;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_START_TIME]).Value = battle.event1_start_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_RANGE_TIME]).Value = battle.event1_range_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_COUNT]).Value = battle.event1_count;
                    ((ParamValue)values[SpiritParameterNames.EVENT1_DAMAGE]).Value = battle.event1_damage;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_TYPE]).Value = battle.event2_type;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_LABEL]).Value = battle.event2_label;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_START_TIME]).Value = battle.event2_start_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_RANGE_TIME]).Value = battle.event2_range_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_COUNT]).Value = battle.event2_count;
                    ((ParamValue)values[SpiritParameterNames.EVENT2_DAMAGE]).Value = battle.event2_damage;
                    ((ParamValue)values[SpiritParameterNames.EVENT3_TYPE]).Value = battle.event3_type;
                    ((ParamValue)values[SpiritParameterNames.EVENT3_LABEL]).Value = battle.event3_label;
                    ((ParamValue)values[SpiritParameterNames.EVENT3_START_TIME]).Value = battle.event3_start_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT3_RANGE_TIME]).Value = battle.event3_range_time;
                    ((ParamValue)values[SpiritParameterNames.EVENT3_COUNT]).Value = battle.event3_count;

                    if (values.ContainsKey(SpiritParameterNames.EVENT3_DAMAGE))
                    {
                        var g = values.Find((h) => h.Key == SpiritParameterNames.EVENT3_DAMAGE);

                        if (g != null && g.Value.TypeKey == ParamType.@bool)
                        {
                            values.Remove(g);

                            var v34 = new ParamValue(ParamType.@ushort, battle.event3_damage);
                            values.Add(SpiritParameterNames.EVENT3_DAMAGE, v34);
                        }
                        else
                        {
                            ((ParamValue)values[SpiritParameterNames.EVENT3_DAMAGE]).Value = battle.event3_damage;
                        }

                    }
                    else
                    {
                        var v34 = new ParamValue(ParamType.@ushort, battle.event3_damage);
                        values.Add(SpiritParameterNames.EVENT3_DAMAGE, v34);
                    }

                    ((ParamValue)values[SpiritParameterNames.EVENT3_DAMAGE]).Value = battle.event3_damage;
                    ((ParamValue)values[SpiritParameterNames.x0d41ef8328]).Value = battle.x0d41ef8328;
                    ((ParamValue)values[SpiritParameterNames.AW_FLAP_DELAY]).Value = battle.aw_flap_delay;

                    if(values.ContainsKey(SpiritParameterNames.x0d6f19abae))
                        ((ParamValue)values[SpiritParameterNames.x0d6f19abae]).Value = battle.x0d6f19abae;
                    else
                    {
                        var v37 = new ParamValue(ParamType.@bool, battle.x0d6f19abae);
                        values.Add(SpiritParameterNames.x0d6f19abae, v37);
                    }
                    ((ParamValue)values[SpiritParameterNames.POWER_SKILL_1]).Value = battle.auto_win_skill;
                    ((ParamValue)values[SpiritParameterNames.POWER_SKILL_2]).Value = battle.x18404d4ecb;
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_1]).Value = battle.recommended_skills[0];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_2]).Value = battle.recommended_skills[1];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_3]).Value = battle.recommended_skills[2];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_4]).Value = battle.recommended_skills[3];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_5]).Value = battle.recommended_skills[4];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_6]).Value = battle.recommended_skills[5];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_7]).Value = battle.recommended_skills[6];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_8]).Value = battle.recommended_skills[7];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_9]).Value = battle.recommended_skills[8];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_10]).Value = battle.recommended_skills[9];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_11]).Value = battle.recommended_skills[10];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_12]).Value = battle.recommended_skills[11];
                    ((ParamValue)values[SpiritParameterNames.RECOMMENDED_SKILL_13]).Value = battle.recommended_skills[12];
                    ((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_1]).Value = battle.un_recommended_skills[0];
                    ((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_2]).Value = battle.un_recommended_skills[1];
                    ((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_3]).Value = battle.un_recommended_skills[2];
                    ((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_4]).Value = battle.un_recommended_skills[3];
                    ((ParamValue)values[SpiritParameterNames.UNRECOMMENDED_SKILL_5]).Value = battle.un_recommended_skills[4];
                    ((ParamValue)values[SpiritParameterNames.x0ff8afd14f]).Value = battle.x0ff8afd14f;
                    ((ParamValue)values[SpiritParameterNames.BATTLE_POWER]).Value = battle.battle_power;

                    for (int i = 0; i < battle.fighters.Count; i++)
                    {
                        int fighterIndex = i >= battle.fighterDBIndexes.Count ? -1 : battle.fighterDBIndexes[i];
                        OverwriteFighter(battle.fighters[i], fighterList, fighterIndex);
                    }

                    // Erase deleted fighters if the number of fighters is less than the full list
                    for (int i = battle.fighters.Count; i < battle.fighterDBIndexes.Count; i++)
                    {
                        toRemove.Add(fighterList.Nodes[battle.fighterDBIndexes[i]]);
                    }
                }
            }
        }


        public void OverwriteLayout(ParamList layoutList, SpiritAesthetics layout)
        {
            if(layout.index == -1)
            {
                var node = new ParamStruct(44);
                var values = node.Nodes;

                var v1 = new ParamValue(ParamType.hash40, layout.layout_id);
                values.Add(SpiritParameterNames.LAYOUT_ID, v1);
                var v2 = new ParamValue(ParamType.@float, layout.art_pos_x[0]);
                values.Add(SpiritParameterNames.ART_CENTER_X, v2);
                var v3 = new ParamValue(ParamType.@float, layout.art_pos_y[0]);
                values.Add(SpiritParameterNames.ART_CENTER_Y, v3);
                var v4 = new ParamValue(ParamType.@float, layout.art_size[0]);
                values.Add(SpiritParameterNames.ART_SCALE, v4);
                var v5 = new ParamValue(ParamType.@float, layout.art_pos_x[1]);
                values.Add(SpiritParameterNames.ART_STAND_CENTER_X, v5);
                var v6 = new ParamValue(ParamType.@float, layout.art_pos_y[1]);
                values.Add(SpiritParameterNames.ART_STAND_CENTER_Y, v6);
                var v7 = new ParamValue(ParamType.@float, layout.art_size[1]);
                values.Add(SpiritParameterNames.ART_STAND_SCALE, v7);
                var v8 = new ParamValue(ParamType.@float, layout.art_pos_x[2]);
                values.Add(SpiritParameterNames.ART_BOARD_X, v8);
                var v9 = new ParamValue(ParamType.@float, layout.art_pos_y[2]);
                values.Add(SpiritParameterNames.ART_BOARD_Y, v9);
                var v10 = new ParamValue(ParamType.@float, layout.art_size[2]);
                values.Add(SpiritParameterNames.ART_BOARD_SCALE, v10);
                var v11 = new ParamValue(ParamType.@float, layout.art_pos_x[3]);
                values.Add(SpiritParameterNames.ART_SELECT_X, v11);
                var v12 = new ParamValue(ParamType.@float, layout.art_pos_y[3]);
                values.Add(SpiritParameterNames.ART_SELECT_Y, v12);
                var v13 = new ParamValue(ParamType.@float, layout.art_size[3]);
                values.Add(SpiritParameterNames.ART_SELECT_SCALE, v13);
                var v14 = new ParamValue(ParamType.@uint, layout.effect_count);
                values.Add(SpiritParameterNames.EFFECT_COUNT, v14);
                var v15 = new ParamValue(ParamType.@int, layout.effect_pos_x[0]);
                values.Add(SpiritParameterNames.EFFECT_0_X, v15);
                var v16 = new ParamValue(ParamType.@int, layout.effect_pos_y[0]);
                values.Add(SpiritParameterNames.EFFECT_0_Y, v16);
                var v17 = new ParamValue(ParamType.@int, layout.effect_pos_x[1]);
                values.Add(SpiritParameterNames.EFFECT_1_X, v17);
                var v18 = new ParamValue(ParamType.@int, layout.effect_pos_y[1]);
                values.Add(SpiritParameterNames.EFFECT_1_Y, v18);
                var v19 = new ParamValue(ParamType.@int, layout.effect_pos_x[2]);
                values.Add(SpiritParameterNames.EFFECT_2_X, v19);
                var v20 = new ParamValue(ParamType.@int, layout.effect_pos_y[2]);
                values.Add(SpiritParameterNames.EFFECT_2_Y, v20);
                var v21 = new ParamValue(ParamType.@int, layout.effect_pos_x[3]);
                values.Add(SpiritParameterNames.EFFECT_3_X, v21);
                var v22 = new ParamValue(ParamType.@int, layout.effect_pos_y[3]);
                values.Add(SpiritParameterNames.EFFECT_3_Y, v22);
                var v23 = new ParamValue(ParamType.@int, layout.effect_pos_x[4]);
                values.Add(SpiritParameterNames.EFFECT_4_X, v23);
                var v24 = new ParamValue(ParamType.@int, layout.effect_pos_y[4]);
                values.Add(SpiritParameterNames.EFFECT_4_Y, v24);
                var v25 = new ParamValue(ParamType.@int, layout.effect_pos_x[5]);
                values.Add(SpiritParameterNames.EFFECT_5_X, v25);
                var v26 = new ParamValue(ParamType.@int, layout.effect_pos_y[5]);
                values.Add(SpiritParameterNames.EFFECT_5_Y, v26);
                var v27 = new ParamValue(ParamType.@int, layout.effect_pos_x[6]);
                values.Add(SpiritParameterNames.EFFECT_6_X, v27);
                var v28 = new ParamValue(ParamType.@int, layout.effect_pos_y[6]);
                values.Add(SpiritParameterNames.EFFECT_6_Y, v28);
                var v29 = new ParamValue(ParamType.@int, layout.effect_pos_x[7]);
                values.Add(SpiritParameterNames.EFFECT_7_X, v29);
                var v30 = new ParamValue(ParamType.@int, layout.effect_pos_y[7]);
                values.Add(SpiritParameterNames.EFFECT_7_Y, v30);
                var v31 = new ParamValue(ParamType.@int, layout.effect_pos_x[8]);
                values.Add(SpiritParameterNames.EFFECT_8_X, v31);
                var v32 = new ParamValue(ParamType.@int, layout.effect_pos_y[8]);
                values.Add(SpiritParameterNames.EFFECT_8_Y, v32);
                var v33 = new ParamValue(ParamType.@int, layout.effect_pos_x[9]);
                values.Add(SpiritParameterNames.EFFECT_9_X, v33);
                var v34 = new ParamValue(ParamType.@int, layout.effect_pos_y[9]);
                values.Add(SpiritParameterNames.EFFECT_9_Y, v34);
                var v35 = new ParamValue(ParamType.@int, layout.effect_pos_x[10]);
                values.Add(SpiritParameterNames.EFFECT_10_X, v35);
                var v36 = new ParamValue(ParamType.@int, layout.effect_pos_y[10]);
                values.Add(SpiritParameterNames.EFFECT_10_Y, v36);
                var v37 = new ParamValue(ParamType.@int, layout.effect_pos_x[11]);
                values.Add(SpiritParameterNames.EFFECT_11_X, v37);
                var v38 = new ParamValue(ParamType.@int, layout.effect_pos_y[11]);
                values.Add(SpiritParameterNames.EFFECT_11_Y, v38);
                var v39 = new ParamValue(ParamType.@int, layout.effect_pos_x[12]);
                values.Add(SpiritParameterNames.EFFECT_12_X, v39);
                var v40 = new ParamValue(ParamType.@int, layout.effect_pos_y[12]);
                values.Add(SpiritParameterNames.EFFECT_12_Y, v40);
                var v41 = new ParamValue(ParamType.@int, layout.effect_pos_x[13]);
                values.Add(SpiritParameterNames.EFFECT_13_X, v41);
                var v42 = new ParamValue(ParamType.@int, layout.effect_pos_y[13]);
                values.Add(SpiritParameterNames.EFFECT_13_Y, v42);
                var v43 = new ParamValue(ParamType.@int, layout.effect_pos_x[14]);
                values.Add(SpiritParameterNames.EFFECT_14_X, v43);
                var v44 = new ParamValue(ParamType.@int, layout.effect_pos_y[14]);
                values.Add(SpiritParameterNames.EFFECT_14_Y, v44);

                layoutList.Nodes.Add(node);
            }
            else
            {
                // Get the node representing the battle in an editable form
                var node = layoutList.Nodes[layout.index];

                if (node.TypeKey.Equals(ParamType.@struct))
                {
                    var values = ((ParamStruct)node).Nodes;

                    ((ParamValue)values[SpiritParameterNames.ART_CENTER_X]).Value = layout.art_pos_x[0];
                    ((ParamValue)values[SpiritParameterNames.ART_CENTER_Y]).Value = layout.art_pos_y[0];
                    ((ParamValue)values[SpiritParameterNames.ART_SCALE]).Value = layout.art_size[0];

                    ((ParamValue)values[SpiritParameterNames.ART_STAND_CENTER_X]).Value = layout.art_pos_x[1];
                    ((ParamValue)values[SpiritParameterNames.ART_STAND_CENTER_Y]).Value = layout.art_pos_y[1];
                    ((ParamValue)values[SpiritParameterNames.ART_STAND_SCALE]).Value = layout.art_size[1];

                    ((ParamValue)values[SpiritParameterNames.ART_BOARD_X]).Value = layout.art_pos_x[2];
                    ((ParamValue)values[SpiritParameterNames.ART_BOARD_Y]).Value = layout.art_pos_y[2];
                    ((ParamValue)values[SpiritParameterNames.ART_BOARD_SCALE]).Value = layout.art_size[2];

                    ((ParamValue)values[SpiritParameterNames.ART_SELECT_X]).Value = layout.art_pos_x[3];
                    ((ParamValue)values[SpiritParameterNames.ART_SELECT_Y]).Value = layout.art_pos_y[3];
                    ((ParamValue)values[SpiritParameterNames.ART_SELECT_SCALE]).Value = layout.art_size[3];

                    ((ParamValue)values[SpiritParameterNames.EFFECT_COUNT]).Value = layout.effect_count;
                    ((ParamValue)values[SpiritParameterNames.EFFECT_0_X]).Value = layout.effect_pos_x[0];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_0_Y]).Value = layout.effect_pos_y[0];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_1_X]).Value = layout.effect_pos_x[1];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_1_Y]).Value = layout.effect_pos_y[1];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_2_X]).Value = layout.effect_pos_x[2];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_2_Y]).Value = layout.effect_pos_y[2];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_3_X]).Value = layout.effect_pos_x[3];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_3_Y]).Value = layout.effect_pos_y[3];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_4_X]).Value = layout.effect_pos_x[4];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_4_Y]).Value = layout.effect_pos_y[4];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_5_X]).Value = layout.effect_pos_x[5];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_5_Y]).Value = layout.effect_pos_y[5];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_6_X]).Value = layout.effect_pos_x[6];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_6_Y]).Value = layout.effect_pos_y[6];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_7_X]).Value = layout.effect_pos_x[7];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_7_Y]).Value = layout.effect_pos_y[7];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_8_X]).Value = layout.effect_pos_x[8];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_8_Y]).Value = layout.effect_pos_y[8];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_9_X]).Value = layout.effect_pos_x[9];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_9_Y]).Value = layout.effect_pos_y[9];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_10_X]).Value = layout.effect_pos_x[10];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_10_Y]).Value = layout.effect_pos_y[10];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_11_X]).Value = layout.effect_pos_x[11];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_11_Y]).Value = layout.effect_pos_y[11];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_12_X]).Value = layout.effect_pos_x[12];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_12_Y]).Value = layout.effect_pos_y[12];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_13_X]).Value = layout.effect_pos_x[13];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_13_Y]).Value = layout.effect_pos_y[13];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_14_X]).Value = layout.effect_pos_x[14];
                    ((ParamValue)values[SpiritParameterNames.EFFECT_14_Y]).Value = layout.effect_pos_y[14];
                }
            }
        }


        public void OverwriteFighter(SpiritBattleFighter fighter, ParamList fighterList, int fighterIndex)
        {
            // Add a new node for the fighter
            if (fighterIndex == -1)
            {
                var node = new ParamStruct(37);
                var values = node.Nodes;

                var v1 = new ParamValue(ParamType.hash40, fighter.battle_id);
                values.Add(SpiritParameterNames.BATTLE_ID, v1);
                var v2 = new ParamValue(ParamType.@byte, fighter.stock);
                values.Add(SpiritParameterNames.STOCK, v2);
                var v3 = new ParamValue(ParamType.@ushort, fighter.hp);
                values.Add(SpiritParameterNames.HP, v3);
                var v4 = new ParamValue(ParamType.hash40, fighter.entry_type);
                values.Add(SpiritParameterNames.ENTRY_TYPE, v4);
                var v5 = new ParamValue(ParamType.@bool, fighter.first_appear);
                values.Add(SpiritParameterNames.FIRST_APPEAR, v5);
                var v6 = new ParamValue(ParamType.@ushort, fighter.appear_rule_time);
                values.Add(SpiritParameterNames.APPEAR_RULE_TIME, v6);
                var v7 = new ParamValue(ParamType.@ushort, fighter.appear_rule_count);
                values.Add(SpiritParameterNames.APPEAR_RULE_COUNT, v7);
                var v8 = new ParamValue(ParamType.hash40, fighter.fighter_kind);
                values.Add(SpiritParameterNames.FIGHTER_KIND, v8);
                var v9 = new ParamValue(ParamType.@byte, fighter.color);
                values.Add(SpiritParameterNames.COLOR, v9);
                var v10 = new ParamValue(ParamType.hash40, fighter.mii_hat_id);
                values.Add(SpiritParameterNames.MII_HAT_ID, v10);
                var v11 = new ParamValue(ParamType.hash40, fighter.mii_body_id);
                values.Add(SpiritParameterNames.MII_BODY_ID, v11);
                var v12 = new ParamValue(ParamType.@byte, fighter.mii_color);
                values.Add(SpiritParameterNames.MII_COLOR, v12);
                var v13 = new ParamValue(ParamType.hash40, fighter.mii_voice);
                values.Add(SpiritParameterNames.MII_VOICE, v13);
                var v14 = new ParamValue(ParamType.@byte, fighter.mii_sp_n);
                values.Add(SpiritParameterNames.MII_SP_N, v14);
                var v15 = new ParamValue(ParamType.@byte, fighter.mii_sp_s);
                values.Add(SpiritParameterNames.MII_SP_S, v15);
                var v16 = new ParamValue(ParamType.@byte, fighter.mii_sp_hi);
                values.Add(SpiritParameterNames.MII_SP_HI, v16);
                var v17 = new ParamValue(ParamType.@byte, fighter.mii_sp_lw);
                values.Add(SpiritParameterNames.MII_SP_LW, v17);
                var v18 = new ParamValue(ParamType.@byte, fighter.cpu_lv);
                values.Add(SpiritParameterNames.CPU_LV, v18);
                var v19 = new ParamValue(ParamType.hash40, fighter.cpu_type);
                values.Add(SpiritParameterNames.CPU_TYPE, v19);
                var v20 = new ParamValue(ParamType.hash40, fighter.cpu_sub_type);
                values.Add(SpiritParameterNames.CPU_SUB_TYPE, v20);
                var v21 = new ParamValue(ParamType.@bool, fighter.cpu_item_pick_up);
                values.Add(SpiritParameterNames.CPU_ITEM_PICK_UP, v21);
                var v22 = new ParamValue(ParamType.@bool, fighter.corps);
                values.Add(SpiritParameterNames.CORPS, v22);
                var v23 = new ParamValue(ParamType.@bool, fighter.x0f2077926c);
                values.Add(SpiritParameterNames.x0f2077926c, v23);
                var v24 = new ParamValue(ParamType.@ushort, fighter.init_damage);
                values.Add(SpiritParameterNames.INIT_DAMAGE, v24);
                var v25 = new ParamValue(ParamType.hash40, fighter.sub_rule);
                values.Add(SpiritParameterNames.SUB_RULE, v25);
                var v26 = new ParamValue(ParamType.@float, fighter.scale);
                values.Add(SpiritParameterNames.SCALE, v26);
                var v27 = new ParamValue(ParamType.@float, fighter.fly_rate);
                values.Add(SpiritParameterNames.FLY_RATE, v27);
                var v28 = new ParamValue(ParamType.@bool, fighter.invalid_drop);
                values.Add(SpiritParameterNames.INVALID_DROP, v28);
                var v29 = new ParamValue(ParamType.@bool, fighter.enable_charge_final);
                values.Add(SpiritParameterNames.ENABLE_CHARGE_FINAL, v29);
                var v30 = new ParamValue(ParamType.hash40, fighter.spirit_name);
                values.Add(SpiritParameterNames.SPIRIT_NAME, v30);
                var v31 = new ParamValue(ParamType.@short, fighter.attack);
                values.Add(SpiritParameterNames.ATTACK, v31);
                var v32 = new ParamValue(ParamType.@short, fighter.defense);
                values.Add(SpiritParameterNames.DEFENSE, v32);
                var v33 = new ParamValue(ParamType.hash40, fighter.attr);
                values.Add(SpiritParameterNames.ATTR, v33);
                var v34 = new ParamValue(ParamType.hash40, fighter.ability_1);
                values.Add(SpiritParameterNames.ABILITY1, v34);
                var v35 = new ParamValue(ParamType.hash40, fighter.ability_2);
                values.Add(SpiritParameterNames.ABILITY2, v35);
                var v36 = new ParamValue(ParamType.hash40, fighter.ability_3);
                values.Add(SpiritParameterNames.ABILITY3, v36);
                var v37 = new ParamValue(ParamType.hash40, fighter.ability_personal);
                values.Add(SpiritParameterNames.ABILITY_PERSONAL, v37);

                fighterList.Nodes.Add(node);
            }

            // Get the node representing the fighter in an editable form
            else
            {
                var node = fighterList.Nodes[fighterIndex];

                if (node.TypeKey.Equals(ParamType.@struct))
                {
                    var values = ((ParamStruct)node).Nodes;
                    ((ParamValue)values[SpiritParameterNames.BATTLE_ID]).Value = fighter.battle_id;
                    ((ParamValue)values[SpiritParameterNames.STOCK]).Value = fighter.stock;
                    ((ParamValue)values[SpiritParameterNames.HP]).Value = fighter.hp;
                    ((ParamValue)values[SpiritParameterNames.ENTRY_TYPE]).Value = fighter.entry_type;
                    ((ParamValue)values[SpiritParameterNames.FIRST_APPEAR]).Value = fighter.first_appear;
                    ((ParamValue)values[SpiritParameterNames.APPEAR_RULE_TIME]).Value = fighter.appear_rule_time;
                    ((ParamValue)values[SpiritParameterNames.APPEAR_RULE_COUNT]).Value = fighter.appear_rule_count;
                    ((ParamValue)values[SpiritParameterNames.FIGHTER_KIND]).Value = fighter.fighter_kind;
                    ((ParamValue)values[SpiritParameterNames.COLOR]).Value = fighter.color;
                    ((ParamValue)values[SpiritParameterNames.MII_HAT_ID]).Value = fighter.mii_hat_id;
                    ((ParamValue)values[SpiritParameterNames.MII_BODY_ID]).Value = fighter.mii_body_id;
                    ((ParamValue)values[SpiritParameterNames.MII_COLOR]).Value = fighter.mii_color;
                    ((ParamValue)values[SpiritParameterNames.MII_VOICE]).Value = fighter.mii_voice;
                    ((ParamValue)values[SpiritParameterNames.MII_SP_N]).Value = fighter.mii_sp_n;
                    ((ParamValue)values[SpiritParameterNames.MII_SP_S]).Value = fighter.mii_sp_s;
                    ((ParamValue)values[SpiritParameterNames.MII_SP_HI]).Value = fighter.mii_sp_hi;
                    ((ParamValue)values[SpiritParameterNames.MII_SP_LW]).Value = fighter.mii_sp_lw;
                    ((ParamValue)values[SpiritParameterNames.CPU_LV]).Value = fighter.cpu_lv;
                    ((ParamValue)values[SpiritParameterNames.CPU_TYPE]).Value = fighter.cpu_type;
                    ((ParamValue)values[SpiritParameterNames.CPU_SUB_TYPE]).Value = fighter.cpu_sub_type;
                    ((ParamValue)values[SpiritParameterNames.CPU_ITEM_PICK_UP]).Value = fighter.cpu_item_pick_up;
                    ((ParamValue)values[SpiritParameterNames.CORPS]).Value = fighter.corps;
                    ((ParamValue)values[SpiritParameterNames.x0f2077926c]).Value = fighter.x0f2077926c;
                    ((ParamValue)values[SpiritParameterNames.INIT_DAMAGE]).Value = fighter.init_damage;
                    ((ParamValue)values[SpiritParameterNames.SUB_RULE]).Value = fighter.sub_rule;
                    ((ParamValue)values[SpiritParameterNames.SCALE]).Value = fighter.scale;
                    ((ParamValue)values[SpiritParameterNames.FLY_RATE]).Value = fighter.fly_rate;
                    ((ParamValue)values[SpiritParameterNames.INVALID_DROP]).Value = fighter.invalid_drop;
                    ((ParamValue)values[SpiritParameterNames.ENABLE_CHARGE_FINAL]).Value = fighter.enable_charge_final;
                    ((ParamValue)values[SpiritParameterNames.SPIRIT_NAME]).Value = fighter.spirit_name;
                    ((ParamValue)values[SpiritParameterNames.ATTACK]).Value = fighter.attack;
                    ((ParamValue)values[SpiritParameterNames.DEFENSE]).Value = fighter.defense;
                    ((ParamValue)values[SpiritParameterNames.ATTR]).Value = fighter.attr;
                    ((ParamValue)values[SpiritParameterNames.ABILITY1]).Value = fighter.ability_1;
                    ((ParamValue)values[SpiritParameterNames.ABILITY2]).Value = fighter.ability_2;
                    ((ParamValue)values[SpiritParameterNames.ABILITY3]).Value = fighter.ability_3;
                    ((ParamValue)values[SpiritParameterNames.ABILITY_PERSONAL]).Value = fighter.ability_personal;
                }
            }
        }

        #endregion


        #region Spirit Addition

        private bool bAddAsUnsavedChange = false;

        public void AddNewSpirit(object sender, RoutedEventArgs e)
        {
            var Index = GetNextValidSaveIndex();
            string ID = $"spirit{Index}";

            if (!bAddAsUnsavedChange)
            {
                InputDialogue inputDialog = new InputDialogue("Enter Spirit ID:", ID);
                if (inputDialog.ShowDialog() == true)
                {
                    if (SpiritValueDisplay.Spirits.ContainsKey(Hash40Util.StringToHash40(inputDialog.Answer)))
                    {
                        MessageBox.Show($"ID {inputDialog.Answer} already exists.", "Something's up");
                        return;
                    }
                    else
                    {
                        ID = inputDialog.Answer.ToLower().Replace(" ", "_");
                    }
                }
            }

            var spirit = new Spirit();

            spirit.index = Index;
            spirit.version = 7;
            spirit.directory_id = Convert.ToUInt16(Index);
            spirit.save_no = Convert.ToUInt16(Index);
            spirit.rematch = true;

            spirit.name_id = ID;
            spirit.spirit_id = Hash40Util.StringToHash40(spirit.name_id);
            spirit.display_name = $"New Spirit {Index}";

            if (!baseSpirits.ContainsKey(21917189278))
            {
                MessageBox.Show("An important ID is missing. Please check ui_spirits_db.prc.", "Something's up");
                return;
            }

            spirit.battle = baseSpirits[21917189278].battle.Copy();
            spirit.battle.index = -1;
            spirit.battle.battle_id = spirit.spirit_id;
            spirit.battle.x0d6f19abae = true;
            spirit.battle.aw_flap_delay = true;

            spirit.aesthetics = baseSpirits[21917189278].aesthetics.Copy();
            spirit.aesthetics.layout_id = spirit.spirit_id;
            spirit.aesthetics.index = -1;
            
            for(int i=0; i<spirit.battle.fighters.Count; i++)
            {
                spirit.battle.fighters[i].battle_id = spirit.spirit_id;
                spirit.battle.fighters[i].spirit_name = spirit.spirit_id;
            }

            spirit.Custom = true;

            if (bAddAsUnsavedChange)
            {
                unsavedEdits.Add(Index, spirit);
            }
            else
            {
                modList.Add(Index, spirit);
                SpiritValueDisplay.Spirits.Add(spirit.spirit_id,
                    new SpiritValueDisplay.SpiritID(spirit.spirit_id, spirit.display_name));

                // Refresh UI
                var brush = new LinearGradientBrush();
                brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 0.0));
                brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.25));
                brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 0.75));
                brush.GradientStops.Add(new GradientStop(Colors.LightCyan, 1.0));

                SpiritButton newBtn = new SpiritButton();

                newBtn.Content = modList.ContainsKey(spirit.index) ?
                    $"{modList[spirit.index].display_name}" :
                    spirit.display_name;
                newBtn.FontWeight = FontWeights.Bold;
                newBtn.Name = "Button_" + spirit.name_id;
                newBtn.Background = brush;
                newBtn.spirit = spirit;
                newBtn.Click += (s, ev) => {
                    DisplaySpirit(newBtn.spirit);
                };

                spiritButtons.Add(Index, newBtn);
                SpiritList.Children.Add(newBtn);

                if (!saveToSpiritID.ContainsKey(spirit.save_no))
                {
                    saveToSpiritID.Add(spirit.save_no, spirit.spirit_id);
                }

                DisplaySpirit(spirit);
            }
        }

        private List<int> backupIDs = new List<int>();

        private int GetNextValidSaveIndex(bool bFetchValid = false)
        {
            int result = customSpiritsFirstIndex;

            while (modList.ContainsKey(result))
            {
                result++;
            }

            // TESTING. BAD.
            /*if(result >= 2000 && bFetchValid)
            {
                List<int> IDs = new List<int>();
                foreach (var spr in baseSpirits.Values)
                {
                    IDs.Add(spr.save_no);
                }

                foreach(var id in backupIDs)
                {
                    IDs.Add(id);
                }

                for(int i=0; i<customSpiritsFirstIndex; i++)
                {
                    if (!IDs.Contains(i))
                    {
                        return i;
                    }
                }
            }*/

            return result;
        }


        public void DeleteNewSpirit(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show("This will delete " + currentSpirit.display_name + ".\n" +
                "If mods are saved, this Spirit will be lost and cannot be recovered.\nAre you sure?",
                "Hold Up", MessageBoxButton.YesNo);

            if (result.Equals(MessageBoxResult.Yes))
            {
                int deletedIndex = currentSpirit.directory_id;

                // Remove the unsaved edits
                if (unsavedEdits.ContainsKey(currentSpirit.index))
                    unsavedEdits.Remove(currentSpirit.index);

                if (modList.ContainsKey(currentSpirit.index))
                {
                    // Revert all mods
                    modList.Remove(currentSpirit.index);
                }

                if (SpiritValueDisplay.Spirits.ContainsKey(currentSpirit.spirit_id))
                {
                    SpiritValueDisplay.Spirits.Remove(currentSpirit.spirit_id);
                }

                if (WoLSpirits.Contains(currentSpirit.save_no))
                {
                    WoLSpirits.Remove(currentSpirit.save_no);
                }

                var button = spiritButtons[currentSpirit.index];
                SpiritList.Children.Remove(button);
                spiritButtons.Remove(currentSpirit.index);

                foreach (var spirit in modList.Values)
                {
                    var s = GetMods(spirit);

                    if (s.directory_id > deletedIndex)
                    {
                        // Edit the spirit's number and add it to the mod list
                        if (!unsavedEdits.ContainsKey(s.index))
                            unsavedEdits.Add(s.index, s.Copy());

                        var newS = unsavedEdits[s.index];

                        int directory_id = (int)newS.directory_id;
                        directory_id--;
                        newS.directory_id = Convert.ToUInt16(directory_id);

                        spiritButtons[newS.index].FontStyle = FontStyles.Italic;
                    }
                }

                // Refresh the display
                DisplaySpirit(baseSpirits[21917189278]);
            }
        }

        #endregion


        #region World of Light Parameters

        //private List<ulong> NumSpirits = new List<ulong>();
        //private int NumChests;

        private void LoadMapParams()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Resources\Maps\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string[] maps = Directory.GetFiles(path, "*.prc", SearchOption.TopDirectoryOnly);

            WoLSpaces.Add(WoLSpace.SPACE_TYPE_SPIRIT, new List<WoLSpace>());
            WoLSpaces.Add(WoLSpace.SPACE_TYPE_FIGHTER, new List<WoLSpace>());
            WoLSpaces.Add(WoLSpace.SPACE_TYPE_BUILD, new List<WoLSpace>());
            WoLSpaces.Add(WoLSpace.SPACE_TYPE_BOSS, new List<WoLSpace>());
            WoLSpaces.Add(WoLSpace.SPACE_TYPE_CHEST, new List<WoLSpace>());

            for(int i=0; i<maps.Length; i++)
            {
                LoadMap(maps[i]);
            }

            //MessageBox.Show(NumSpirits.Count + " " + NumChests);
        }

        /*private static int GenerateSeed()
        {
            // note the usage of Interlocked, remember that in a shared context we can't just "_seedCount++"
            return (int)((DateTime.Now.Ticks << 4));
        }*/


        private void RegenerateSeed(object sender, EventArgs e)
        {
            int seed = new Random().Next(0, Int32.MaxValue);

            Rand_Seed.Value = (int)seed;
        }
        private void LoadMap(string path)
        {
            // Read the map params
            ParamFile ui_map_db = new ParamFile();
            ui_map_db.Open(path);

            if (!ui_map_db.Root.Nodes.ContainsKey("map_spot_tbl"))
            {
                MessageBox.Show($"Cannot load map data. { path } is not a proper World of Light map.", "Something's up...");
                return;
            }

            var Map = new WoLMap(path);

            var spotDB = ((ParamList)ui_map_db.Root.Nodes["map_spot_tbl"]);

            int count = 0;
            bool bOffset = false;//path.EndsWith("spirits_campaign_map_param_sub_002_woods.prc");

            if (spotDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < spotDB.Nodes.Count; i++)
                {
                    var node = spotDB.Nodes[i];

                    if (bOffset)
                    {
                        var z = Convert.ToInt32(((ParamValue)((ParamStruct)node).Nodes[42992058099]).Value);
                        z += 2;

                        ((ParamValue)((ParamStruct)node).Nodes[42992058099]).Value = z;
                    }

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var space_params = ((ParamStruct)node);

                        var space_type = Convert.ToUInt64(((ParamValue)space_params.Nodes[19543250729]).Value);
                        var spirit_id = Convert.ToUInt64(((ParamValue)space_params.Nodes[26703716223]).Value);
                        var battle_id = Convert.ToUInt64(((ParamValue)space_params.Nodes[42034472729]).Value);

                        switch (space_type)
                        {
                            // Add spirit entry
                            case WoLSpace.SPACE_TYPE_SPIRIT:

                                if (spirit_id != 0 && spirit_id != 9398208554)
                                {
                                    if (baseSpirits.ContainsKey(spirit_id))
                                    {
                                        // Most maps have a dummy space for Karate Joe.
                                        if (spirit_id == 72346765417 &&
                                            !path.Contains("spirits_campaign_map_param_dark"))
                                        {
                                            continue;
                                        }

                                        // Exception for Zangief who is labeled as a spirit
                                        if (baseSpirits[spirit_id].type == 83372998134 && !DevMode)
                                        {
                                            var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                            space.aux_val = 
                                                Convert.ToUInt64(((ParamValue)space_params.Nodes[37233412328]).Value);
                                            WoLSpaces[WoLSpace.SPACE_TYPE_BUILD].Add(space);
                                        }
                                        else
                                        {
                                            var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                            WoLSpaces[WoLSpace.SPACE_TYPE_SPIRIT].Add(space);
                                            count++;

                                            var spr = GetMods(spirit_id);

                                            if (spr != null && !DefWoLData.Contains(spr.save_no))
                                            {
                                                DefWoLData.Add(spr.save_no);
                                            }

                                            /*if (!NumSpirits.Contains(spirit_id))
                                            {
                                                NumSpirits.Add(spirit_id);
                                            }*/
                                        }
                                    }
                                }
                                break;

                            // Add fighter entry
                            case WoLSpace.SPACE_TYPE_FIGHTER:

                                if (spirit_id != 0 && spirit_id != 9398208554)
                                {
                                    var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                    WoLSpaces[WoLSpace.SPACE_TYPE_FIGHTER].Add(space);
                                }
                                break;

                            // Add boss entry
                            case WoLSpace.SPACE_TYPE_BOSS:

                                if (spirit_id != 0 && spirit_id != 9398208554)
                                {
                                    if (baseSpirits.ContainsKey(spirit_id) && 
                                        SpiritValueDisplay.Bosses.Contains(baseSpirits[spirit_id].display_name))
                                    {
                                        // TODO: Copy reward hashes. Clear hash will be a later thing.
                                        var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                        space.aux_val =
                                            Convert.ToUInt64(((ParamValue)space_params.Nodes[17701741854]).Value);
                                        WoLSpaces[WoLSpace.SPACE_TYPE_BOSS].Add(space);
                                    }
                                }
                                break;

                            // Add master entry
                            case WoLSpace.SPACE_TYPE_BUILD:

                                if (spirit_id != 0 && spirit_id != 9398208554)
                                {
                                    if (baseSpirits.ContainsKey(spirit_id))
                                    {
                                        var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                        space.aux_val =
                                            Convert.ToUInt64(((ParamValue)space_params.Nodes[37233412328]).Value);
                                        WoLSpaces[WoLSpace.SPACE_TYPE_BUILD].Add(space);
                                    }
                                }
                                break;

                            // Add chest entry
                            case WoLSpace.SPACE_TYPE_CHEST:

                                if(Convert.ToUInt64(((ParamValue)space_params.Nodes[17701741854]).Value) != 0 &&
                                    !path.Contains("spirits_campaign_map_param_sub_013_dark_marx"))
                                {
                                    var space = new WoLSpace(i, spirit_id, battle_id, space_type, Map);
                                    WoLSpaces[WoLSpace.SPACE_TYPE_CHEST].Add(space);
                                    //NumChests++;
                                }
                                break;
                        }
                    }
                }
            }

            WoLMaps.Add(Map);
        }


        Dictionary<string, List<string>> SpiritReplacements = new Dictionary<string, List<string>>();

        private void RandomizeMaps(object sender, EventArgs e)
        {
            SpiritReplacements.Clear();
            // Check for options that will force a rebuild of the spirit list as well.
            bool bHazardKey = Rand_Add_Hazard_Key.IsChecked.Value;
            bool bDLC = Plant_Owned.IsChecked.Value || Joker_Owned.IsChecked.Value || Hero_Owned.IsChecked.Value ||
                Banjo_Owned.IsChecked.Value || Terry_Owned.IsChecked.Value || Byleth_Owned.IsChecked.Value ||
                MinMin_Owned.IsChecked.Value || Steve_Owned.IsChecked.Value || Sephiroth_Owned.IsChecked.Value ||
                PyraMythra_Owned.IsChecked.Value || Kazuya_Owned.IsChecked.Value || Sora_Owned.IsChecked.Value;
            bool bDataOrg = Rand_DifficultFighters.IsChecked.Value;
            bool bDisableSummons = Rand_DisableSummons.IsChecked.Value;
            bool bBuildSpirits = true;
            ulong HazardKeyID = 0;

            if (bBuildSpirits)
            {
                var result = MessageBox.Show("Spirit mods need to be rebuilt in order to preserve fighter difficulty." +
                    "\nNone of these changes will be permanent, but all unsaved changes will be lost.\n" + 
                    "It is recommended that you save all changes before building.\nAre you sure you wish to proceed?",
                    "Just letting you know", MessageBoxButton.YesNo);

                if(result != MessageBoxResult.Yes)
                {
                    return;
                }

                bBuildSpirits = true;

                unsavedEdits.Clear();

                if (bHazardKey)
                {
                    bAddAsUnsavedChange = true;
                    AddNewSpirit(sender, null);

                    foreach(var key in unsavedEdits.Keys)
                    {
                        unsavedEdits[key].display_name = "Obstacle Bypass Key";
                        unsavedEdits[key].type = 89039743027;
                        unsavedEdits[key].ability_id = 19320013007;
                        unsavedEdits[key].summon_sp = 100;
                        unsavedEdits[key].summon_id1 = 51304578326;
                        unsavedEdits[key].summon_quantity1 = 1;
                        unsavedEdits[key].summon_id2 = 45518778546;
                        unsavedEdits[key].summon_quantity2 = 1;
                        unsavedEdits[key].rematch = false;
                        unsavedEdits[key].is_board_appear = false;
                        unsavedEdits[key].shop_sales_type = 53553909307;

                        HazardKeyID = unsavedEdits[key].spirit_id;
                    }

                    bAddAsUnsavedChange = false;
                }

                if (bDLC)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Campaign\dlc_template.json"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Ignore;

                        using (StreamReader sr = new StreamReader
                            (AppDomain.CurrentDomain.BaseDirectory + @"Resources\Campaign\dlc_template.json"))
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            var dlcTemplate = serializer.Deserialize<Dictionary<int, Spirit>>(reader);

                            foreach(var dlcSpirit in dlcTemplate.Values)
                            {
                                var current = GetMods(dlcSpirit).Copy();

                                var dlcBattle = dlcSpirit.battle.Copy();
                                dlcBattle.index = current.battle.index;

                                for(int i=0; i<dlcBattle.fighterDBIndexes.Count; ++i)
                                {
                                    if(i < current.battle.fighterDBIndexes.Count)
                                    {
                                        dlcBattle.fighterDBIndexes[i] = current.battle.fighterDBIndexes[i];
                                    }
                                }

                                current.battle = dlcBattle;

                                unsavedEdits.Add(current.index, current);
                            }
                        }
                    }
                    else
                    {
                        result = MessageBox.Show("There is no included template file for DLC fights.\n" +
                            "This may cause issues unlocking Hero, Banjo, Sephiroth, Pyra/Mythra, Kazuya, and Sora\n" +
                            "if you have not changed the fighters for these character's Fighter Spirit battles.\n" +
                            "Are you sure you wish to proceed?",
                            "Just letting you know", MessageBoxButton.YesNo);

                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }
                }
            }

            // Get randomization mode
            int Mode = Rand_Type.SelectedIndex;

            // Get a temp data structure of every available spirit for the randomization
            int SpiritCount = WoLSpirits.Count;
            Dictionary<ulong, List<ulong>> AvailableSpirits = 
                new Dictionary<ulong, List<ulong>>(5);
            Dictionary<int, List<ulong>> SpiritsByRarity =
                new Dictionary<int, List<ulong>>(4);
            
            // Fix for an issue caused by the Street Fighter map
            Dictionary<ulong, WoLSpirit> SpiritHashLookup = new Dictionary<ulong, WoLSpirit>();

            // Add all fighters to the Available Spirits list
            foreach (var fighter in WoLSpaces[WoLSpace.SPACE_TYPE_FIGHTER])
            {
                if (!AvailableSpirits.ContainsKey(WoLSpace.SPACE_TYPE_FIGHTER))
                {
                    AvailableSpirits.Add(WoLSpace.SPACE_TYPE_FIGHTER, new List<ulong>());
                }

                if (!AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(fighter.spirit_id))
                {
                    AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(fighter.spirit_id);
                }
            }

            // TODO: add DLC
            #region DLC
            List<ulong> DLC = new List<ulong>(12);
            DLC.Add(58829770439);
            DLC.Add(81263879583);
            DLC.Add(37677046025);
            DLC.Add(21989132121);
            DLC.Add(23228653234);
            DLC.Add(37406502556);
            DLC.Add(59150244689);
            DLC.Add(55459943160);
            DLC.Add(61833929230);
            DLC.Add(62207429187);
            DLC.Add(100388753450);
            DLC.Add(43366633786);

            if (Plant_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(58829770439))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(58829770439);
            if (Joker_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(81263879583))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(81263879583);
            if (Hero_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(37677046025)) 
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(37677046025);
            if (Banjo_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(21989132121))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(21989132121);
            if (Terry_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(23228653234))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(23228653234);
            if (Byleth_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(37406502556))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(37406502556);
            if (MinMin_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(59150244689))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(59150244689);
            if (Steve_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(55459943160))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(55459943160);
            if (Sephiroth_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(61833929230))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(61833929230);
            if (PyraMythra_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(62207429187))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(62207429187);
            if (Kazuya_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(100388753450))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(100388753450);
            if (Sora_Owned.IsChecked.Value && !AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(43366633786))
                AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Add(43366633786);
            #endregion

            ParamFile ui_reward_db = new ParamFile();
            string rewardPath = AppDomain.CurrentDomain.BaseDirectory +
                @"Resources\Campaign\spirits_campaign_item_param.prc";
            ui_reward_db.Open(rewardPath);

            var rewardDB = ((ParamList)ui_reward_db.Root.Nodes["skill_parts_tbl"]);

            if (rewardDB.TypeKey.Equals(ParamType.list))
            {
                for (int i = 0; i < rewardDB.Nodes.Count; i++)
                {
                    var node = rewardDB.Nodes[i];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var obs = ((ParamStruct)node);
                        var battle = (ulong)((ParamValue)obs.Nodes["battle_id"]).Value;

                        if (AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Contains(battle))
                        {
                            ((ParamValue)obs.Nodes["num"]).Value = Rand_OrbAmount.Value.HasValue ? 
                                Rand_OrbAmount.Value : 10;
                        }
                    }
                }

                for(int i=0; i<DLC.Count; i++)
                {
                    var newRewardEntry = new ParamStruct();
                    var ParamItem = new ParamValue(ParamType.@int, 
                        Rand_OrbAmount.Value.HasValue ? Rand_OrbAmount.Value : 10);
                    newRewardEntry.Nodes.Add("num", ParamItem);
                    var ParamBattle = new ParamValue(ParamType.hash40, DLC[i]);
                    newRewardEntry.Nodes.Add("battle_id", ParamBattle);

                    rewardDB.Nodes.Add(newRewardEntry);
                }
            }

            // Add all spirits to the Available Spirits list
            foreach (var spirit in WoLSpirits)
            {
                if (!AvailableSpirits.ContainsKey(WoLSpace.SPACE_TYPE_SPIRIT))
                {
                    AvailableSpirits.Add(WoLSpace.SPACE_TYPE_SPIRIT, new List<ulong>());
                }

                if (!saveToSpiritID.ContainsKey(spirit))
                {
                    continue;
                }

                if (saveToSpiritID.ContainsKey(spirit) && 
                    AvailableSpirits[WoLSpace.SPACE_TYPE_SPIRIT].Contains(saveToSpiritID[spirit]))
                    continue;

                var sprData = GetMods(saveToSpiritID[spirit]);

                if(sprData.type == 86992126058 || sprData.type == 83372998134)
                {
                    continue;
                }

                AvailableSpirits[WoLSpace.SPACE_TYPE_SPIRIT].Add(saveToSpiritID[spirit]);

                // Add the spirit to the rarity list
                if (sprData != null)
                {
                    var StarValue = Spirit.RankToStarValue(sprData.rank);

                    if (!SpiritsByRarity.ContainsKey(StarValue))
                    {
                        SpiritsByRarity.Add(StarValue, new List<ulong>());
                    }

                    SpiritsByRarity[StarValue].Add(sprData.spirit_id);

                    if (bDisableSummons)
                    {
                        var sprCopy = sprData.Copy();
                        sprCopy.summon_id1 = 19320013007;
                        sprCopy.summon_id2 = 19320013007;
                        sprCopy.summon_id3 = 19320013007;
                        sprCopy.summon_id4 = 19320013007;
                        sprCopy.summon_id5 = 19320013007;
                        sprCopy.summon_quantity1 = 0;
                        sprCopy.summon_quantity2 = 0;
                        sprCopy.summon_quantity3 = 0;
                        sprCopy.summon_quantity4 = 0;
                        sprCopy.summon_quantity5 = 0;

                        if (unsavedEdits.ContainsKey(sprCopy.index))
                        {
                            unsavedEdits[sprCopy.index] = sprCopy;
                        }
                        else
                        {
                            unsavedEdits.Add(sprCopy.index, sprCopy);
                        }
                    }
                }
            }

            // Get the output path
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (!Directory.Exists(outputPath + @"param\spirits\campaign\"))
            {
                Directory.CreateDirectory(outputPath + @"param\spirits\campaign\");
            }

            // Check if we should use an existing seed or generate a new one
            int seed = Rand_Use_Seed.IsChecked.HasValue && Rand_Use_Seed.IsChecked.Value && Rand_Seed.Value.HasValue ? 
                Rand_Seed.Value.Value :
                new Random().Next(0, Int32.MaxValue);

            Rand_Seed.Value = (int)seed;
            var Rand = new Random(seed);

            // Initialize map list. This will be used for saving
            Dictionary<string, ParamFile> maps = new Dictionary<string, ParamFile>();

            // Prepare obstacle check
            Dictionary<int, WoLObstacle> ObstacleLookup = new Dictionary<int, WoLObstacle>();

            ParamFile ui_barrier_db = new ParamFile();
            string obstaclePath = AppDomain.CurrentDomain.BaseDirectory + 
                @"Resources\Campaign\spirits_campaign_barrier_param.prc";
            ui_barrier_db.Open(obstaclePath);

            if (!ui_barrier_db.Root.Nodes.ContainsKey("barrier_tbl"))
            {
                MessageBox.Show($"Cannot load obtsacle data. " +
                    $"{ obstaclePath } is not an obstacle prc.", "Something's up...");
                return;
            }
            else
            {
                ObstacleLookup.Add(0, new WoLObstacle());
                ObstacleLookup.Add(12, new WoLObstacle());
                ObstacleLookup.Add(14, new WoLObstacle());
                ObstacleLookup.Add(15, new WoLObstacle());
                ObstacleLookup.Add(16, new WoLObstacle());
                ObstacleLookup.Add(17, new WoLObstacle());
                ObstacleLookup.Add(18, new WoLObstacle());
                ObstacleLookup.Add(19, new WoLObstacle());
                ObstacleLookup.Add(20, new WoLObstacle());
                ObstacleLookup.Add(21, new WoLObstacle());
                ObstacleLookup.Add(22, new WoLObstacle());
                ObstacleLookup.Add(31, new WoLObstacle());
                ObstacleLookup.Add(32, new WoLObstacle());
                ObstacleLookup.Add(33, new WoLObstacle());
                ObstacleLookup.Add(34, new WoLObstacle());
                ObstacleLookup.Add(35, new WoLObstacle());
                ObstacleLookup.Add(36, new WoLObstacle());

                var spotDB = ((ParamList)ui_barrier_db.Root.Nodes["barrier_tbl"]);

                if (spotDB.TypeKey.Equals(ParamType.list))
                {
                    for (int i = 0; i < spotDB.Nodes.Count; i++)
                    {
                        if (ObstacleLookup.ContainsKey(i))
                        {
                            var node = spotDB.Nodes[i];

                            if (node.TypeKey.Equals(ParamType.@struct))
                            {
                                var obs = ((ParamStruct)node);

                                for (int n=0; n<8; n++)
                                {
                                    var spr = Convert.ToUInt64
                                        (((ParamValue)obs.Nodes[WoLObstacle.GetHashFromIndex(n)]).Value);

                                    if (spr != 0)
                                        ObstacleLookup[i].clear_spirits.Add(spr);
                                }

                                if(HazardKeyID != 0)
                                {
                                    ((ParamValue)obs.Nodes[WoLObstacle.GetHashFromIndex(-1)]).Value = HazardKeyID;
                                }
                            }
                        }
                    }
                }
            }


            // Loop through all spaces
            if (SpiritCount < WoLMap.WoLSpiritsMinThreshold)
            {
                MessageBox.Show("There are less available spirits than there are spaces." +
                    "\nNot all spaces will be randomized.", "Just letting you know");
            }

            List<Spirit> FighterBattleOverrides = new List<Spirit>();
            List<WoLSpace> MasterSpiritPool = new List<WoLSpace>();
            List<WoLSpace> BossSpiritPool = new List<WoLSpace>();

            List<ulong> SpaceTypesInUse = new List<ulong>();
            SpaceTypesInUse.Add(WoLSpace.SPACE_TYPE_FIGHTER);
            if (Rand_ShuffleMasters.IsChecked.Value)
                SpaceTypesInUse.Add(WoLSpace.SPACE_TYPE_BUILD);
            if (Rand_Replace_Chest.IsChecked.Value)
                SpaceTypesInUse.Add(WoLSpace.SPACE_TYPE_CHEST);
            SpaceTypesInUse.Add(WoLSpace.SPACE_TYPE_SPIRIT);
            if (Rand_ShuffleBosses.IsChecked.Value)
                SpaceTypesInUse.Add(WoLSpace.SPACE_TYPE_BOSS);

            int bossCount = 3;

            int startAmount = WoLSpirits.Count;

            foreach (var type in SpaceTypesInUse)
            {
                if (!AvailableSpirits.ContainsKey(type))
                {
                    //continue;
                }

                List<WoLSpace> TargetSpaces = new List<WoLSpace>(WoLSpaces[type].Count);

                foreach(var space in WoLSpaces[type])
                {
                    TargetSpaces.Add(space);

                    if(type == WoLSpace.SPACE_TYPE_BUILD)
                    {
                        if(MasterSpiritPool.Find((s) => s.spirit_id == space.spirit_id) == null)
                        {
                            MasterSpiritPool.Add(space);
                        }
                    }

                    if (type == WoLSpace.SPACE_TYPE_BOSS)
                    {
                        if (BossSpiritPool.Find((s) => s.spirit_id == space.spirit_id) == null)
                        {
                            BossSpiritPool.Add(space);
                        }
                    }
                }

                while (TargetSpaces.Count > 0)
                {
                    int index = Rand.Next(0, TargetSpaces.Count);

                    ParamFile map = null;

                    // Open the map if it isn't open yet.
                    if (!maps.ContainsKey(TargetSpaces[index].owningMap.name))
                    {
                        map = new ParamFile();
                        map.Open(TargetSpaces[index].owningMap.name);
                        maps.Add(TargetSpaces[index].owningMap.name, map);

                        /*string result = "Route Index,Route Name,Active Condition,Move Type,Area," +
                            "Start Space Hash,Start Space Name,End Space Hash,End Space Name," +
                            "Arrive Hash A,Decide Hash A,Clear Hash A,Active Condition A,Move Spot A,Spirit A," +
                            "Arrive Hash B,Decide Hash B,Clear Hash B,Active Condition B,Move Spot B,Spirit B,\n";

                        Dictionary<ulong, Hash40Pairs<IParam>> spotData = new Dictionary<ulong, Hash40Pairs<IParam>>();
                        Dictionary<ulong, List<string>> startToRoute = new Dictionary<ulong, List<string>>();
                        Dictionary<ulong, List<ulong>> startToEnd = new Dictionary<ulong, List<ulong>>();

                        if (TargetSpaces[index].owningMap.name.Contains("spirits_campaign_map_param_light.prc"))
                        {
                            var entries = ((ParamList)map.Root.Nodes["map_spot_tbl"]);

                            for (int i = 0; i < entries.Nodes.Count; i++)
                            {
                                var noden = entries.Nodes[i];

                                if (noden.TypeKey.Equals(ParamType.@struct))
                                {
                                    var space = ((ParamStruct)noden).Nodes;
                                    spotData.Add((ulong)((ParamValue)space["hash"]).Value, space);
                                }
                            }

                            var routes = ((ParamList)map.Root.Nodes["map_route_tbl"]);

                            for (int i = 0; i < routes.Nodes.Count; i++)
                            {
                                var noden = routes.Nodes[i];

                                if (noden.TypeKey.Equals(ParamType.@struct))
                                {
                                    var space = ((ParamStruct)noden).Nodes;
                                    ulong startSpace = (ulong)((ParamValue)space["spot_start"]).Value;
                                    ulong endSpace = (ulong)((ParamValue)space["spot_end"]).Value;

                                    var startData = spotData[startSpace];
                                    var endData = spotData[endSpace];

                                    Spirit startSpirit = (ulong)((ParamValue)startData["spirit"]).Value != 0 ? 
                                        GetMods((ulong)((ParamValue)startData["spirit"]).Value) : null;

                                    Spirit endSpirit = (ulong)((ParamValue)endData["spirit"]).Value != 0 ?
                                        GetMods((ulong)((ParamValue)endData["spirit"]).Value) : null;

                                    string startSprName = startSpirit == null ?
                                        "None" : startSpirit.display_name;
                                    string endSprName = endSpirit == null ?
                                        "None" : endSpirit.display_name;

                                    string moveType = "normal";

                                    switch ((ulong)((ParamValue)space["ride"]).Value)
                                    {
                                        case 107247347872:
                                            moveType = "airplane";
                                            break;
                                        case 88720708764:
                                            moveType = "boat";
                                            break;
                                        case 81633030764:
                                            moveType = "bus";
                                            break;
                                        case 91084187784:
                                            moveType = "fzero";
                                            break;
                                        case 90259547112:
                                            moveType = "ocean";
                                            break;
                                        case 108341441979:
                                            moveType = "spaceship";
                                            break;
                                        case 91060885333:
                                            moveType = "train";
                                            break;
                                    }

                                    string line = $"{i}," +
                                        $"{((ParamValue)space["label"]).Value}," +
                                        $"{((ParamValue)space["active_condition_spot_flag"]).Value}," +
                                        $"{moveType}," +
                                        $"{((ParamValue)startData["area"]).Value}," +
                                        $"{startSpace}," +
                                        $"{((ParamValue)startData["label"]).Value}," +
                                        $"{endSpace}," +
                                        $"{((ParamValue)endData["label"]).Value}," +
                                        $"{((ParamValue)startData["act_hash_arrive"]).Value}," +
                                        $"{((ParamValue)startData["act_hash_decide"]).Value}," +
                                        $"{((ParamValue)startData["act_hash_clear"]).Value}," +
                                        $"{((ParamValue)startData["active_condition_spot_01"]).Value}," +
                                        $"{((ParamValue)startData["move_spot"]).Value}," +
                                        $"{startSprName}," +
                                        $"{((ParamValue)endData["act_hash_arrive"]).Value}," +
                                        $"{((ParamValue)endData["act_hash_decide"]).Value}," +
                                        $"{((ParamValue)endData["act_hash_clear"]).Value}," +
                                        $"{((ParamValue)endData["active_condition_spot_01"]).Value}," +
                                        $"{((ParamValue)endData["move_spot"]).Value}," +
                                        $"{endSprName},\n";

                                    if (startToRoute.ContainsKey(startSpace))
                                    {
                                        startToRoute[startSpace].Add(line);
                                        startToEnd[startSpace].Add(endSpace);
                                    }
                                    else
                                    {
                                        startToRoute.Add(startSpace, new List<string> { line });
                                        startToEnd.Add(startSpace, new List<ulong> { endSpace });
                                    }
                                }
                            }

                            List<ulong> printed = new List<ulong>();

                            foreach(var space in startToRoute.Keys)
                            {
                                LightRealmOutputPerRoute(ref result, space, printed, startToRoute, startToEnd);
                            }

                            File.WriteAllText(outputPath + "light_realm.csv", result);
                        }*/
                    }
                    else
                    {
                        map = maps[TargetSpaces[index].owningMap.name];
                    }

                    var spotDB = ((ParamList)map.Root.Nodes["map_spot_tbl"]);

                    if (!spotDB.TypeKey.Equals(ParamType.list))
                    {
                        MessageBox.Show(WoLSpaces[type][index].owningMap.name + " is not a World of Light Map." +
                            "\nRandomization will not execute.");
                        return;
                    }

                    // Get the param values
                    var node = spotDB.Nodes[TargetSpaces[index].index];

                    if (node.TypeKey.Equals(ParamType.@struct))
                    {
                        var space = ((ParamStruct)node).Nodes;
                        var oldSpr = (ulong)((ParamValue)space[26703716223]).Value;
                        ulong newSpr = 0;
                        List<int> obsTypes = new List<int>();

                        bool bObstacle = false;

                        // Check if this is an obstacle spirit and add to the list if so
                        foreach (var obsKey in ObstacleLookup.Keys)
                        {
                            if (ObstacleLookup[obsKey].clear_spirits.Contains(oldSpr))
                            {
                                var objDB = ((ParamList)ui_barrier_db.Root.Nodes["barrier_tbl"]);
                                var objNode = objDB.Nodes[obsKey];
                                var obsStruct = ((ParamStruct)objNode);

                                obsTypes.Add(obsKey);
                                bObstacle = true;
                            }
                        }

                        // Overwrite param values
                        if (SpiritHashLookup.ContainsKey(oldSpr))
                        {
                            newSpr = SpiritHashLookup[(ulong)((ParamValue)space[26703716223]).Value].spirit_id;
                            ((ParamValue)space[19543250729]).Value =
                                SpiritHashLookup[(ulong)((ParamValue)space[26703716223]).Value].type;
                            ((ParamValue)space[42034472729]).Value =
                                SpiritHashLookup[(ulong)((ParamValue)space[26703716223]).Value].battle_id;

                            if(type == WoLSpace.SPACE_TYPE_MASTER || 
                                GetMods(oldSpr).type == 83372998134)
                            {
                                ((ParamValue)space[37233412328]).Value =
                                    SpiritHashLookup[(ulong)((ParamValue)space[26703716223]).Value].aux_val;
                            }

                            ((ParamValue)space[26703716223]).Value = newSpr;
                        }
                        else
                        {
                            ulong spirit = 0;
                            ulong battle = 0;
                            var tType = type;
                            bool bIsChestFighter = false;

                            if((tType == WoLSpace.SPACE_TYPE_CHEST))
                            {
                                bIsChestFighter = true;
                                tType = (AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Count > 0) ?
                                    WoLSpace.SPACE_TYPE_FIGHTER : WoLSpace.SPACE_TYPE_SPIRIT;
                            }

                            if ((tType == WoLSpace.SPACE_TYPE_SPIRIT) && 
                                (AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Count == 0 ||
                                GetMods((ulong)((ParamValue)space[26703716223]).Value).rank != 29098685350))
                            {
                                if(type == WoLSpace.SPACE_TYPE_CHEST)
                                {
                                    ((ParamValue)space[19543250729]).Value = WoLSpace.SPACE_TYPE_SPIRIT;

                                    List<int> Rarities = new List<int>();

                                    if (Rand_Prioritize_Legend.IsChecked.Value &&
                                        SpiritsByRarity.ContainsKey(4) &&
                                        SpiritsByRarity[4].Count > 0)
                                    {
                                        Rarities.Add(4);
                                    }
                                    else
                                    {
                                        foreach (var key in SpiritsByRarity.Keys)
                                        {
                                            if (SpiritsByRarity[key].Count > 0)
                                            {
                                                Rarities.Add(key);
                                            }
                                        }
                                    }

                                    var rarIndex = Rand.Next(0, Rarities.Count);
                                    var smTarget = Rand.Next(0, SpiritsByRarity[Rarities[rarIndex]].Count);
                                    spirit = SpiritsByRarity[Rarities[rarIndex]][smTarget];
                                    newSpr = SpiritsByRarity[Rarities[rarIndex]][smTarget];
                                    AvailableSpirits[tType].Remove(spirit);
                                    SpiritsByRarity[Rarities[rarIndex]].Remove(spirit);
                                }
                                else
                                {
                                    if(Rand_Prioritize_Support.IsChecked.HasValue &&
                                        Rand_Prioritize_Support.IsChecked.Value && bObstacle)
                                    {
                                        var target = Rand.Next(0, AvailableSpirits[tType].Count);
                                        spirit = AvailableSpirits[tType][target];

                                        int pass = 5;

                                        var ModData = GetMods(spirit);

                                        while(ModData.type != 89039743027 && pass > 0)
                                        {
                                            target = Rand.Next(0, AvailableSpirits[tType].Count);
                                            spirit = AvailableSpirits[tType][target];
                                            ModData = GetMods(spirit);

                                            pass--;
                                        }

                                        var spiritData = GetMods(spirit);

                                        newSpr = AvailableSpirits[tType][target];
                                        AvailableSpirits[tType].RemoveAt(target);

                                        if (SpiritsByRarity.ContainsKey(Spirit.RankToStarValue(spiritData.rank)))
                                        {
                                            SpiritsByRarity[Spirit.RankToStarValue(spiritData.rank)].Remove(newSpr);
                                        }

                                        var objDB = ((ParamList)ui_barrier_db.Root.Nodes["barrier_tbl"]);

                                        foreach(var obsType in obsTypes)
                                        {
                                            var objNode = objDB.Nodes[obsType];
                                            var obsStruct = ((ParamStruct)objNode);
                                            ((ParamValue)obsStruct.Nodes[WoLObstacle.GetHashFromIndex(
                                                ObstacleLookup[obsType].clear_spirits.IndexOf(oldSpr))]).Value = newSpr;
                                        }
                                    }
                                    else
                                    {
                                        switch (Mode)
                                        {
                                            case 0:
                                                var target = Rand.Next(0, AvailableSpirits[tType].Count);
                                                spirit = AvailableSpirits[tType][target];
                                                newSpr = AvailableSpirits[tType][target];
                                                AvailableSpirits[tType].RemoveAt(target);
                                                break;

                                            case 1:
                                                int smRank = Spirit.RankToStarValue(GetMods(oldSpr).rank);

                                                if (TargetSpaces[index].owningMap.name.Contains(
                                                    "spirits_campaign_map_param_light_and_dark") &&
                                                    Rand_Final_Legend.IsChecked.HasValue && Rand_Final_Legend.IsChecked.Value)
                                                {
                                                    smRank = 4;
                                                }

                                                if (SpiritsByRarity[smRank].Count > 0)
                                                {
                                                    var smTarget = Rand.Next(0, SpiritsByRarity[smRank].Count);
                                                    spirit = SpiritsByRarity[smRank][smTarget];
                                                    newSpr = SpiritsByRarity[smRank][smTarget];
                                                    AvailableSpirits[tType].Remove(spirit);
                                                    SpiritsByRarity[smRank].Remove(spirit);
                                                }
                                                else
                                                {
                                                    List<int> Rarities = new List<int>();

                                                    foreach (var key in SpiritsByRarity.Keys)
                                                    {
                                                        if (SpiritsByRarity[key].Count > 0)
                                                        {
                                                            Rarities.Add(key);
                                                        }
                                                    }

                                                    var rarIndex = Rand.Next(0, Rarities.Count);
                                                    var smTarget = Rand.Next(0, SpiritsByRarity[Rarities[rarIndex]].Count);
                                                    spirit = SpiritsByRarity[Rarities[rarIndex]][smTarget];
                                                    newSpr = SpiritsByRarity[Rarities[rarIndex]][smTarget];
                                                    AvailableSpirits[tType].Remove(spirit);
                                                    SpiritsByRarity[Rarities[rarIndex]].Remove(spirit);
                                                }

                                                break;

                                            case 2:
                                                int vRank = Spirit.RankToStarValue(GetMods(oldSpr).rank);

                                                if (TargetSpaces[index].owningMap.name.Contains(
                                                    "spirits_campaign_map_param_light_and_dark") &&
                                                    Rand_Final_Legend.IsChecked.HasValue && Rand_Final_Legend.IsChecked.Value)
                                                {
                                                    vRank = 4;
                                                }

                                                List<int> vValidRanks = new List<int>(4);

                                                if (SpiritsByRarity[vRank].Count > 0) vValidRanks.Add(vRank);
                                                if (SpiritsByRarity[Math.Max(1, vRank - 1)].Count > 0)
                                                    vValidRanks.Add(Math.Max(1, vRank - 1));
                                                if (SpiritsByRarity[Math.Min(4, vRank + 1)].Count > 0)
                                                    vValidRanks.Add(Math.Min(4, vRank + 1));

                                                if (vValidRanks.Count == 0)
                                                {
                                                    foreach (var key in SpiritsByRarity.Keys)
                                                    {
                                                        if (SpiritsByRarity[key].Count > 0)
                                                        {
                                                            vValidRanks.Add(key);
                                                        }
                                                    }
                                                }

                                                int udRank = vValidRanks[Rand.Next(0, vValidRanks.Count)];

                                                var udTarget = Rand.Next(0, SpiritsByRarity[udRank].Count);
                                                spirit = SpiritsByRarity[udRank][udTarget];
                                                newSpr = SpiritsByRarity[udRank][udTarget];
                                                AvailableSpirits[tType].Remove(spirit);
                                                SpiritsByRarity[udRank].Remove(spirit);

                                                break;

                                            case 3:
                                                int viRank = Spirit.RankToStarValue(GetMods(oldSpr).rank);

                                                if (TargetSpaces[index].owningMap.name.Contains(
                                                    "spirits_campaign_map_param_light_and_dark") &&
                                                    Rand_Final_Legend.IsChecked.HasValue && Rand_Final_Legend.IsChecked.Value)
                                                {
                                                    viRank = 4;
                                                }

                                                List<int> viValidRanks = new List<int>(4);

                                                if (SpiritsByRarity[viRank].Count > 0) viValidRanks.Add(viRank);

                                                for (int z = viRank - 1; z > 0; z--)
                                                {
                                                    if (SpiritsByRarity[z].Count > 0)
                                                    {
                                                        viValidRanks.Add(z);
                                                        break;
                                                    }
                                                }

                                                if (viValidRanks.Count == 0)
                                                {
                                                    foreach (var key in SpiritsByRarity.Keys)
                                                    {
                                                        if (SpiritsByRarity[key].Count > 0)
                                                        {
                                                            viValidRanks.Add(key);
                                                        }
                                                    }
                                                }

                                                int uiRank = viValidRanks[Rand.Next(0, viValidRanks.Count)];

                                                var uiTarget = Rand.Next(0, SpiritsByRarity[uiRank].Count);
                                                spirit = SpiritsByRarity[uiRank][uiTarget];
                                                newSpr = SpiritsByRarity[uiRank][uiTarget];
                                                AvailableSpirits[tType].Remove(spirit);
                                                SpiritsByRarity[uiRank].Remove(spirit);

                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if(tType == WoLSpace.SPACE_TYPE_FIGHTER)
                                {
                                    if (AvailableSpirits[WoLSpace.SPACE_TYPE_FIGHTER].Count > 0 && type != WoLSpace.SPACE_TYPE_FIGHTER)
                                    {
                                        tType = WoLSpace.SPACE_TYPE_FIGHTER;
                                        ((ParamValue)space[19543250729]).Value = tType;
                                    }

                                    var target = Rand.Next(0, AvailableSpirits[tType].Count);

                                    spirit = AvailableSpirits[tType][target];
                                    newSpr = AvailableSpirits[tType][target];
                                    AvailableSpirits[tType].RemoveAt(target);

                                    if (bDataOrg)
                                    {
                                        var fighterMods = GetMods(newSpr).Copy();
                                        //fighterMods.battle.battle_power = 15000;
                                        fighterMods.battle.item_level = 19320013007;

                                        foreach (var fighter in fighterMods.battle.fighters)
                                        {
                                            fighter.cpu_lv = 99;
                                            //fighter.attack = 9500;
                                            //fighter.attack = 9000;
                                            fighter.fly_rate = 1.0f;
                                        }

                                        if (unsavedEdits.ContainsKey(fighterMods.index))
                                        {
                                            unsavedEdits[fighterMods.index] = fighterMods;
                                        }
                                        else
                                        {
                                            unsavedEdits.Add(fighterMods.index, fighterMods);
                                        }
                                    }
                                    else
                                    {
                                        var targetBattle = GetMods(newSpr).Copy();

                                        if (!bIsChestFighter)
                                        {
                                            var orgBattle = GetMods(oldSpr).Copy();

                                            targetBattle.battle.battle_power = orgBattle.battle.battle_power;

                                            for (int f = 0; f < orgBattle.battle.fighters.Count; f++)
                                            {
                                                targetBattle.battle.fighters[f].cpu_lv = orgBattle.battle.fighters[f].cpu_lv;
                                                targetBattle.battle.fighters[f].attack = orgBattle.battle.fighters[f].attack;
                                                targetBattle.battle.fighters[f].defense = orgBattle.battle.fighters[f].defense;
                                                targetBattle.battle.fighters[f].fly_rate = orgBattle.battle.fighters[f].fly_rate;
                                            }
                                        }
                                        else
                                        {
                                            targetBattle.battle.battle_power = 13500;

                                            for (int f = 0; f < targetBattle.battle.fighters.Count; f++)
                                            {
                                                targetBattle.battle.fighters[f].cpu_lv = 91;
                                                targetBattle.battle.fighters[f].attack = 9250;
                                                targetBattle.battle.fighters[f].defense = 8750;
                                                targetBattle.battle.fighters[f].fly_rate = 1;
                                            }
                                        }

                                        FighterBattleOverrides.Add(targetBattle);
                                    }
                                }

                                else if (tType == WoLSpace.SPACE_TYPE_BUILD)
                                {
                                    var target = Rand.Next(0, MasterSpiritPool.Count);

                                    spirit = MasterSpiritPool[target].spirit_id;
                                    battle = MasterSpiritPool[target].battle_id;

                                    ((ParamValue)space[37233412328]).Value = MasterSpiritPool[target].aux_val;
                                    MasterSpiritPool.RemoveAt(target);
                                }

                                else if(tType == WoLSpace.SPACE_TYPE_BOSS)
                                {
                                    var target = Rand.Next(0, BossSpiritPool.Count);

                                    spirit = BossSpiritPool[target].spirit_id;
                                    battle = BossSpiritPool[target].battle_id;

                                    ((ParamValue)space[17701741854]).Value = BossSpiritPool[target].aux_val;

                                    if(Rand_Boss_Kill.IsChecked.HasValue && Rand_Boss_Kill.IsChecked.Value)
                                    {
                                        ((ParamValue)space[62054714314]).Value = bossCount > 0 ? 
                                            (ulong)106257450093 : (ulong)0;

                                        bossCount--;
                                    }

                                    BossSpiritPool.RemoveAt(target);
                                }
                            }

                            if (!(type == WoLSpace.SPACE_TYPE_BOSS || type == WoLSpace.SPACE_TYPE_BUILD))
                            {
                                battle = spirit;
                            }

                            if (type != WoLSpace.SPACE_TYPE_CHEST)
                            {
                                var WSpirit = new WoLSpirit(spirit, battle, tType);
                                WSpirit.aux_val = Convert.ToUInt64(((ParamValue)space[37233412328]).Value);
                                SpiritHashLookup.Add((ulong)((ParamValue)space[26703716223]).Value, WSpirit);
                            }

                            if (!Rand_Retain_Chest_Items.IsChecked.Value && type == WoLSpace.SPACE_TYPE_CHEST)
                            {
                                ((ParamValue)space[17701741854]).Value = ((ulong)0);
                            }

                            // TODO: apply filter for short mode
                            if (!SpiritReplacements.ContainsKey(TargetSpaces[index].owningMap.name))
                            {
                                SpiritReplacements.Add(TargetSpaces[index].owningMap.name, new List<string>());
                            }

                            string oldRes = GetMods((ulong)((ParamValue)space[26703716223]).Value).display_name;

                            if (string.IsNullOrEmpty(oldRes))
                            {
                                oldRes = "Chest";
                            }

                            string newRes = GetMods(spirit).display_name + "(" +
                                GetMods(spirit).directory_id + ")";

                            SpiritReplacements[TargetSpaces[index].owningMap.name].Add($"{oldRes}\t->\t{newRes}");

                            ((ParamValue)space[26703716223]).Value = Rand_Debug.IsChecked.HasValue &&
                                Rand_Debug.IsChecked.Value == true ? 9398208554 : spirit;
                            ((ParamValue)space[42034472729]).Value = Rand_Debug.IsChecked.HasValue &&
                                Rand_Debug.IsChecked.Value == true ? 9398208554 : battle;
                        }

                        // TODO: Properly remove this space
                        if(type == WoLSpace.SPACE_TYPE_SPIRIT && !bObstacle)
                        {
                            var spr = GetMods((ulong)((ParamValue)space[26703716223]).Value);
                            int rank = Spirit.RankToStarValue(spr.rank);

                            bool rankInvalid = 4 - 1 < Rand_RankMin.SelectedIndex;

                            if (rankInvalid)
                            {
                                ((ParamValue)space[19543250729]).Value = ((ulong)64623784654);
                                ((ParamValue)space[26703716223]).Value = ((ulong)0);
                                ((ParamValue)space[42034472729]).Value = ((ulong)9398208554);

                                // Check before removing this.
                                // hash_clear: 62054714314
                                if (Convert.ToUInt64(((ParamValue)space[62054714314]).Value) == 0)
                                {
                                    ((ParamValue)space[37233412328]).Value = ((ulong)0);
                                }
                            }
                        }

                        // Remove spirits if they don't match

                        // Removes items, but I'm testing something
                        //((ParamValue)space[17701741854]).Value = (ulong)0;
                    }

                    TargetSpaces.RemoveAt(index);

                    if(AvailableSpirits.ContainsKey(type) && AvailableSpirits[type].Count == 0)
                    {
                        foreach (var space in TargetSpaces)
                        {
                            if (SpiritReplacements.ContainsKey(space.owningMap.name))
                            {
                                var spaceID = GetMods(space.spirit_id).display_name;

                                if (string.IsNullOrEmpty(spaceID))
                                {
                                    spaceID = "Chest";
                                }

                                SpiritReplacements[space.owningMap.name].Add($"{spaceID}\t->\tUNCHANGED");
                            }
                        }

                        break;
                    }
                }
            }

            int remainingSpirits = 0;

            foreach(var key in AvailableSpirits.Keys)
            {
                remainingSpirits += AvailableSpirits[key].Count;
            }

            if (DevMode)
            {
                SpiritDataOutput(startAmount, remainingSpirits);
            }

            // Remove auto DLC
            if (Rand_AutoDLC.IsChecked.Value)
            {
                ParamFile ui_common_db = new ParamFile();
                string commonPath = AppDomain.CurrentDomain.BaseDirectory +
                    @"Resources\Campaign\spirits_campaign_common_param.prc";
                ui_common_db.Open(commonPath);

                if (ui_common_db.Root.Nodes.ContainsKey("unlock_dlc_fighter_count"))
                {
                    var param = ui_common_db.Root.Nodes["unlock_dlc_fighter_count"];
                    var value = ((ParamValue)param);
                    value.Value = ((uint)1000);
                }

                ui_common_db.Save(outputPath + @"param\spirits\campaign\spirits_campaign_common_param.prc");
            }


            if (bBuildSpirits)
            {
                foreach(var spirit in FighterBattleOverrides)
                {
                    if (unsavedEdits.ContainsKey(spirit.index))
                    {
                        unsavedEdits[spirit.index] = spirit;
                    }
                    else
                    {
                        unsavedEdits.Add(spirit.index, spirit);
                    }
                }

                bShowLogs = false;
                Spirit_Save_Param_Click(sender, null);
                bShowLogs = true;

                unsavedEdits.Clear();
            }

            Dictionary<string, string> ParamLabels = new Dictionary<string, string>();

            if (File.Exists(System.IO.Path.Combine(outputPath, "ParamLabels.csv")))
            {
                string[] text = File.ReadAllLines(System.IO.Path.Combine(outputPath, "ParamLabels.csv"));

                foreach (string line in text)
                {
                    var param = line.Split(',');

                    if(param.Length >= 2)
                    {
                        ParamLabels.Add(param[0], param[1]);
                    }
                }
            }

            string output = "";

            foreach (var key in maps.Keys)
            {
                if (Rand_Quiz.IsChecked.HasValue && Rand_Quiz.IsChecked.Value &&
                    System.IO.Path.GetFileName(key).Contains("spirits_campaign_map_param_sub_013_dark_marx"))
                {
                    RandomizeQuiz(((ParamList)maps[key].Root.Nodes["map_spot_tbl"]), Rand);
                }

                maps[key].Save(outputPath + @"param\spirits\campaign\" + System.IO.Path.GetFileName(key));
            }

            ui_barrier_db.Save(outputPath + @"param\spirits\campaign\spirits_campaign_barrier_param.prc");
            ui_reward_db.Save(outputPath + @"param\spirits\campaign\spirits_campaign_item_param.prc");

            #region Quiz
            // Quiz MSBT Polish :)
            if (File.Exists(outputPath + @"ui\message\msg_campaign.msbt"))
                File.Delete(outputPath + @"ui\message\msg_campaign.msbt");

            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Campaign\msg_campaign.MSBT",
                outputPath + @"ui\message\msg_campaign.msbt");

            var campaignMSBT = new MSBT(outputPath + @"ui\message\msg_campaign.msbt");

            for (int i = 0; i <= 10; i++)
            {
                var quizName = campaignMSBT.LBL1.Labels.Find
                    ((lbl) => lbl.ToString().Equals(i == 10 ? 
                    "cam_map_quiz_dark_marx_" + i : "cam_map_quiz_dark_marx_0" + i));

                if (quizName != null)
                {
                    var entry = ((MsbtEditor.Label)quizName).String;
                    string value = BytesToDisplayName(entry.Value);

                    if (value.Contains("Kirby"))
                    {
                        entry.Value = NameToBytes("Two spirits block the path...", false);
                    }
                    else
                    {
                        entry.Value = NameToBytes("One spirit blocks the path...", false);
                    }
                }
            }

            campaignMSBT.Save();

            #endregion

            MessageBox.Show("Randomized maps exported to /ArcOutput/Maps.\n\n" +
                $"Your seed is { seed }.", "It's done.");
        }


        public void RandomizeQuiz(ParamList map, System.Random seed)
        {
            Dictionary<ulong, List<int>> quizToSpaces = new Dictionary<ulong, List<int>>();
            Dictionary<ulong, List<ulong>> quizClearHash = new Dictionary<ulong, List<ulong>>();
            Dictionary<ulong, ulong> quizLoseHash = new Dictionary<ulong, ulong>();
            Dictionary<ulong, ulong> quizChainHash = new Dictionary<ulong, ulong>();

            List<ulong> quizHashes = new List<ulong>
        {
            58464191239, // quiz_01
            58742593946, // quiz_02
            56021471790, // quiz_03
            59299392672, // quiz_04
            57595389716, // quiz_05
            57329581449, // quiz_06
            60119905853, // quiz_07
            55987963604, // quiz_08
            58776063328, // quiz_09
            59968178221, // quiz_10
            57178243993, // quiz_11
        };

            List<ulong> quizFailHashes = new List<ulong>
        {
            80289578692, // quiz_01
            80635456564, // quiz_02
            78879793243, // quiz_03
            77997459924, // quiz_04
            80306734523, // quiz_05
            80618280779, // quiz_06
            78896700196, // quiz_07
            78048166997, // quiz_08
            80391030842, // quiz_09
            81462962990, // quiz_10
            79124981569, // quiz_11
        };

            for (int i = 0; i < map.Nodes.Count; i++)
            {
                var noden = map.Nodes[i];

                if (noden.TypeKey.Equals(ParamType.@struct))
                {
                    var space = ((ParamStruct)noden).Nodes;

                    var spiritHash = (ulong)((ParamValue)space["spirit"]).Value;
                    var quizID = (ulong)((ParamValue)space["active_condition_spot_01"]).Value;
                    var clearHash = (ulong)((ParamValue)space["act_hash_clear"]).Value;
                    var chainHash = (ulong)((ParamValue)space["chain_clear_spot_01"]).Value;

                    if (spiritHash != 0 && quizHashes.Contains(quizID))
                    {
                        if (!quizToSpaces.ContainsKey(quizID))
                        {
                            quizToSpaces.Add(quizID, new List<int>());
                        }

                        quizToSpaces[quizID].Add(i);

                        if (quizFailHashes.Contains(chainHash))
                        {
                            if (!quizLoseHash.ContainsKey(quizID))
                                quizLoseHash.Add(quizID, clearHash);
                            if (!quizChainHash.ContainsKey(quizID))
                                quizChainHash.Add(quizID, chainHash);
                        }
                        else
                        {
                            if (!quizClearHash.ContainsKey(quizID))
                            {
                                quizClearHash.Add(quizID, new List<ulong>());
                            }

                            quizClearHash[quizID].Add(clearHash);
                        }
                    }

                }
            }

            foreach (var quiz in quizToSpaces.Keys)
            {
                List<int> correctAnswers = new List<int>();

                for (int i = 0; i < quizClearHash[quiz].Count; i++)
                {
                    int ind = seed.Next(0, quizToSpaces[quiz].Count);

                    while (correctAnswers.Contains(ind))
                    {
                        ind = seed.Next(0, quizToSpaces[quiz].Count);
                    }

                    correctAnswers.Add(ind);
                }

                for (int i = 0; i < quizToSpaces[quiz].Count; i++)
                {
                    var noden = map.Nodes[quizToSpaces[quiz][i]];
                    var space = ((ParamStruct)noden).Nodes;

                    if (correctAnswers.Contains(i))
                    {
                        ((ParamValue)space["chain_clear_spot_01"]).Value = (ulong)0;
                        ((ParamValue)space["act_hash_clear"]).Value =
                            quizClearHash[quiz][0];

                        quizClearHash[quiz].RemoveAt(0);
                        correctAnswers.Remove(i);
                    }
                    else
                    {
                        ((ParamValue)space["act_hash_clear"]).Value =
                            quizLoseHash[quiz];
                        ((ParamValue)space["chain_clear_spot_01"]).Value =
                            quizChainHash[quiz];
                    }
                }
            }
        }


        void LightRealmOutputPerRoute(ref string result, ulong startSpace, List<ulong> printed, 
            Dictionary<ulong, List<string>> startToRoute, Dictionary<ulong, List<ulong>> startToEnd)
        {
            if (printed.Contains(startSpace)) return;

            printed.Add(startSpace);

            if (!startToRoute.ContainsKey(startSpace)) return;

            for(int i=0; i < startToRoute[startSpace].Count; i++)
            {
                result += startToRoute[startSpace][i];

                LightRealmOutputPerRoute(ref result, startToEnd[startSpace][i], printed, startToRoute, startToEnd);
            }
        }


        void SpiritDataOutput(int start, int end)
        {
            string result = $"Initial Spirit Count: {start} | Remaining Spirits At End: {end}\n";

            foreach(var key in SpiritReplacements.Keys)
            {
                result += $"{key}:\n";

                foreach(var value in SpiritReplacements[key])
                {
                    result += $"\t{value}\n";
                }
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\wol.txt", result);
        }

        #endregion


        #region DLC Board


        public class DLCSpiritData
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public DLCSpiritData(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }


        public void ImportDLCBoards()
        {
            // Get the directory
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";

            if (!File.Exists(path + "spirits_board_special_param.prc"))
            {
                MessageBox.Show("Cannot boot. DLC Spirit Board param is missing.", "Something's up...");
                bootFail = true;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            Dictionary<int, ListBox> boardMap = new Dictionary<int, ListBox>();
            boardMap.Add(0, FP1_Spirits);
            boardMap.Add(1, FP2_Spirits);
            boardMap.Add(2, FP3_Spirits);
            boardMap.Add(3, FP4_Spirits);
            boardMap.Add(4, FP5_Spirits);
            boardMap.Add(5, FP6_Spirits);
            boardMap.Add(6, FP7_Spirits);
            boardMap.Add(7, FP8_Spirits);
            boardMap.Add(8, FP9_Spirits);
            boardMap.Add(9, FP10_Spirits);
            boardMap.Add(10, FP11_Spirits);

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Data\boards.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                List<DLCSpiritBoard> BoardList = new List<DLCSpiritBoard>();

                // Get the directory
                using (StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"Data\boards.json"))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    BoardList = serializer.Deserialize<List<DLCSpiritBoard>>(reader);

                    for(int i=0; i<BoardList.Count; ++i)
                    {
                        boardMap[i].SelectedValuePath = "id";
                        boardMap[i].DisplayMemberPath = "name";

                        foreach (var id in BoardList[i].IncludedSpirits)
                        {
                            var name = GetMods(id).display_name;

                            boardMap[i].Items.Add(new DLCSpiritData(id, name));
                        }
                    }
                }
            }
            else
            {
                ParamFile ui_series_db = new ParamFile();
                ui_series_db.Open(path + "spirits_board_special_param.prc");

                var dlcDB = ((ParamList)ui_series_db.Root.Nodes[98800501348]);

                for (int i = 0; i < 11; ++i)
                {
                    var data = (ParamStruct)dlcDB.Nodes[i];

                    var list = (ParamList)data.Nodes[50296625259];

                    boardMap[i].SelectedValuePath = "id";
                    boardMap[i].DisplayMemberPath = "name";

                    foreach (var val in list.Nodes)
                    {
                        var id = Convert.ToUInt64(((ParamValue)((ParamStruct)val).Nodes[39506993415]).Value);
                        var name = GetMods(id).display_name;

                        boardMap[i].Items.Add(new DLCSpiritData(id, name));
                    }
                }
            }

            AddApplicationControlDLC();
        }


        private void AddApplicationControlDLC()
        {
            FP1_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP1_Spirits);
            };

            FP2_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP2_Spirits);
            };

            FP3_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP3_Spirits);
            };

            FP4_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP4_Spirits);
            };

            FP5_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP5_Spirits);
            };

            FP6_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP6_Spirits);
            };

            FP7_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP7_Spirits);
            };

            FP8_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP8_Spirits);
            };

            FP9_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP9_Spirits);
            };

            FP10_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP10_Spirits);
            };

            FP11_Remove.Click += (s, e) => {
                RemoveSpiritFromList(FP11_Spirits);
            };


            FP1_Add.Click += (s, e) => {
                AddSpiritToList(FP1_Spirits);
            };

            FP2_Add.Click += (s, e) => {
                AddSpiritToList(FP2_Spirits);
            };

            FP3_Add.Click += (s, e) => {
                AddSpiritToList(FP3_Spirits);
            };

            FP4_Add.Click += (s, e) => {
                AddSpiritToList(FP4_Spirits);
            };

            FP5_Add.Click += (s, e) => {
                AddSpiritToList(FP5_Spirits);
            };

            FP6_Add.Click += (s, e) => {
                AddSpiritToList(FP6_Spirits);
            };

            FP7_Add.Click += (s, e) => {
                AddSpiritToList(FP7_Spirits);
            };

            FP8_Add.Click += (s, e) => {
                AddSpiritToList(FP8_Spirits);
            };

            FP9_Add.Click += (s, e) => {
                AddSpiritToList(FP9_Spirits);
            };

            FP10_Add.Click += (s, e) => {
                AddSpiritToList(FP10_Spirits);
            };

            FP11_Add.Click += (s, e) => {
                AddSpiritToList(FP11_Spirits);
            };
        }


        private void AddSpiritToList(ListBox List)
        {
            DropdownPopup inputDialog = new DropdownPopup(SpiritValueDisplay.Spirits, "Select a Spirit:");
            inputDialog.dropdown.SelectedValuePath = "id";
            inputDialog.dropdown.DisplayMemberPath = "name";

            if (inputDialog.ShowDialog() == true)
            {
                var data = ((SpiritValueDisplay.SpiritID)inputDialog.dropdown.Items[inputDialog.dropdown.SelectedIndex]);

                if (data.id == 19320013007 || data.id == 44827135837 || data.id == 44095001115 ||
                    data.id == 43877750031 || data.id == 50718054494 || data.id == 45518778546 || data.id == 8403017505)
                {
                    MessageBox.Show($"Invalid spirit. Please select a real spirit.", "Something's up");
                    return;
                }
                else
                {
                    var id = data.id;
                    var name = data.name;

                    List.Items.Add(new DLCSpiritData(id, name));
                }
            }
        }


        private void RemoveSpiritFromList(ListBox List)
        {
            if(List.Items.Count == 0)
            {
                MessageBox.Show("There are no spirits on this board.", "Something's up...");
                return;
            }

            if (List.SelectedItem != null)
            {
                List.Items.RemoveAt(List.SelectedIndex);
            }
            else
            {
                List.Items.RemoveAt(List.Items.Count - 1);
            }
        }


        private void BuildDLCSpirits()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + @"Resources\";
            string outputPath = AppDomain.CurrentDomain.BaseDirectory + @"ArcOutput\";

            // Load dlc params
            ParamFile ui_dlc_db = new ParamFile();
            ui_dlc_db.Open(basePath + "spirits_board_special_param.prc");

            if (!Directory.Exists(outputPath + @"\ui\param_patch\spirits_board_special\"))
                Directory.CreateDirectory(outputPath + @"\ui\param_patch\spirits_board_special\");

            var dlcDB = ((ParamList)ui_dlc_db.Root.Nodes[98800501348]);

            Dictionary<int, ListBox> boardMap = new Dictionary<int, ListBox>();
            boardMap.Add(0, FP1_Spirits);
            boardMap.Add(1, FP2_Spirits);
            boardMap.Add(2, FP3_Spirits);
            boardMap.Add(3, FP4_Spirits);
            boardMap.Add(4, FP5_Spirits);
            boardMap.Add(5, FP6_Spirits);
            boardMap.Add(6, FP7_Spirits);
            boardMap.Add(7, FP8_Spirits);
            boardMap.Add(8, FP9_Spirits);
            boardMap.Add(9, FP10_Spirits);
            boardMap.Add(10, FP11_Spirits);

            List<ulong> CustomSpirits = new List<ulong>();
            foreach (var spirit in modList.Values)
            {
                if (spirit.Custom)
                {
                    CustomSpirits.Add(spirit.spirit_id);
                }
            }

            for (int i = 0; i < 11; ++i)
            {
                var data = (ParamStruct)dlcDB.Nodes[i];

                var list = (ParamList)data.Nodes[50296625259];
                list.Nodes.Clear();

                for(int n=0; n<boardMap[i].Items.Count; ++n)
                {
                    var spirit = ((DLCSpiritData)boardMap[i].Items[n]);

                    var entry = new ParamValue(ParamType.hash40, spirit.id);
                    var param = new ParamStruct(1);
                    param.Nodes.Add(39506993415, entry);

                    list.Nodes.Add(param);
                }

                if (bDebugBoard && CustomSpirits.Count > 0)
                {
                    while(list.Nodes.Count < 72 && CustomSpirits.Count > 0)
                    {
                        var entry = new ParamValue(ParamType.hash40, CustomSpirits[0]);
                        var param = new ParamStruct(1);
                        param.Nodes.Add(39506993415, entry);

                        list.Nodes.Add(param);
                        CustomSpirits.RemoveAt(0);
                    }
                }
            }

            ui_dlc_db.Save(outputPath +
                @"\ui\param_patch\spirits_board_special\spirits_board_special_param.prc");
        }

        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            if (unsavedEdits.Count > 0)
            {
                var result = MessageBox.Show("All unsaved edits will be lost. Are you sure you wish to quit?", 
                    "Hold Up", MessageBoxButton.YesNo);

                if (!result.Equals(MessageBoxResult.Yes))
                {
                    e.Cancel = true;
                    return;
                }
            }

            var RandoSave = new WoLRandomizerSaveData();
            RandoSave.bUseSeed = Rand_Use_Seed.IsChecked.Value;
            RandoSave.Seed = Rand_Seed.Value.HasValue ? Rand_Seed.Value.Value : 0;
            RandoSave.bReplaceChests = Rand_Replace_Chest.IsChecked.Value;
            RandoSave.bRandomizeBosses = Rand_ShuffleBosses.IsChecked.Value;
            RandoSave.bRandomizeMasters = Rand_ShuffleMasters.IsChecked.Value;
            RandoSave.bHazardKey = Rand_Add_Hazard_Key.IsChecked.Value;
            RandoSave.bDisableSummons = Rand_DisableSummons.IsChecked.Value;
            RandoSave.bDataOrg = Rand_DifficultFighters.IsChecked.Value;
            RandoSave.bDLCAuto = Rand_AutoDLC.IsChecked.Value;
            RandoSave.bLegendFinal = Rand_Final_Legend.IsChecked.Value;
            RandoSave.bRandomQuiz = Rand_Quiz.IsChecked.Value;
            RandoSave.bRandomBoss = Rand_Boss_Kill.IsChecked.Value;
            RandoSave.Mode = Rand_Type.SelectedIndex;
            RandoSave.bSupport = Rand_Prioritize_Support.IsChecked.Value;
            RandoSave.bLegend = Rand_Prioritize_Legend.IsChecked.Value;
            RandoSave.bRetainChest = Rand_Retain_Chest_Items.IsChecked.Value;
            RandoSave.RankLimit = Rand_RankMin.SelectedIndex;
            RandoSave.SphereCount = Rand_OrbAmount.Value.HasValue ? Rand_OrbAmount.Value.Value : 10;

            RandoSave.bPlant = Plant_Owned.IsChecked.Value;
            RandoSave.bJoker = Joker_Owned.IsChecked.Value;
            RandoSave.bHero = Hero_Owned.IsChecked.Value;
            RandoSave.bBanjo = Banjo_Owned.IsChecked.Value;
            RandoSave.bTerry = Terry_Owned.IsChecked.Value;
            RandoSave.bByleth = Byleth_Owned.IsChecked.Value;
            RandoSave.bMinMin = MinMin_Owned.IsChecked.Value;
            RandoSave.bSteve = Steve_Owned.IsChecked.Value;
            RandoSave.bSephiroth = Sephiroth_Owned.IsChecked.Value;
            RandoSave.bPyra = PyraMythra_Owned.IsChecked.Value;
            RandoSave.bKazuya = Kazuya_Owned.IsChecked.Value;
            RandoSave.bSora = Sora_Owned.IsChecked.Value;

            // Save a JSON representation of the WoL Data
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            // Get the directory
            string path = AppDomain.CurrentDomain.BaseDirectory + "Data";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Export to JSON
            using (StreamWriter sw = new StreamWriter(path + @"/randomizer.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, RandoSave);
            }


            Dictionary<int, ListBox> boardMap = new Dictionary<int, ListBox>();
            boardMap.Add(0, FP1_Spirits);
            boardMap.Add(1, FP2_Spirits);
            boardMap.Add(2, FP3_Spirits);
            boardMap.Add(3, FP4_Spirits);
            boardMap.Add(4, FP5_Spirits);
            boardMap.Add(5, FP6_Spirits);
            boardMap.Add(6, FP7_Spirits);
            boardMap.Add(7, FP8_Spirits);
            boardMap.Add(8, FP9_Spirits);
            boardMap.Add(9, FP10_Spirits);
            boardMap.Add(10, FP11_Spirits);

            List<DLCSpiritBoard> Boards = new List<DLCSpiritBoard>();

            for(int i=0; i<11; ++i)
            {
                var Board = new DLCSpiritBoard();

                for (int n = 0; n < boardMap[i].Items.Count; ++n)
                {
                    var spirit = ((DLCSpiritData)boardMap[i].Items[n]);
                    Board.IncludedSpirits.Add(spirit.id);
                }

                Boards.Add(Board);
            }

            using (StreamWriter sw = new StreamWriter(path + @"/boards.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Boards);
            }

            CurrentPreview = null;
            Spirit_Preview_2.Source = null;

            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\corrected.png"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\corrected.png");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\spirits_2.png"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\spirits_2.png");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\webp_convert.png"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\webp_convert.png");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Temp\resized.png"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Temp\resized.png");

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }


    public partial class SpiritButton : Button
    {
        public Spirit spirit;
    }


    /// <summary>
    /// Visual representation of spirit data.
    /// ulongs are the actual values that govern how a spirit works
    /// names are the end-user representation of those values
    /// </summary>
    public static class SpiritValueDisplay
    {
        #region Generic
        public class SpiritType
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public SpiritType(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        public static Dictionary<ulong, SpiritType> SpiritTypes = new Dictionary<ulong, SpiritType>(4)
        {
            { 81665422107, new SpiritType(81665422107, "Primary") },
            { 89039743027, new SpiritType(89039743027, "Support") },
            { 86992126058, new SpiritType(86992126058, "Fighter") },
            { 83372998134, new SpiritType(83372998134, "Master") }
        };


        public class SpiritRank
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string imageName { get; set; }

            public SpiritRank(ulong id, string name, string imageName)
            {
                this.id = id;
                this.name = name;
                this.imageName = "/Icons/" + imageName + ".png";
            }
        }

        public static Dictionary<ulong, SpiritRank> SpiritRanks = new Dictionary<ulong, SpiritRank>(4)
        {
            { 26332195044, new SpiritRank(26332195044, "Novice", "SpiritHeaderNovice") },
            { 19574448332, new SpiritRank(19574448332, "Advanced", "SpiritHeaderAdvanced") },
            { 16196097462, new SpiritRank(16196097462, "Ace", "SpiritHeaderAce") },
            { 29098685350, new SpiritRank(29098685350, "Legend", "SpiritHeaderLegend") }
        };


        public class SpiritAttribute
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string imageName { get; set; }
            public byte bgR { get; set; }
            public byte bgG { get; set; }
            public byte bgB { get; set; }

            public SpiritAttribute(ulong id, string name, string imageName, byte bgR, byte bgG, byte bgB)
            {
                this.id = id;
                this.name = name;
                this.imageName = "/Icons/" + imageName + ".png";
                this.bgR = bgR;
                this.bgG = bgG;
                this.bgB = bgB;
            }
        }

        public static Dictionary<ulong, SpiritAttribute> SpiritAttributes = new Dictionary<ulong, SpiritAttribute>(4)
        {
            { 72803744452, new SpiritAttribute(72803744452, "Attack", "SpiritTypeAttack", 0xEB, 0x8D, 0x8D) },
            { 72357209986, new SpiritAttribute(72357209986, "Shield", "SpiritTypeShield", 0x94, 0xD5, 0xF7) },
            { 71601381526, new SpiritAttribute(71601381526, "Grab", "SpiritTypeGrab", 0x70, 0xBA, 0x70) },
            { 74193544331, new SpiritAttribute(74193544331, "Neutral", "SpiritTypeNeutral", 0xBD, 0xAB, 0xC4) }
        };


        public class SpiritSeriesID
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string imageURL { get; set; }

            public SpiritSeriesID(ulong id, string name, string imageURL)
            {
                this.id = id;
                this.name = name;
                this.imageURL = "/Icons/" + imageURL + ".png";
            }
        }

        public static Dictionary<ulong, SpiritSeriesID> SpiritSeries = new Dictionary<ulong, SpiritSeriesID>(40)
        {
            { 85690272096, new SpiritSeriesID(85690272096, "Super Smash Bros.", "ui_series_smashbros") },
            { 68627075841, new SpiritSeriesID(68627075841, "Mario", "ui_series_mario") },
            { 83821041006, new SpiritSeriesID(83821041006, "Mario Kart", "ui_series_mariokart") },
            { 87305699505, new SpiritSeriesID(87305699505, "Donkey Kong", "ui_series_donkeykong") },
            { 68240413428, new SpiritSeriesID(68240413428, "The Legend of Zelda", "ui_series_zelda") },
            { 75989617305, new SpiritSeriesID(75989617305, "Metroid", "ui_series_metroid") },
            { 66546543411, new SpiritSeriesID(66546543411, "Yoshi", "ui_series_yoshi") },
            { 67237487316, new SpiritSeriesID(67237487316, "Kirby", "ui_series_kirby") },
            { 75895170176, new SpiritSeriesID(75895170176, "Star Fox", "ui_series_starfox") },
            { 75374961010, new SpiritSeriesID(75374961010, "Pokémon", "ui_series_pokemon") },
            { 70774870552, new SpiritSeriesID(70774870552, "EarthBound / Mother", "ui_series_mother") },
            { 67560377569, new SpiritSeriesID(67560377569, "F-Zero", "ui_series_fzero") },
            { 89303637441, new SpiritSeriesID(89303637441, "Ice Climber", "ui_series_iceclimber") },
            { 88954390139, new SpiritSeriesID(88954390139, "Fire Emblem", "ui_series_fireemblem") },
            { 84112380239, new SpiritSeriesID(84112380239, "Game & Watch", "ui_series_gamewatch") },
            { 78843239766, new SpiritSeriesID(78843239766, "Kid Icarus", "ui_series_palutena") },
            { 67917224994, new SpiritSeriesID(67917224994, "Wario", "ui_series_wario") },
            { 72956747932, new SpiritSeriesID(72956747932, "Pikmin", "ui_series_pikmin") },
            { 95401922247, new SpiritSeriesID(95401922247, "Robot", "ui_series_famicomrobot") },
            { 78472615768, new SpiritSeriesID(78472615768, "Animal Crossing", "ui_series_doubutsu") },
            { 71851855412, new SpiritSeriesID(71851855412, "Wii Fit", "ui_series_wiifit") },
            { 80415712290, new SpiritSeriesID(80415712290, "Punch-Out!!", "ui_series_punchout") },
            { 58933696934, new SpiritSeriesID(58933696934, "Mii", "ui_series_mii") },
            { 83807100960, new SpiritSeriesID(83807100960, "Xenoblade Chronicles", "ui_series_xenoblade") },
            { 79284858576, new SpiritSeriesID(79284858576, "Duck Hunt", "ui_series_duckhunt") },
            { 77394957707, new SpiritSeriesID(77394957707, "Splatoon", "ui_series_splatoon") },
            { 60579006242, new SpiritSeriesID(60579006242, "ARMS", "ui_series_arms") },
            { 81824949712, new SpiritSeriesID(81824949712, "Metal Gear", "ui_series_metalgear") },
            { 68081638382, new SpiritSeriesID(68081638382, "Sonic", "ui_series_sonic") },
            { 74440614183, new SpiritSeriesID(74440614183, "Mega Man", "ui_series_rockman") },
            { 71967066554, new SpiritSeriesID(71967066554, "PAC-MAN", "ui_series_pacman") },
            { 100229489815, new SpiritSeriesID(100229489815, "Street Fighter", "ui_series_streetfighter") },
            { 96095409209, new SpiritSeriesID(96095409209, "FINAL FANTASY VII", "ui_series_finalfantasy") },
            { 83536744749, new SpiritSeriesID(83536744749, "Bayonetta", "ui_series_bayonetta") },
            { 92824453941, new SpiritSeriesID(92824453941, "Castlevania", "ui_series_castlevania") },
            { 76228036378, new SpiritSeriesID(76228036378, "Persona", "ui_series_persona") },
            { 94199199541, new SpiritSeriesID(94199199541, "DRAGON QUEST", "ui_series_dragonquest") },
            { 95424208063, new SpiritSeriesID(95424208063, "Banjo-Kazooie", "ui_series_banjokazooie") },
            { 83107867128, new SpiritSeriesID(83107867128, "FATAL FURY", "ui_series_fatalfury") },
            { 82779413083, new SpiritSeriesID(82779413083, "Minecraft", "ui_series_minecraft") },
            { 68901289013, new SpiritSeriesID(68901289013, "Tekken", "ui_series_tekken") },
            { 100174952124, new SpiritSeriesID(100174952124, "KINGDOM HEARTS", "ui_series_kingdomhearts") },
            { 58671107356, new SpiritSeriesID(58671107356, "Other", "ui_series_etc") },
        };


        public class SpiritAbility
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string description { get; set; }

            public SpiritAbility(ulong id, string name, string description)
            {
                this.id = id;
                this.name = name;
                this.description = description;
            }
        }

        public static Dictionary<ulong, SpiritAbility> SpiritAbilities = new Dictionary<ulong, SpiritAbility>(200);


        public class SpiritAppearCondition
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public SpiritAppearCondition(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        public static Dictionary<ulong, SpiritAppearCondition> SpiritBoardAppearConditions = new Dictionary<ulong, SpiritAppearCondition>(4)
        {
            { 61968333762, new SpiritAppearCondition(61968333762, "Novice Spirits") },
            { 64232680056, new SpiritAppearCondition(64232680056, "Advanced Spirits") },
            { 62337309422, new SpiritAppearCondition(62337309422, "Ace Spirits") },
            { 60632026957, new SpiritAppearCondition(60632026957, "Legend Spirits") }
        };


        public class SpiritSalesType
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public SpiritSalesType(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        public static Dictionary<ulong, SpiritSalesType> SpiritSalesTypes = new Dictionary<ulong, SpiritSalesType>(3)
        {
            { 32952601179, new SpiritSalesType(32952601179, "For Sale") },
            { 53553909307, new SpiritSalesType(53553909307, "Not For Sale") },
            { 33699157543, new SpiritSalesType(33699157543, "Only if Unlocked") }
        };


        public class SpiritGameTitle: IComparable<SpiritGameTitle>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public SpiritGameTitle(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(SpiritGameTitle other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, SpiritGameTitle> SpiritGameTitles = new Dictionary<ulong, SpiritGameTitle>(1000);


        public class SpiritID : IComparable<SpiritID>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public SpiritID(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(SpiritID other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, SpiritID> Spirits = new Dictionary<ulong, SpiritID>(1500);
        public static Dictionary<ulong, SpiritID> SummonSpirits = new Dictionary<ulong, SpiritID>(4);


        public class BattleType : IComparable<BattleType>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleType(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleType other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleType> BattleTypes = new Dictionary<ulong, BattleType>(5)
        {
            { 22736688736, new BattleType(22736688736, "Stock") },
            { 44938739841, new BattleType(44938739841, "Stock/Time") },
            { 11760337516, new BattleType(11760337516, "Stamina") },
            { 33730908030, new BattleType(33730908030, "Stamina/Time") },
            { 18236728890, new BattleType(18236728890, "Boss") }
        };


        public class BattleFloorPlace : IComparable<BattleFloorPlace>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleFloorPlace(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleFloorPlace other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleFloorPlace> BattleFloorPlaces = new Dictionary<ulong, BattleFloorPlace>(4)
        {
            { 19320013007, new BattleFloorPlace(19320013007, "None") },
            { 31325675987, new BattleFloorPlace(31325675987, "Main Platform Only") },
            { 32835731711, new BattleFloorPlace(32835731711, "All But Main Platform") },
            { 33591046249, new BattleFloorPlace(33591046249, "Entire Stage (BF/Omega)") }
        };


        public class BattleItemLevel : IComparable<BattleItemLevel>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleItemLevel(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleItemLevel other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleItemLevel> BattleItemLevels = new Dictionary<ulong, BattleItemLevel>(5)
        {
            { 31060653905, new BattleItemLevel(31060653905, "None") },
            { 42306121174, new BattleItemLevel(42306121174, "Low") },
            { 55151767369, new BattleItemLevel(55151767369, "Normal") },
            { 45383078924, new BattleItemLevel(45383078924, "High") },
            { 46701562532, new BattleItemLevel(46701562532, "Very High") }
        };


        public class BattleItemTable : IComparable<BattleItemTable>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleItemTable(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleItemTable other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleItemTable> BattleItemTables = new Dictionary<ulong, BattleItemTable>(100)
        {
            { 19320013007, new BattleItemTable(19320013007, "None") },
            { 26332195044, new BattleItemTable(26332195044, "Normal") },
            { 33245306116, new BattleItemTable(33245306116, "Normal 2") },
            { 33463479698, new BattleItemTable(33463479698, "Normal 3") },
            { 49646326180, new BattleItemTable(49646326180, "Animal Crossing Items") },
            { 28486567822, new BattleItemTable(28486567822, "Assist Trophy") },
            { 25829271503, new BattleItemTable(25829271503, "Banana Peel") },
            { 50298442841, new BattleItemTable(50298442841, "Banana & Banana Gun") },
            { 35756388331, new BattleItemTable(35756388331, "Baseball Items") },
            { 43221581677, new BattleItemTable(43221581677, "Battering Items Only") },
            { 41832571995, new BattleItemTable(41832571995, "Beastball") },
            { 23609711764, new BattleItemTable(23609711764, "Beehive") },
            { 47344125582, new BattleItemTable(47344125582, "Beetle") },
            { 38854845382, new BattleItemTable(38854845382, "Black Hole") },
            { 43599225131, new BattleItemTable(43599225131, "Block-Shaped Items") },
            { 31401746988, new BattleItemTable(31401746988, "Bombchu") },
            { 49803918083, new BattleItemTable(49803918083, "Bomber") },
            { 39893350585, new BattleItemTable(39893350585, "Bombs Only") },
            { 33861300175, new BattleItemTable(33861300175, "Bob-omb") },
            { 41433083132, new BattleItemTable(41433083132, "Boomerang") },
            { 30046988337, new BattleItemTable(30046988337, "Bumper") },
            { 29283565067, new BattleItemTable(29283565067, "Bullet Bill") },
            { 29457573516, new BattleItemTable(29457573516, "Byte & Barq") },
            { 31924635046, new BattleItemTable(31924635046, "Charlie") },
            { 32349011612, new BattleItemTable(32349011612, "Cucco") },
            { 32383884160, new BattleItemTable(32383884160, "Daybreak") },
            { 19285049692, new BattleItemTable(19285049692, "Deku Nut") },
            { 60482810762, new BattleItemTable(60482810762, "Donkey Kong") },
            { 54995561838, new BattleItemTable(54995561838, "Dragoon") },
            { 24191481840, new BattleItemTable(24191481840, "Drill") },
            { 50905515270, new BattleItemTable(50905515270, "Fashionable Items") },
            { 46948793960, new BattleItemTable(46948793960, "Fire Flower") },
            { 31619094805, new BattleItemTable(31619094805, "Firebar") },
            { 37994350621, new BattleItemTable(37994350621, "Food") },
            { 25751996445, new BattleItemTable(25751996445, "Franklin Badge") },
            { 32500455187, new BattleItemTable(32500455187, "Freezie") },//32500455187
            { 26450167230, new BattleItemTable(26450167230, "Galaga") },
            { 23484006844, new BattleItemTable(23484006844, "Geno") },
            { 51586967474, new BattleItemTable(51586967474, "Golden Hammer") },
            { 47214197371, new BattleItemTable(47214197371, "Green Shell") },
            { 29510437430, new BattleItemTable(29510437430, "Hammer") },
            { 40210999174, new BattleItemTable(40210999174, "Head Modifiers Only") },
            { 41616975339, new BattleItemTable(41616975339, "Healing Items") },
            { 50156198844, new BattleItemTable(50156198844, "Hocotate Bomb") },
            { 47194048359, new BattleItemTable(47194048359, "Home Run Bat") },
            { 30314165076, new BattleItemTable(30314165076, "Hothead") },
            { 41212996069, new BattleItemTable(41212996069, "Killing Edge") },
            { 37345622040, new BattleItemTable(37345622040, "Launch Star") },
            { 34758365372, new BattleItemTable(34758365372, "Lip's Stick") },
            { 18812117247, new BattleItemTable(18812117247, "Magical Vacation") },
            { 47222416467, new BattleItemTable(47222416467, "Mario Items") },
            { 62949222554, new BattleItemTable(62949222554, "Mario Kart Items") },
            { 26509328410, new BattleItemTable(26509328410, "Maxim Tomato") },
            { 23462446792, new BattleItemTable(23462446792, "Mechanical Items") },
            { 43672814098, new BattleItemTable(43672814098, "Motion Sensor Bomb") },
            { 35809702479, new BattleItemTable(35809702479, "Mr. Saturn") },
            { 56814385066, new BattleItemTable(56814385066, "Mushrooms") },
            { 20282488946, new BattleItemTable(20282488946, "Ore Club") },
            { 32007706623, new BattleItemTable(32007706623, "Pauline") },
            { 32358602712, new BattleItemTable(32358602712, "Pitfall") },
            { 37884781078, new BattleItemTable(37884781078, "Poké Ball") },
            { 38244855731, new BattleItemTable(38244855731, "Pow Block") },
            { 47774872402, new BattleItemTable(47774872402, "Powerful Explosives Only") },
            { 37105946614, new BattleItemTable(37105946614, "Ramblin' Evil Mushroom") },
            { 29806272715, new BattleItemTable(29806272715, "Raygun") },
            { 44296156106, new BattleItemTable(44296156106, "Rocket Belt") },
            { 24970654380, new BattleItemTable(24970654380, "Screw Attack") },
            { 45199327435, new BattleItemTable(45199327435, "Shooting Items") },
            { 54159182454, new BattleItemTable(54159182454, "Shooting Items 2") },
            { 22704220538, new BattleItemTable(22704220538, "Smoke Ball") },
            { 29571259446, new BattleItemTable(29571259446, "Soccer Ball") },
            { 50523150390, new BattleItemTable(50523150390, "Special Flag") },
            { 27212211847, new BattleItemTable(27212211847, "Sphere-Shaped Items") },
            { 41878241359, new BattleItemTable(41878241359, "Sports Ball Items") },
            { 22589404050, new BattleItemTable(22589404050, "Staff") },
            { 31322276578, new BattleItemTable(31322276578, "Star Rod") },
            { 39053699865, new BattleItemTable(39053699865, "Star-Shaped Items") },
            { 45927181473, new BattleItemTable(45927181473, "Super Scope") },
            { 22662730625, new BattleItemTable(22662730625, "Super Scope 2") },
            { 38995168303, new BattleItemTable(38995168303, "Super Star") },
            { 46987834110, new BattleItemTable(46987834110, "Swords Only") },
            { 43048324582, new BattleItemTable(43048324582, "Throwing Items") },
            { 31716845040, new BattleItemTable(31716845040, "Thunder") },
            { 18069549533, new BattleItemTable(18069549533, "Timer") },
            { 49443404523, new BattleItemTable(49443404523, "Transformation Items Only") },
            { 23356055229, new BattleItemTable(23356055229, "Transformation Items Only 2") },
            { 22755385991, new BattleItemTable(22755385991, "Unira") },
            { 37477271996, new BattleItemTable(37477271996, "Warpstar") },
            { 40850636885, new BattleItemTable(40850636885, "X-Bomb") },
            { 24985736396, new BattleItemTable(24985736396, "Zelda Items") },
        };


        public class BattleWinCondition : IComparable<BattleWinCondition>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleWinCondition(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleWinCondition other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleWinCondition> BattleWinConditions = new Dictionary<ulong, BattleWinCondition>(4)
        {
            { 48643092140, new BattleWinCondition(48643092140, "Normal Rule") },
            { 56983179045, new BattleWinCondition(56983179045, "Defeat Main Fighter") },
            { 48640252968, new BattleWinCondition(48640252968, "Protect Ally") },
            { 39949115894, new BattleWinCondition(39949115894, "Survive") }
        };


        public class BattleStageAttribute : IComparable<BattleStageAttribute>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleStageAttribute(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleStageAttribute other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleStageAttribute> BattleStageAttributes = new Dictionary<ulong, BattleStageAttribute>(10)
        {
            { 57709432320, new BattleStageAttribute(57709432320, "None") },
            { 47793324270, new BattleStageAttribute(47793324270, "Sleep Floor") },
            { 39000505434, new BattleStageAttribute(39000505434, "Ice Floor") },
            { 52337765233, new BattleStageAttribute(52337765233, "Poison Floor") },
            { 52223644470, new BattleStageAttribute(52223644470, "Lava Floor") },
            { 40087441667, new BattleStageAttribute(40087441667, "Sticky Floor") },
            { 46338258500, new BattleStageAttribute(46338258500, "Electric Floor") },
            { 48935335206, new BattleStageAttribute(48935335206, "Poison Fog") },
            { 41759588792, new BattleStageAttribute(41759588792, "Wind") },
            { 40888301250, new BattleStageAttribute(40888301250, "Fog") },
        };


        public class BattleStageType : IComparable<BattleStageType>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleStageType(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleStageType other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleStageType> BattleStageTypes = new Dictionary<ulong, BattleStageType>(10)
        {
            { 53953130547, new BattleStageType(53953130547, "Normal") },
            { 50522800377, new BattleStageType(50522800377, "Omega") },
            { 55218520795, new BattleStageType(55218520795, "Battlefield") }
        };


        public class BattleStageID : IComparable<BattleStageID>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public Dictionary<int, BattleStageForm> stageForms;

            public BattleStageID(ulong id, string name, int[] forms, string[] formNames)
            {
                this.id = id;
                this.name = name;

                stageForms = new Dictionary<int, BattleStageForm>(forms.Length);

                for(int i=0; i<forms.Length; i++)
                {
                    stageForms.Add(forms[i], new BattleStageForm(forms[i], formNames[i]));
                }
            }

            public int CompareTo(BattleStageID other)
            {
                return name.CompareTo(other.name);
            }
        }


        public class BattleStageForm : IComparable<BattleStageForm>
        {
            public int id { get; set; }
            public string name { get; set; }

            public BattleStageForm(int id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleStageForm other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleStageID> BattleStageIDs = new Dictionary<ulong, BattleStageID>(110);


        public class BattleEvent : IComparable<BattleEvent>
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string description { get; set; }

            public Dictionary<ulong, BattleEventParam> eventParams;

            public BattleEvent(ulong id, string name, string description, int numEvents)
            {
                this.id = id;
                this.name = name;
                this.description = description;
                eventParams = new Dictionary<ulong, BattleEventParam>(numEvents);
            }

            public int CompareTo(BattleEvent other)
            {
                return name.CompareTo(other.name);
            }
        }


        public class BattleEventParam : IComparable<BattleEventParam>
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string description { get; set; }

            public BattleEventParam(ulong id, string name, string description)
            {
                this.id = id;
                this.name = name;
                this.description = description;
            }

            public int CompareTo(BattleEventParam other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleEvent> BattleEvents = new Dictionary<ulong, BattleEvent>(35);


        public class BattleBGM : IComparable<BattleBGM>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public BattleBGM(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(BattleBGM other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, BattleBGM> BattleBGMIDs = new Dictionary<ulong, BattleBGM>(2000);


        public class FighterID : IComparable<FighterID>
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public string internalID { get; set; }
            public bool hasAlts { get; set; }

            public FighterID(ulong id, string name, string internalID, bool hasAlts)
            {
                this.id = id;
                this.name = name;
                this.internalID = internalID;
                this.hasAlts = hasAlts;
            }

            public int CompareTo(FighterID other)
            {
                return name.CompareTo(other.name);
            }
        }

        public static Dictionary<ulong, FighterID> FighterIDs = new Dictionary<ulong, FighterID>(105);


        public class MiiData
        {
            public class MiiOutfit : IComparable<MiiOutfit>
            {
                public ulong id { get; set; }
                public string name { get; set; }

                public MiiOutfit(ulong id, string name)
                {
                    this.id = id;
                    this.name = name;
                }

                public int CompareTo(MiiOutfit other)
                {
                    return name.CompareTo(other.name);
                }
            }

            public class MiiSpecial : IComparable<MiiSpecial>
            {
                public int id { get; set; }
                public string name { get; set; }

                public MiiSpecial(int id, string name)
                {
                    this.id = id;
                    this.name = name;
                }

                public int CompareTo(MiiSpecial other)
                {
                    return name.CompareTo(other.name);
                }
            }


            public List<MiiOutfit> miiCostumes = new List<MiiOutfit>();
            public List<MiiSpecial> neutralSpecial = new List<MiiSpecial>();
            public List<MiiSpecial> sideSpecial = new List<MiiSpecial>();
            public List<MiiSpecial> upSpecial = new List<MiiSpecial>();
            public List<MiiSpecial> downSpecial = new List<MiiSpecial>();
        }


        public static Dictionary<ulong, MiiData> Miis = new Dictionary<ulong, MiiData>(3)
        {
            { 85865659179, new MiiData{
                neutralSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Shot Put"),
                    new MiiData.MiiSpecial(1, "Flashing Mach Punch"),
                    new MiiData.MiiSpecial(2, "Exploding Side Kick")
                },
                sideSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Onslaught"),
                    new MiiData.MiiSpecial(1, "Burning Drop Kick"),
                    new MiiData.MiiSpecial(2, "Suplex")
                },
                upSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Soaring Axe Kick"),
                    new MiiData.MiiSpecial(1, "Helicopter Kick"),
                    new MiiData.MiiSpecial(2, "Thrust Uppercut")
                },
                downSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Head On Assault"),
                    new MiiData.MiiSpecial(1, "Feint Jump"),
                    new MiiData.MiiSpecial(2, "Counter Throw")
                },
                miiCostumes = new List<MiiData.MiiOutfit>(50)
                {
                    new MiiData.MiiOutfit(120241759735, "Default (Male)"),
                    new MiiData.MiiOutfit(117728181375, "Default (Female)"),
                    new MiiData.MiiOutfit(83042725232, "Akira"),
                    new MiiData.MiiOutfit(81992822273, "Biker (Female)"),
                    new MiiData.MiiOutfit(83767981961, "Biker (Male)"),
                    new MiiData.MiiOutfit(85593655277, "Bionic Suit (Female)"),
                    new MiiData.MiiOutfit(83652599397, "Bionic Suit (Male)"),
                    new MiiData.MiiOutfit(99448277870, "Bomberman"),
                    new MiiData.MiiOutfit(103230141509, "Builder Mario"),
                    new MiiData.MiiOutfit(115546657956, "Business Suit (Female)"),
                    new MiiData.MiiOutfit(113561619756, "Business Suit (Male)"),
                    new MiiData.MiiOutfit(120220344395, "Butler Outfit"),
                    new MiiData.MiiOutfit(80749430342, "Callie (Female)"),
                    new MiiData.MiiOutfit(78833541070, "Callie (Male)"),
                    new MiiData.MiiOutfit(91883076202, "Captain Falcon"),
                    new MiiData.MiiOutfit(94047023748, "Cat Suit (Female)"),
                    new MiiData.MiiOutfit(92114348812, "Cat Suit (Male)"),
                    new MiiData.MiiOutfit(90510485314, "Creeper"),
                    new MiiData.MiiOutfit(80052714100, "DQ Martial Artist (Female)"),
                    new MiiData.MiiOutfit(78187176956, "DQ Martial Artist (Male)"),
                    new MiiData.MiiOutfit(109207665547, "Fighter Uniform (Female)"),
                    new MiiData.MiiOutfit(111578604035, "Fighter Uniform (Male)"),
                    new MiiData.MiiOutfit(100950625046, "Flying Man"),
                    new MiiData.MiiOutfit(96063335885, "Heihachi (Female)"),
                    new MiiData.MiiOutfit(97878340677, "Heihachi (Male)"),
                    new MiiData.MiiOutfit(80372030883, "Iori"),
                    new MiiData.MiiOutfit(83813984400, "Jacky"),
                    new MiiData.MiiOutfit(102694304877, "King K. Rool"),
                    new MiiData.MiiOutfit(95737211849, "Knuckles"),
                    new MiiData.MiiOutfit(109660211159, "Maid Outfit"),
                    new MiiData.MiiOutfit(93468668531, "Mech Suit (Female)"),
                    new MiiData.MiiOutfit(91619879931, "Mech Suit (Male)"),
                    new MiiData.MiiOutfit(76196375020, "Nia"),
                    new MiiData.MiiOutfit(93402531706, "Ninjara"),
                    new MiiData.MiiOutfit(77060367669, "Pig"),
                    new MiiData.MiiOutfit(100130094196, "Protetive Gear (Female)"),
                    new MiiData.MiiOutfit(102137931260, "Protetive Gear (Male)"),
                    new MiiData.MiiOutfit(106176703116, "Ribbon Girl"),
                    new MiiData.MiiOutfit(89029955544, "Rocket Grunt (Female)"),
                    new MiiData.MiiOutfit(86659274320, "Rocket Grunt (Male)"),
                    new MiiData.MiiOutfit(76603568242, "Ryo"),
                    new MiiData.MiiOutfit(91162313194, "Shantae"),
                    new MiiData.MiiOutfit(95535626646, "Skull Kid"),
                    new MiiData.MiiOutfit(102563765284, "Spring Man"),
                    new MiiData.MiiOutfit(95068035172, "SSB T-Shirt (Female)"),
                    new MiiData.MiiOutfit(97537343980, "SSB T-Shirt (Male)"),
                    new MiiData.MiiOutfit(80534401470, "Tifa (Female)"),
                    new MiiData.MiiOutfit(78784473142, "Tifa (Male)"),
                    new MiiData.MiiOutfit(80872832588, "Toad (Female)"),
                    new MiiData.MiiOutfit(78445520836, "Toad (Male)"),
                    new MiiData.MiiOutfit(77542381916, "Toy-Con Outfit"),
                    new MiiData.MiiOutfit(92725540178, "Tracksuit (Female)"),
                    new MiiData.MiiOutfit(90211986650, "Tracksuit (Male)"),
                    new MiiData.MiiOutfit(91415543123, "Vampire Garb (Female)"),
                    new MiiData.MiiOutfit(93937247451, "Vampire Garb (Male)"),
                    new MiiData.MiiOutfit(100151035440, "Woolly Yoshi"),
                }
            } },
            { 93801815927, new MiiData{
                neutralSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Gale Strike"),
                    new MiiData.MiiSpecial(1, "Shuriken of Light"),
                    new MiiData.MiiSpecial(2, "Blurring Blade")
                },
                sideSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Airborne Assault"),
                    new MiiData.MiiSpecial(1, "Gale Stab"),
                    new MiiData.MiiSpecial(2, "Chakram")
                },
                upSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Stone Scabbard"),
                    new MiiData.MiiSpecial(1, "Skyward Slash Dash"),
                    new MiiData.MiiSpecial(2, "Hero's Spin")
                },
                downSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Blade Counter"),
                    new MiiData.MiiSpecial(1, "Reversal Slash"),
                    new MiiData.MiiSpecial(2, "Power Thrust")
                },
                // Missing hashes for Arthur and MonHun costumes
                miiCostumes = new List<MiiData.MiiOutfit>(50)
                {
                    new MiiData.MiiOutfit(123719058421, "Default (Male)"),
                    new MiiData.MiiOutfit(121767582333, "Default (Female)"),
                    new MiiData.MiiOutfit(88635002606, "Aerith (Female)"),
                    new MiiData.MiiOutfit(86786185062, "Aerith (Male)"),
                    new MiiData.MiiOutfit(90004476813, "Altaïr"),
                    new MiiData.MiiOutfit(110654253517, "Ancient Armor"),
                    new MiiData.MiiOutfit(89465634376, "Arthur"),
                    new MiiData.MiiOutfit(86854348842, "Ashley"),
                    new MiiData.MiiOutfit(110634848217, "Black Knight"),
                    new MiiData.MiiOutfit(115929740607, "Business Suit (Female)"),
                    new MiiData.MiiOutfit(113449741495, "Business Suit (Male)"),
                    new MiiData.MiiOutfit(113449741495, "Business Suit (Male)"),
                    new MiiData.MiiOutfit(124793707902, "Butler Outfit"),
                    new MiiData.MiiOutfit(93011531273, "Champion's Outfit"),
                    new MiiData.MiiOutfit(85780454978, "Chrom"),
                    new MiiData.MiiOutfit(98028405506, "Cybernetic Suit (Female)"),
                    new MiiData.MiiOutfit(95649012362, "Cybernetic Suit (Male)"),
                    new MiiData.MiiOutfit(82516265633, "Dante"),
                    new MiiData.MiiOutfit(91845744920, "Diamond Armor"),
                    new MiiData.MiiOutfit(104087320230, "Dragonborn"),
                    new MiiData.MiiOutfit(87638071610, "Dunban"),
                    new MiiData.MiiOutfit(81578953877, "Erdrick"),
                    new MiiData.MiiOutfit(102213398902, "Gil"),
                    new MiiData.MiiOutfit(86939892064, "Goemon"),
                    new MiiData.MiiOutfit(95589798213, "Hunter (Female)"),
                    new MiiData.MiiOutfit(98084473037, "Hunter (Male)"),
                    new MiiData.MiiOutfit(83274239879, "Isaac"),
                    new MiiData.MiiOutfit(79193540089, "Link"),
                    new MiiData.MiiOutfit(76681114571, "Lip"),
                    new MiiData.MiiOutfit(83615779906, "Lloyd"),
                    new MiiData.MiiOutfit(117949410344, "Maid Outfit"),
                    new MiiData.MiiOutfit(103145972295, "Monkey Suit (Female)"),
                    new MiiData.MiiOutfit(105564896207, "Monkey Suit (Male)"),
                    new MiiData.MiiOutfit(97892485773, "Nakoruru (Female)"),
                    new MiiData.MiiOutfit(96050221829, "Nakoruru (Male)"),
                    new MiiData.MiiOutfit(83188041338, "Ninja Outfit (Female)"),
                    new MiiData.MiiOutfit(84988534770, "Ninja Outfit (Male)"),
                    new MiiData.MiiOutfit(88102502395, "Persona 3 Protagonist"),
                    new MiiData.MiiOutfit(87089889653, "Persona 4 Protagonist"),
                    new MiiData.MiiOutfit(89860296125, "Pirate Outfit (Female)"),
                    new MiiData.MiiOutfit(87975892021, "Pirate Outfit (Male)"),
                    new MiiData.MiiOutfit(76609744288, "Rex"),
                    new MiiData.MiiOutfit(98851210119, "SSB T-Shirt (Female)"),
                    new MiiData.MiiOutfit(101270362639, "SSB T-Shirt (Male)"),
                    new MiiData.MiiOutfit(96005126261, "Takamaru"),
                    new MiiData.MiiOutfit(87896635868, "Travis"),
                    new MiiData.MiiOutfit(98284598184, "Veronica"),
                    new MiiData.MiiOutfit(85488708109, "Vince"),
                    new MiiData.MiiOutfit(89399209280, "Viridi"),
                    new MiiData.MiiOutfit(95634787387, "Yiga Clan (Female)"),
                    new MiiData.MiiOutfit(98039092659, "Yiga Clan (Male)"),
                    new MiiData.MiiOutfit(79422605701, "Zero"),
                }
            } },
            { 78780243335, new MiiData{
                neutralSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Charge Blast"),
                    new MiiData.MiiSpecial(1, "Laser Blaze"),
                    new MiiData.MiiSpecial(2, "Grenade Launch")
                },
                sideSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Flame Pillar"),
                    new MiiData.MiiSpecial(1, "Stealth Burst"),
                    new MiiData.MiiSpecial(2, "Gunner Missile")
                },
                upSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Lunar Launch"),
                    new MiiData.MiiSpecial(1, "Cannon Jump Kick"),
                    new MiiData.MiiSpecial(2, "Arm Rocket")
                },
                downSpecial = new List<MiiData.MiiSpecial>(3)
                {
                    new MiiData.MiiSpecial(0, "Echo Reflector"),
                    new MiiData.MiiSpecial(1, "Bomb Drop"),
                    new MiiData.MiiSpecial(2, "Absorbing Vortex")
                },
                miiCostumes = new List<MiiData.MiiOutfit>(50)
                {
                    new MiiData.MiiOutfit(113391760503, "Default (Female)"),
                    new MiiData.MiiOutfit(115720728063, "Default (Male)"),
                    new MiiData.MiiOutfit(99394658249, "Astronaut Suit"),
                    new MiiData.MiiOutfit(86761742691, "Barret (Female)"),
                    new MiiData.MiiOutfit(88662735083, "Barret (Male)"),
                    new MiiData.MiiOutfit(95953613765, "Bear Suit (Female)"),
                    new MiiData.MiiOutfit(97720281677, "Bear Suit (Male)"),
                    new MiiData.MiiOutfit(115542359699, "Business Suit (Female)"),
                    new MiiData.MiiOutfit(113565935387, "Business Suit (Male)"),
                    new MiiData.MiiOutfit(111683411325, "Butler Outfit"),
                    new MiiData.MiiOutfit(102932226582, "Chibi-Robo"),
                    new MiiData.MiiOutfit(93124861579, "Cuphead"),
                    new MiiData.MiiOutfit(106835105204, "Doomguy"),
                    new MiiData.MiiOutfit(89933217970, "Dragon Armor (Female)"),
                    new MiiData.MiiOutfit(87638071610, "Dragon Armor (Male)"),
                    new MiiData.MiiOutfit(90720779540, "Fox"),
                    new MiiData.MiiOutfit(80940839143, "Geno"),
                    new MiiData.MiiOutfit(88482096023, "High-Tech Armor"),
                    new MiiData.MiiOutfit(97572113230, "Inkling (Girl)"),
                    new MiiData.MiiOutfit(95027368646, "Inkling (Boy)"),
                    new MiiData.MiiOutfit(96735756807, "Isabelle"),
                    new MiiData.MiiOutfit(95775577877, "K.K. Slider"),
                    new MiiData.MiiOutfit(103336068156, "Maid Outfit"),
                    new MiiData.MiiOutfit(89381490088, "Marie (Female)"),
                    new MiiData.MiiOutfit(87382004768, "Marie (Male)"),
                    new MiiData.MiiOutfit(73649144027, "MegaMan.EXE (Female)"),
                    new MiiData.MiiOutfit(76001188179, "MegaMan.EXE (Male)"),
                    new MiiData.MiiOutfit(81775740443, "Proto Man"),
                    new MiiData.MiiOutfit(106103734373, "Ray Mk III"),
                    new MiiData.MiiOutfit(79572720170, "Saki"),
                    new MiiData.MiiOutfit(85434233630, "Samus"),
                    new MiiData.MiiOutfit(80002714814, "Sans"),
                    new MiiData.MiiOutfit(118123779778, "Special Forces (Female)"),
                    new MiiData.MiiOutfit(116357009226, "Special Forces (Male)"),
                    new MiiData.MiiOutfit(101628728053, "Splatoon 2 (Girl)"),
                    new MiiData.MiiOutfit(99830303613, "Splatoon 2 (Boy)"),
                    new MiiData.MiiOutfit(99200714795, "SSB T-Shirt (Female)"),
                    new MiiData.MiiOutfit(101183394211, "SSB T-Shirt (Male)"),
                    new MiiData.MiiOutfit(82928624965, "Steampunk Getup (Female)"),
                    new MiiData.MiiOutfit(85249014989, "Steampunk Getup (Male)"),
                    new MiiData.MiiOutfit(83460532941, "Tails"),
                    new MiiData.MiiOutfit(97145992500, "Vault Boy"),
                    new MiiData.MiiOutfit(89691340313, "Wild West Outfit (Female)"),
                    new MiiData.MiiOutfit(87876429713, "Wild West Outfit (Male)"),
                    new MiiData.MiiOutfit(67582803953, "X"),
                }
            } },
        };

        public static Dictionary<ulong, MiiData.MiiOutfit> NotMiis = new Dictionary<ulong, MiiData.MiiOutfit>(1)
        {
            { 70328531327, new MiiData.MiiOutfit(70328531327, "None") }
        };

        public static Dictionary<ulong, MiiData.MiiSpecial> NotMiiSpecial = new Dictionary<ulong, MiiData.MiiSpecial>(1)
        {
            { 0, new MiiData.MiiSpecial(0, "None") }
        };

        public static Dictionary<ulong, MiiData.MiiOutfit> MiiFaces = new Dictionary<ulong, MiiData.MiiOutfit>(11)
        {
            { 0, new MiiData.MiiOutfit(0, "Default") },
            { 1, new MiiData.MiiOutfit(1, "Tifa") },
            { 2, new MiiData.MiiOutfit(2, "Aerith") },
            { 3, new MiiData.MiiOutfit(3, "Barret") },
            { 4, new MiiData.MiiOutfit(4, "Elena") },
            { 5, new MiiData.MiiOutfit(5, "Reno") },
            { 6, new MiiData.MiiOutfit(6, "Rude") },
            { 7, new MiiData.MiiOutfit(7, "Tseng") },
            { 8, new MiiData.MiiOutfit(8, "Jessie") },
            { 9, new MiiData.MiiOutfit(9, "Wedge") },
            { 10, new MiiData.MiiOutfit(10, "Biggs") },
            { 11, new MiiData.MiiOutfit(11, "Heihachi") }
        };

        public static Dictionary<ulong, MiiData.MiiOutfit> MiiHats = new Dictionary<ulong, MiiData.MiiOutfit>(150)
        {
            { 67275564522, new MiiData.MiiOutfit(67275564522, "None") },
            { 89501282834, new MiiData.MiiOutfit(89501282834, "1-Up Mushroom") },
            { 81024406504, new MiiData.MiiOutfit(81024406504, "Akira") },
            { 81687166967, new MiiData.MiiOutfit(81687166967, "Altaïr") },
            { 107012575246, new MiiData.MiiOutfit(107012575246, "Ancient Armor") },
            { 67836520881, new MiiData.MiiOutfit(67836520881, "Arcade Bunny") },
            { 82453295302, new MiiData.MiiOutfit(82453295302, "Arthur") },
            { 73221737838, new MiiData.MiiOutfit(73221737838, "Ashley") },
            { 88700531282, new MiiData.MiiOutfit(88700531282, "Astronaut") },
            { 74785720055, new MiiData.MiiOutfit(74785720055, "Bandit") },
            { 77631691035, new MiiData.MiiOutfit(77631691035, "Barbara") },
            { 94269564858, new MiiData.MiiOutfit(94269564858, "Bear (Female)") },
            { 91892334130, new MiiData.MiiOutfit(91892334130, "Bear (Male)") },
            { 78744890805, new MiiData.MiiOutfit(78744890805, "Bionic Helmet (Female)") },
            { 80570315837, new MiiData.MiiOutfit(80570315837, "Bionic Helmet (Male)") },
            { 95244996709, new MiiData.MiiOutfit(95244996709, "Black Knight") },
            { 97565956808, new MiiData.MiiOutfit(97565956808, "Bomberman") },
            { 93696009607, new MiiData.MiiOutfit(93696009607, "Builder Mario") },
            { 74224499586, new MiiData.MiiOutfit(74224499586, "Callie") },
            { 70916184378, new MiiData.MiiOutfit(70916184378, "Cappy") },
            { 105972398864, new MiiData.MiiOutfit(105972398864, "Captain Falcon") },
            { 88518294019, new MiiData.MiiOutfit(88518294019, "Cat (Female)") },
            { 86097208203, new MiiData.MiiOutfit(86097208203, "Cat (Male)") },
            { 73305127812, new MiiData.MiiOutfit(73305127812, "Chain Chomp") },
            { 79435870774, new MiiData.MiiOutfit(79435870774, "Champion (BotW)") },
            { 89069794763, new MiiData.MiiOutfit(89069794763, "Chibi-Robo") },
            { 89450498048, new MiiData.MiiOutfit(89450498048, "Chocobo") },
            { 69455796285, new MiiData.MiiOutfit(69455796285, "Chrom") },
            { 86520180947, new MiiData.MiiOutfit(86520180947, "Creeper") },
            { 69183673862, new MiiData.MiiOutfit(69183673862, "Crown") },
            { 89795580298, new MiiData.MiiOutfit(89795580298, "Cuphead") },
            { 68855074326, new MiiData.MiiOutfit(68855074326, "Daisy") },
            { 78086097382, new MiiData.MiiOutfit(78086097382, "Dante") },
            { 72575805703, new MiiData.MiiOutfit(72575805703, "Devil Horns") },
            { 89017416445, new MiiData.MiiOutfit(89017416445, "Diamond Helmet") },
            { 90023595279, new MiiData.MiiOutfit(90023595279, "Dixie Kong") },
            { 102831393864, new MiiData.MiiOutfit(102831393864, "Doomguy") },
            { 84112110201, new MiiData.MiiOutfit(84112110201, "DQ Martial Artist (Female)") },
            { 81649085425, new MiiData.MiiOutfit(81649085425, "DQ Martial Artist (Male)") },
            { 98918948199, new MiiData.MiiOutfit(98918948199, "Dragonborn") },
            { 74785978443, new MiiData.MiiOutfit(74785978443, "Dragon Horns") },
            { 73477675880, new MiiData.MiiOutfit(73477675880, "Dunban") },
            { 76060164416, new MiiData.MiiOutfit(76060164416, "Erdrick") },
            { 79501094803, new MiiData.MiiOutfit(79501094803, "Felyne") },
            { 68309939374, new MiiData.MiiOutfit(68309939374, "Floral Hat") },
            { 90017423093, new MiiData.MiiOutfit(90017423093, "Flying Man") },
            { 79064135133, new MiiData.MiiOutfit(79064135133, "Fox") },
            { 74892958176, new MiiData.MiiOutfit(74892958176, "Geno") },
            { 97431513424, new MiiData.MiiOutfit(97431513424, "Gil") },
            { 82526245997, new MiiData.MiiOutfit(82526245997, "Goemon") },
            { 93781997664, new MiiData.MiiOutfit(93781997664, "Heihachi") },
            { 74558650273, new MiiData.MiiOutfit(74558650273, "Hockey Mask") },
            { 100464504399, new MiiData.MiiOutfit(100464504399, "Hunter (Female)") },
            { 102877132743, new MiiData.MiiOutfit(102877132743, "Hunter (Male)") },
            { 110008335723, new MiiData.MiiOutfit(110008335723, "Inkling (Girl)") },
            { 107555792099, new MiiData.MiiOutfit(107555792099, "Inkling (Boy)") },
            { 75391292269, new MiiData.MiiOutfit(75391292269, "Iori") },
            { 83243368651, new MiiData.MiiOutfit(83243368651, "Isabelle") },
            { 69671685114, new MiiData.MiiOutfit(69671685114, "Isaac") },
            { 78399419909, new MiiData.MiiOutfit(78399419909, "Jacky") },
            { 77887763211, new MiiData.MiiOutfit(77887763211, "Judd") },
            { 86042363111, new MiiData.MiiOutfit(86042363111, "King K. Rool") },
            { 81326473563, new MiiData.MiiOutfit(81326473563, "K.K. Slider") },
            { 92185966822, new MiiData.MiiOutfit(92185966822, "Knuckles") },
            { 66731540831, new MiiData.MiiOutfit(66731540831, "Lacy Headband") },
            { 68196449492, new MiiData.MiiOutfit(68196449492, "Link") },
            { 64217974564, new MiiData.MiiOutfit(64217974564, "Lip") },
            { 78101806614, new MiiData.MiiOutfit(78101806614, "Lloyd") },
            { 72276722222, new MiiData.MiiOutfit(72276722222, "Luigi") },
            { 74755704771, new MiiData.MiiOutfit(74755704771, "Magic Hat") },
            { 98692702275, new MiiData.MiiOutfit(98692702275, "Majora's Mask") },
            { 82998086358, new MiiData.MiiOutfit(82998086358, "Marie") },
            { 70087831420, new MiiData.MiiOutfit(70087831420, "Mario") },
            { 65308792458, new MiiData.MiiOutfit(65308792458, "Marx") },
            { 69323329637, new MiiData.MiiOutfit(69323329637, "MegaMan.EXE") },
            { 64450572560, new MiiData.MiiOutfit(64450572560, "Meta Knight") },
            { 99973365658, new MiiData.MiiOutfit(99973365658, "Monkey (Female)") },
            { 102294005266, new MiiData.MiiOutfit(102294005266, "Monkey (Male)") },
            { 81958183285, new MiiData.MiiOutfit(81958183285, "Morgana") },
            { 85247418384, new MiiData.MiiOutfit(85247418384, "Mr. Saturn") },
            { 94056507848, new MiiData.MiiOutfit(94056507848, "Nakoruru") },
            { 62721913784, new MiiData.MiiOutfit(62721913784, "Nia") },
            { 88561520131, new MiiData.MiiOutfit(88561520131, "Ninjara") },
            { 68890532243, new MiiData.MiiOutfit(68890532243, "Ninja Headband") },
            { 86533994248, new MiiData.MiiOutfit(86533994248, "Octoling") },
            { 84319162731, new MiiData.MiiOutfit(84319162731, "P3 Protagonist") },
            { 81938910587, new MiiData.MiiOutfit(81938910587, "P4 Protagonist") },
            { 69733894936, new MiiData.MiiOutfit(69733894936, "Peach") },
            { 70946927554, new MiiData.MiiOutfit(70946927554, "Pig") },
            { 75667671172, new MiiData.MiiOutfit(75667671172, "Pirate Hat") },
            { 73028255010, new MiiData.MiiOutfit(73028255010, "Prince's Crown") },
            { 84266029455, new MiiData.MiiOutfit(84266029455, "Princess's Crown") },
            { 81442343984, new MiiData.MiiOutfit(81442343984, "Proto Man") },
            { 92812857706, new MiiData.MiiOutfit(92812857706, "Ray Mk III") },
            { 86771149341, new MiiData.MiiOutfit(86771149341, "Rabbid") },
            { 61327461984, new MiiData.MiiOutfit(61327461984, "Rex") },
            { 99554351380, new MiiData.MiiOutfit(99554351380, "Ribbon Girl") },
            { 82736985563, new MiiData.MiiOutfit(82736985563, "Rocket Grunt") },
            { 71996898767, new MiiData.MiiOutfit(71996898767, "Ryo") },
            { 75757841851, new MiiData.MiiOutfit(75757841851, "Sans") },
            { 66400272332, new MiiData.MiiOutfit(66400272332, "Saki") },
            { 70520120256, new MiiData.MiiOutfit(70520120256, "Samus") },
            { 86480602174, new MiiData.MiiOutfit(86480602174, "Shantae") },
            { 72701147385, new MiiData.MiiOutfit(72701147385, "Sheik") },
            { 68774242811, new MiiData.MiiOutfit(68774242811, "Shy Guy") },
            { 79993009299, new MiiData.MiiOutfit(79993009299, "Silk Hat") },
            { 79736759604, new MiiData.MiiOutfit(79736759604, "Slime") },
            { 83982261425, new MiiData.MiiOutfit(83982261425, "Skull Kid") },
            { 107317967963, new MiiData.MiiOutfit(107317967963, "Special Forces") },
            { 75504579795, new MiiData.MiiOutfit(75504579795, "Spiny") },
            { 96367429693, new MiiData.MiiOutfit(96367429693, "Splatoon 2 (Female)") },
            { 98652164533, new MiiData.MiiOutfit(98652164533, "Splatoon (Male)") },
            { 95147771628, new MiiData.MiiOutfit(95147771628, "Spring Man") },
            { 81872837203, new MiiData.MiiOutfit(81872837203, "Squid Hat") },
            { 87236830120, new MiiData.MiiOutfit(87236830120, "Super Mushroom") },
            { 78335698409, new MiiData.MiiOutfit(78335698409, "Tails") },
            { 84103095399, new MiiData.MiiOutfit(84103095399, "Takamaru") },
            { 82535621244, new MiiData.MiiOutfit(82535621244, "Teddie") },
            { 66149695048, new MiiData.MiiOutfit(66149695048, "Toad") },
            { 65583769209, new MiiData.MiiOutfit(65583769209, "Toy-Con VR") },
            { 83777923058, new MiiData.MiiOutfit(83777923058, "Travis") },
            { 94452486259, new MiiData.MiiOutfit(94452486259, "Vault Boy") },
            { 93936447816, new MiiData.MiiOutfit(93936447816, "Veronica") },
            { 71069186688, new MiiData.MiiOutfit(71069186688, "Vince") },
            { 77302584152, new MiiData.MiiOutfit(77302584152, "Viridi") },
            { 77371298601, new MiiData.MiiOutfit(77371298601, "Waluigi") },
            { 70797746271, new MiiData.MiiOutfit(70797746271, "Wario") },
            { 90036947742, new MiiData.MiiOutfit(90036947742, "Woolly Yoshi") },
            { 62067193535, new MiiData.MiiOutfit(62067193535, "X") },
            { 82817225652, new MiiData.MiiOutfit(82817225652, "Yiga Clan") },
            { 103170071798, new MiiData.MiiOutfit(103170071798, "Zelda") },
            { 73751792639, new MiiData.MiiOutfit(73751792639, "Zero") },
        };


        public class MiiVoice : IComparable<MiiVoice>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public MiiVoice(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(MiiVoice other)
            {
                return name.CompareTo(other.name);
            }
        }


        public static Dictionary<ulong, MiiVoice> MiiVoices = new Dictionary<ulong, MiiVoice>(37)
        {
            { 44330854164, new MiiVoice(44330854164, "No Voice") },
            { 48975738972, new MiiVoice(48975738972, "Type 1 (Mid)") },
            { 50265125939, new MiiVoice(50265125939, "Type 1 (Low)") },
            { 55551628977, new MiiVoice(55551628977, "Type 1 (High)") },
            { 48386539288, new MiiVoice(48386539288, "Type 2 (Mid)") },
            { 49780716407, new MiiVoice(49780716407, "Type 2 (Low)") },
            { 54201955007, new MiiVoice(54201955007, "Type 2 (High)") },
            { 71434724761, new MiiVoice(71434724761, "Type 3 (Mid)") },
            { 70647933430, new MiiVoice(70647933430, "Type 3 (Low)") },
            { 73096911251, new MiiVoice(73096911251, "Type 3 (High)") },
            { 54142912223, new MiiVoice(54142912223, "Type 4 (Mid)") },
            { 52748470960, new MiiVoice(52748470960, "Type 4 (Low)") },
            { 58442334485, new MiiVoice(58442334485, "Type 4 (High)") },
            { 46187931566, new MiiVoice(46187931566, "Type 5 (Mid)") },
            { 43254115265, new MiiVoice(43254115265, "Type 5 (Low)") },
            { 50402371614, new MiiVoice(50402371614, "Type 5 (High)") },
            { 53686801963, new MiiVoice(53686801963, "Type 6 (Mid)") },
            { 54439778884, new MiiVoice(54439778884, "Type 6 (Low)") },
            { 56394229696, new MiiVoice(56394229696, "Type 6 (High)") },
            { 42881237986, new MiiVoice(42881237986, "Type 7 (Mid)") },
            { 39339249549, new MiiVoice(39339249549, "Type 7 (Low)") },
            { 46228032897, new MiiVoice(46228032897, "Type 7 (High)") },
            { 44580288220, new MiiVoice(44580288220, "Type 8 (Mid)") },
            { 45937572531, new MiiVoice(45937572531, "Type 8 (Low)") },
            { 47288421087, new MiiVoice(47288421087, "Type 8 (High)") },
            { 58389387617, new MiiVoice(58389387617, "Type 9 (Mid)") },
            { 57099277582, new MiiVoice(57099277582, "Type 9 (Low)") },
            { 63211680641, new MiiVoice(63211680641, "Type 9 (High)") },
            { 57974646466, new MiiVoice(57974646466, "Type 10 (Mid)") },
            { 58731293357, new MiiVoice(58731293357, "Type 10 (Low)") },
            { 64299567212, new MiiVoice(64299567212, "Type 10 (High)") },
            { 48320749720, new MiiVoice(48320749720, "Type 11 (Mid)") },
            { 49711066359, new MiiVoice(49711066359, "Type 11 (Low)") },
            { 53469813312, new MiiVoice(53469813312, "Type 11 (High)") },
            { 54887475669, new MiiVoice(54887475669, "Type 12 (Mid)") },
            { 51886351802, new MiiVoice(51886351802, "Type 12 (Low)") },
            { 57913249572, new MiiVoice(57913249572, "Type 12 (High)") },
        };

        public static Dictionary<ulong, MiiData.MiiOutfit> MiiColors = new Dictionary<ulong, MiiData.MiiOutfit>(12)
        {
            { 0, new MiiData.MiiOutfit(0, "Red") },
            { 1, new MiiData.MiiOutfit(1, "Orange") },
            { 2, new MiiData.MiiOutfit(2, "Yellow") },
            { 3, new MiiData.MiiOutfit(3, "Light Green") },
            { 4, new MiiData.MiiOutfit(4, "Green") },
            { 5, new MiiData.MiiOutfit(5, "Blue") },
            { 6, new MiiData.MiiOutfit(6, "Light Blue") },
            { 7, new MiiData.MiiOutfit(7, "Pink") },
            { 8, new MiiData.MiiOutfit(8, "Purple") },
            { 9, new MiiData.MiiOutfit(9, "Brown") },
            { 10, new MiiData.MiiOutfit(10, "White") },
            { 11, new MiiData.MiiOutfit(11, "Black") },
        };


        public class CPUBehavior : IComparable<CPUBehavior>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public CPUBehavior(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(CPUBehavior other)
            {
                return name.CompareTo(other.name);
            }
        }


        public static Dictionary<ulong, CPUBehavior> CPUBehaviors = new Dictionary<ulong, CPUBehavior>(100);


        public class CPUSubRule : IComparable<CPUSubRule>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public CPUSubRule(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(CPUSubRule other)
            {
                return name.CompareTo(other.name);
            }
        }


        public static Dictionary<ulong, CPUSubRule> CPUSubRules = new Dictionary<ulong, CPUSubRule>(10)
        {
            { 51007861228, new CPUSubRule(51007861228, "No Sub Rule") },
            { 45815804672, new CPUSubRule(45815804672, "Giant") },
            { 45657510465, new CPUSubRule(45657510465, "Tiny") },
            { 44934963085, new CPUSubRule(44934963085, "Metal") },
            { 61493440249, new CPUSubRule(61493440249, "Invisible") },
            { 54151641411, new CPUSubRule(54151641411, "Reflect") },
            { 43069108659, new CPUSubRule(43069108659, "Curry") },
            { 48334460511, new CPUSubRule(48334460511, "Bunny") },
            { 42609986447, new CPUSubRule(42609986447, "Tail") },
            { 42235333315, new CPUSubRule(42235333315, "Golden") },
        };


        public class CPUEntryType : IComparable<CPUEntryType>
        {
            public ulong id { get; set; }
            public string name { get; set; }

            public CPUEntryType(ulong id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public int CompareTo(CPUEntryType other)
            {
                return name.CompareTo(other.name);
            }
        }


        public static Dictionary<ulong, CPUEntryType> EntryTypes = new Dictionary<ulong, CPUEntryType>(4)
        {
            { 40473814347, new CPUEntryType(40473814347, "Main Fighter") },
            { 37233412328, new CPUEntryType(37233412328, "Fighter") },
            { 47624522697, new CPUEntryType(47624522697, "Ally Fighter") },
            { 42324224621, new CPUEntryType(42324224621, "Boss") },
        };

        #endregion

        public static List<string> Fighters = new List<string>(80)
        {
            "Mario",
            "Donkey Kong",
            "Link",
            "Samus",
            "Dark Samus",
            "Yoshi",
            "Fox McCloud",
            "Pikachu",
            "Luigi",
            "Ness",
            "Captain Falcon",
            "Jigglypuff",
            "Peach",
            "Daisy",
            "Sheik",
            "Zelda",
            "Ice Climbers",
            "Marth",
            "Lucina",
            "Ganondorf",
            "Falco Lombardi",
            "Mewtwo",
            "Dr. Mario",
            "Young Link",
            "Pichu",
            "Roy (Fire Emblem)",
            "Mr. Game & Watch",
            "Chrom",
            "Meta Knight",
            "Pit",
            "Dark Pit",
            "Zero Suit Samus",
            "Wario",
            "Solid Snake",
            "Ike (Path of Radiance)",
            "Diddy Kong",
            "Pokémon Trainer (Male)",
            "Lucas",
            "King Dedede",
            "Sonic the Hedgehog",
            "Olimar",
            "Lucario",
            "R.O.B.",
            "Toon Link",
            "Wolf O'Donnell",
            "Villager (Boy)",
            "Mega Man",
            "Wii Fit Trainer (Female)",
            "Rosalina",
            "Little Mac",
            "Greninja",
            "Palutena",
            "PAC-MAN",
            "Robin (Male)",
            "Shulk",
            "Bowser Jr.",
            "Duck Hunt",
            "Ryu",
            "Ken",
            "Cloud",
            "Corrin (Male)",
            "Bayonetta (Bayonetta 2)",
            "Inkling (Girl)",
            "Ridley",
            "Simon Belmont",
            "Richter Belmont",
            "King K. Rool",
            "Isabelle",
            "Incineroar",
            "Piranha Plant",
            "Joker",
            "Hero (DRAGON QUEST XI S)",
            "Banjo & Kazooie",
            "Terry Bogard",
            "Byleth (Male)",
            "Min Min (Fighter)",
            "Steve",
            "Sephiroth",
            "Pyra (Fighter)",
            "Kazuya Mishima",
            "Sora",
            "Mii Brawler",
            "Mii Swordfighter",
            "Mii Gunner"
        };


        public static List<string> Bosses = new List<string>(6)
        {
            "Bowser",
            "Giga Bowser",
            "Rathalos",
            "Galleom",
            "Ganon",
            "Ganondorf",
            "Marx",
            "Marx (True Form)",
            "Dracula",
            "Dracula (2nd Form)"
        };
    }
}


public class AppSaveData
{
    public string bgmPath = "";
    public int DebugSpiritBoardID = 10;
}
