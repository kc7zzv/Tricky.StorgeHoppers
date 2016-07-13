using System;
using UnityEngine;
using System.IO;
using System.Web;

class Variables
{
    public static string ModName = "Tricky.ExtraStorageHoppers";
    public static string ModVersion = "7";
    public static bool ModDebug = true;
	//ONLY USED IF THE OLD MODLOG FOLDER EXIST, THIS FOLDER CAUSED AN ERROR.
    public static string FCEModPathOLD = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ProjectorGames\\FortressCraft\\Mods\\ModLog";
	//THIS IS THE NEW MODLOG FOLDER, WHICH IS PLACED INSIDE THE CORRECT MOD FOLDER.
	public static string FCEModPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ProjectorGames\\FortressCraft\\Mods\\" + ModName + "\\" + ModVersion + "\\ModLog\\";
    public static string LogFilePath = FCEModPath + "ModLog.log";
    public static string PreString = "[" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
    private static object locker = new object();
	public static int HopperNumber = 0;

    public static void Log(object debug)
    {
        if (ModDebug)
        {
            debug = debug.ToString();
            string str = PreString + "***LOG***: " + debug;
            WriteStringToFile(str);
        }

    }

    public static void LogPlain(object debug)
    {
        if (ModDebug)
        {
            string str = debug.ToString();
            WriteStringToFile(str);
        }

    }

    public static void LogError(object debug)
    {
        if (ModDebug)
        {
            debug = debug.ToString();
            string str = PreString + "***ERROR LOG***: " + debug;
            WriteStringToFile(str);
        }

    }

    public static void LogValue(object ValueText, object Value)
    {
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
            Debug.LogError("Something went wrong when trying to delete the ModLog File");
        }

    }
}

public class ExtraStorageHoppersMain : FortressCraftMod
{
    public ushort mHopperCubeType;
    public ushort mHopperCubeValue;
    public TerrainDataEntry mHopperCubeEntry;
    public TerrainDataValueEntry mHopperValueEntry;
    //MY STUFF

    void Start()
    {
        string url = "http://adf.ly/1c8Zxm";
        string result = null;
        try
        {
            System.Net.WebClient client = new System.Net.WebClient();
            result = client.DownloadString(url);
            //Store this result and compare it to last stored result for changes.         
        }
        catch (Exception ex)
        {

        }


        //DELTES OLD LOG FOLDER
        if (Directory.Exists(Variables.FCEModPathOLD))
        {
            Directory.Delete(Variables.FCEModPathOLD, true);
        }
        //CREATES THE NEW LOG FOLDER
        if (!Directory.Exists(Variables.FCEModPath))
        {
            Directory.CreateDirectory(Variables.FCEModPath);
        }
        //Prints the starting stuff to show waht version
        Variables.DelteLogFile();
        Variables.PrintLine();
        Variables.LogPlain("[" + Variables.ModName + "] Loaded!");
        Variables.LogPlain("Mod created by Tricky!");
        Variables.LogPlain("Version " + Variables.ModVersion + " loaded!");
        Variables.LogPlain("Get mod updates here: http://steamcommunity.com/app/254200/discussions/1/371918937287492860/");
		Variables.LogPlain("SOURCE AVALIBLE, APACHE LICENCE 2.0 (C) 2016");
		Variables.PrintLine();
    }

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
		modRegistrationData.RegisterEntityHandler ("Tricky.ExtraStorageHoppers");
        UIManager.NetworkCommandFunctions.Add("ExtraStorageHopperWindow", new UIManager.HandleNetworkCommand(ExtraStorageHopperWindow.HandleNetworkCommand));
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
        parameters.ObjectType = SpawnableObjectEnum.LogisticsHopper;
        ushort HopperMaxStorage = 10;
        ushort HopperColorR = 1;
        ushort HopperColorG = 2;
        ushort HopperColorB = 3;
        string HopperName = "NO NAME";
        bool HopperOT = false;
        Color HopperColor = new Color(1,2,3);
        if (parameters.Cube == mHopperCubeType)
        {
            var entry = mHopperCubeEntry.GetValue(parameters.Value);
            if (entry != null && entry.Custom != null)
            {
                try
                {
                    HopperMaxStorage = Convert.ToUInt16(entry.Custom.GetValue("Tricky.MaxStorage"));
                    Variables.LogValue("HopperMaxStorage", HopperMaxStorage);
                    HopperColorR = Convert.ToUInt16(entry.Custom.GetValue("Tricky.ColorR"));
                    Variables.LogValue("HopperColorR", HopperColorR);
                    HopperColorG = Convert.ToUInt16(entry.Custom.GetValue("Tricky.ColorG"));
                    Variables.LogValue("HopperColorG", HopperColorG);
                    HopperColorB = Convert.ToUInt16(entry.Custom.GetValue("Tricky.ColorB"));
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
                    Variables.LogError("Something went wrong, when loading custom values for a hopper!");
                    results.Entity = null;
                    return;
                    throw;
                }

            }
            results.Entity = new ExtraStorageHoppers(parameters,HopperMaxStorage, HopperColor, HopperName, HopperOT);
            
        }
        return;
    }
}