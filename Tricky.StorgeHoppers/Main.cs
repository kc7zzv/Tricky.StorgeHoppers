using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Net;

class Variables
{
    public static string ModName = "Tricky.ExtraStorageHoppers";
    public static string ModVersion = "7";
    public static bool ModDebug = true;
    public static string FCEModPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ProjectorGames\\FortressCraft\\Mods\\ModLog\\" + ModName + "\\" + ModVersion + "\\";
    public static string LogFilePath = FCEModPath + "ModLog.log";
    public static string PreString = "[" + ModName + "][V" + ModVersion + "][" + System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ":" + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond + "]";
    private static object locker = new object();
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

            throw;
        }

    }
}

public class ExtraStorageHoppersMain : FortressCraftMod
{
    public ushort mHopperCubeType;
    public ushort mHopperCubeType2;
    //MY STUFF

    void Start()
    {

        if (!Directory.Exists(Variables.FCEModPath))
        {
            Directory.CreateDirectory(Variables.FCEModPath);
        }
        Variables.DelteLogFile();
        Variables.PrintLine();
        Variables.LogPlain("[" + Variables.ModName + "] Loaded!");
        Variables.LogPlain("Mod created by Tricky!");
        Variables.LogPlain("Version " + Variables.ModVersion + " loaded!");
        Variables.LogPlain("Get mod updates here: http://steamcommunity.com/app/254200/discussions/1/371918937287492860/");
        Variables.PrintLine();
    }

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("Tricky.ExtraStorageHoppers");
        modRegistrationData.RegisterEntityHandler("Tricky.ExtraStorageHoppers_OT");
        UIManager.NetworkCommandFunctions.Add("ExtraStorageHopperWindow_OT", new UIManager.HandleNetworkCommand(ExtraStorageHopperWindow_OT.HandleNetworkCommand));
        UIManager.NetworkCommandFunctions.Add("ExtraStorageHopperWindow", new UIManager.HandleNetworkCommand(ExtraStorageHopperWindow.HandleNetworkCommand));
        TerrainDataEntry CubeEntry;
        TerrainDataValueEntry EntryValue;
        TerrainData.GetCubeByKey("Tricky.ExtraStorageHoppers", out CubeEntry, out EntryValue);
        if (CubeEntry != null)
        {
            mHopperCubeType = CubeEntry.CubeType;

        }
        TerrainData.GetCubeByKey("Tricky.ExtraStorageHoppers_OT", out CubeEntry, out EntryValue);
        if (CubeEntry != null)
        {
            mHopperCubeType2 = CubeEntry.CubeType;

        }
        return modRegistrationData;
    }

    public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters)
    {
        ModCreateSegmentEntityResults result = new ModCreateSegmentEntityResults();
        if (parameters.Cube == mHopperCubeType)
        {
            result.Entity = new ExtraStorageHoppers(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value, parameters.LoadFromDisk);
        }
        else if (parameters.Cube == mHopperCubeType2)
        {
            result.Entity = new ExtraStorageHoppers_OT(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value, parameters.LoadFromDisk);
        }
        return result;
    }
}