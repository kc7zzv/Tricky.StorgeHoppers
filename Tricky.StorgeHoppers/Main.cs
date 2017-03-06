using System;
using UnityEngine;
using System.IO;
using System.Web;
using System.Reflection;
using System.Globalization;

public static class Variables
{
    public static bool ModDebug = true;
    public static string FCEModPathOLD = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ProjectorGames\\FortressCraft\\Mods\\ModLog";
    public static string FCEModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ModLog\\";
    public static string[] FCEModPath_split = FCEModPath.Split('\\');
    public static int FCEModPath_length = FCEModPath_split.Length - 1;

    public static string ModName = Path.GetFileName(Assembly.GetExecutingAssembly().Location).Split('.')[1];
    public static string Author = Path.GetFileName(Assembly.GetExecutingAssembly().Location).Split('.')[0].Split('_')[1];
    public static string ModVersion = FCEModPath_split[FCEModPath_length - 2];
    public static string LogFilePath = FCEModPath + "ModLog.log";
    public static string PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
    private static object locker = new object();
    public static int HopperNumber = 0;

    public static void Start()
    {
        if (Directory.Exists(Variables.FCEModPathOLD))
        {
            Directory.Delete(Variables.FCEModPathOLD, true);
        }
        if (!Directory.Exists(Variables.FCEModPath))
        {
            Directory.CreateDirectory(Variables.FCEModPath);
        }
        DelteLogFile();
        PrintLine();
        LogPlain("[" + ModName + "] Loaded!");
        LogPlain("Mod created by " + Author + "!");
        LogPlain("Version " + ModVersion + " loaded!");
        PrintLine();

    }


    public static void Log(object debug)
    {
        PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
        if (ModDebug)
        {
            debug = debug.ToString();
            string str = PreString + "***LOG***: " + debug;
            WriteStringToFile(str);
        }

    }
    public static void LogPlain(object debug)
    {
        PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
        if (ModDebug)
        {
            string str = debug.ToString();
            WriteStringToFile(str);
        }

    }

    public static void LogError(object debug)
    {
        PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
        if (ModDebug)
        {
            debug = debug.ToString();
            string str = PreString + "***ERROR LOG***: " + debug;
            WriteStringToFile(str);
        }

    }

    public static void LogValue(object ValueText, object Value)
    {
        PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
        if (ModDebug)
        {
            ValueText = ValueText.ToString();
            Value = Value.ToString();
            string str = PreString + "***VALUE LOG***: " + ValueText + " = " + Value;
            WriteStringToFile(str);
        }

    }
    public static void PrintLine()
    {
        WriteStringToFile("*******************************************************************************************");
    }

    public static void LogValue(object ValueText, object Value, bool Error)
    {
        PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
        if (ModDebug)
        {
            ValueText = ValueText.ToString();
            Value = Value.ToString();
            string str = PreString + "***VALUE LOG***: " + ValueText + " = " + Value;
            string str2 = PreString + "***VALUE LOG***: " + ValueText + " = " + Value;
            if (Error)
            {
                WriteStringToFile(str);
            }
            else
            {
                WriteStringToFile(str2);
            }
        }
    }

    public static void WriteStringToFile(string ValueText)
    {
        try
        {
            lock (locker)
            {
                using (FileStream file = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.WriteLine(ValueText);
                }

            }
        }
        catch (Exception)
        {

            throw;
        }

    }

    public static void DelteLogFile()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
                File.Create(LogFilePath).Close();
            }
            else
            {
                File.Create(LogFilePath).Close();
            }
        }
        catch (Exception)
        {

            throw;
        }

    }
}

public class ExtraStorageHoppersMain : FortressCraftMod
{
    public ushort mHopperCubeType;
    public ushort mHopperCubeValue;
    public TerrainDataEntry mHopperCubeEntry;
    public TerrainDataValueEntry mHopperValueEntry;
    public string FCExmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Xml";
    public string FCEBackup = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Backup";
    //MY STUFF
    public void CreateBackupFolder()
    {
        if (!Directory.Exists(FCEBackup))
        {
            Directory.CreateDirectory(FCEBackup);
        }
    }
    //******************** Backup TerrainData and ManufacturerRecipes files ********************
    //For people with custom hoppers
    public void BackupFiles()
    {
        string FolderName = System.DateTime.Today.Month + "-" + DateTime.Today.Day + "\\";
        string BackupTrueFolder = FCEBackup + FolderName;
        if (!Directory.Exists(BackupTrueFolder))
        {
            File.Copy(FCExmlPath + "TerrainData.xml", BackupTrueFolder + "TerrainData.xml");
            File.Copy(FCExmlPath + "ManufacturerRecipes.xml", BackupTrueFolder + "ManufacturerRecipes.xml");
        }

    }


    //******************** UI STUFF ********************
    public static ExtraStorageHopperWindowNew ExtraStorageHopperUI = new ExtraStorageHopperWindowNew();

    void Start()
    {
        Variables.Start();
    }

    public override ModRegistrationData Register()
    {
        //Registers my mod, so FC knows what to load
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("Tricky.ExtraStorageHoppers");
        modRegistrationData.RegisterEntityUI("Tricky.ExtraStorageHoppers", ExtraStorageHopperUI);
        UIManager.NetworkCommandFunctions.Add("ExtraStorageHopperWindowNew", new UIManager.HandleNetworkCommand(ExtraStorageHopperWindowNew.HandleNetworkCommand));
        TerrainDataEntry CubeEntry;
        TerrainDataValueEntry EntryValue;
        TerrainData.GetCubeByKey("Tricky.ExtraStorageHoppers", out CubeEntry, out EntryValue);
        if (CubeEntry != null)
        {
            mHopperCubeType = CubeEntry.CubeType;
            mHopperCubeEntry = CubeEntry;
        }
        return modRegistrationData;
    }

    public override void CreateSegmentEntity(ModCreateSegmentEntityParameters parameters, ModCreateSegmentEntityResults results)
    {
        //Makes sure that all hoppers has the LogisticsHopper model
        parameters.ObjectType = SpawnableObjectEnum.LogisticsHopper;
        //Default Custom Storage Hopper values
        ushort HopperMaxStorage = 10;
        float HopperColorR = 1;
        float HopperColorG = 2;
        float HopperColorB = 3;
        string HopperName = "NO NAME";
        bool HopperOT = false;
        Color HopperColor = new Color(1, 2, 3);
        //Starts to parse the parameteres from the xml file into values
        if (parameters.Cube == mHopperCubeType)
        {
            var entry = mHopperCubeEntry.GetValue(parameters.Value);
            if (entry != null && entry.Custom != null)
            {
                //Trys to log what the values loaded for custom hoppers are, this is to debug
                try
                {
                    HopperMaxStorage = Convert.ToUInt16(entry.Custom.GetValue("Tricky.MaxStorage"));
                    Variables.LogValue("HopperMaxStorage", HopperMaxStorage);
                    HopperColorR = float.Parse(entry.Custom.GetValue("Tricky.ColorR"), CultureInfo.InvariantCulture.NumberFormat);
                    Variables.LogValue("HopperColorR", HopperColorR);
                    HopperColorG = float.Parse(entry.Custom.GetValue("Tricky.ColorG"), CultureInfo.InvariantCulture.NumberFormat);
                    Variables.LogValue("HopperColorG", HopperColorG);
                    HopperColorB = float.Parse(entry.Custom.GetValue("Tricky.ColorB"), CultureInfo.InvariantCulture.NumberFormat);
                    Variables.LogValue("HopperColorB", HopperColorB);
                    HopperColor = new Color(HopperColorR, HopperColorG, HopperColorB);
                    Variables.LogValue("HopperColor", HopperColor);
                    HopperName = entry.Custom.GetValue("Tricky.HopperName");
                    Variables.LogValue("HopperName", HopperName);
                    HopperOT = Convert.ToBoolean(entry.Custom.GetValue("Tricky.OT"));
                    Variables.LogValue("HopperOT", HopperOT);
                }
                catch (Exception)
                {
                    Variables.LogError("Something went wrong, when loading values for a hopper!");
                    results.Entity = null;
                    return;
                    throw;
                }

            }
            //Moves the variables into a new hopper, and uses the functions from ExtraStorageHoppers.cs 
            results.Entity = new ExtraStorageHoppers(parameters, HopperMaxStorage, HopperColor, HopperName, HopperOT);

        }
        return;
    }
}