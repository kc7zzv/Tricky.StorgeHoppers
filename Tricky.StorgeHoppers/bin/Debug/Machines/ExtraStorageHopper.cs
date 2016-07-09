using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class ExtraStorageHoppers : MachineEntity, ItemConsumerInterface, StorageMachineInterface, StorageInventoryIterationInterface
{
    /*
	------DONE--------
	Void Hopper MeshText (Saves, and Loads, and counts if items are added, regardless of machine that is adding items. )
	Debug mode (Enable and disable debug.logError, to only affect curtin hoppers, this is good for testing 1 machine in a world with many) - Should work
	Added full debugmode and added all output errors regarding the hopper, to the ModLog! -Should work
	Sort hoppers in ManuFactoring Plant according to Inventory Size. -Working
	Debug mode: Print all hopper information (Position, hopper name, inventory, Exemplar, Hopper Mode, Permission, CubeValue, CubeColor, Hopper Number) - SHould work
	Read - LogError(Read values) NOT STORAGE!
	Write - LogError(Write values) NOT STORAGE!
    Optimise Code
    100% PORT OF OT HOPPERS
	------PENDING-----

	-----NOTES--------
	Overall description:
	Alot of work done on Void Hopper, and a custom debug mode has been implemented.
	If a hopper is not behaving, press CTRL+SHIFT+X on a hopper to enable Debug mode. This can in some cases print new information. 
	All beta versions will have this enabled by default, all else will have it disabled by default. 
	Void hopper now shows how many items has been deleted/Sent to the void
	Debug mode is now done.
	All prep work for the new API system is done.
	Added LastItem deleted for Void Hopper
	Added modlog support for all errors. - This allows for alot more debugging, and indepth bug fixing.
	Optimized code to a point where there is very littel to be done
	*/

    //MY STUFF 
    //private string ModName = Variables.ModName; //this is just mod stuff, dont bother.
    //private string ModVersion = Variables.ModVersion;//this is just mod stuff, dont bother.
    private string PopUpText;
    //STORAGE HOPPER STUFF
    private Segment[] CheckSegments;
    //private Segment[,,] HooverSegment; //This is apparently not in use, so no reason to have it
    public ItemBase[] maItemInventory; //contains all the items stored in the hopper
    public ushort[] maStorage; // contains all the cubes stored in the hopper
    public bool mbAllowLogistics; // this should always be true, to maximize thoughput
    private bool mbForceHoloUpdate; //This entity has a holobase. 
    private bool mbForceTextUpdate; //This eneity shows text in one way or another. 
    public bool mbHooverOn; //Vacuum mode
    //private bool mbLinkedToGO;
    private bool mbShowHopper;
    private GameObject mHoloStatus;
    //private ParticleSystem mHooverPart;
    //private GameObject mHopperPart;
    public string mLastItemAdded; //Last item added, this is used to inform the player
    private int mnHooverEmissionRate;
    private int mnLowFrequencyUpdates;
    private int mnMaxStorage; //How many slots the hopper have. 
    private int mnReadouts;
    private int mnRoundRobinOffset;
    public int mnStorageFree; //The amount of storage free in the hopper. CountFreeSlots()
    public int mnStorageUsed; //The amount of storage used in the hopper. CountFreeSlots()
    static int[] mRemoveCache = new int[100];
    private int mnUpdates;
    public eHopperPermissions mPermissions = eHopperPermissions.AddAndRemove; //Hopper permissions.
    private eHopperPermissions mPreviousPermissions;
    //public float mrCurrentPower; //This is not used for my hoppers, all are free from power.
    //public float mrCurrentTemperature; //My hoppers are not cryo hoppers, and therefore dont use temperature
    public float mrExtractionTime;
    private float mrLogisticsDebounce;
    private float mrMaxLightDistance;
    private float mrPrevDistanceToPlayer;
    public float mrReadoutTick;
    private float mrSpoilTimer; //How much time it takes before organic items spoil. 
    private float mrTimeElapsed;
    private float mrTimeSinceLogistics;
    private float mrTimeUntilPlayerDistanceUpdate;
    bool mbLinkedToGO;
    Light WorkLight;
    TextMesh mTextMesh;
    ParticleSystem mHooverPart;
    GameObject mHopperPart;

    //UI STUFF
    public static bool AllowBuilding = true;

    public static bool AllowInteracting = true;
    public static bool AllowLooking = true;
    public static bool AllowMovement = true;
    public static float ForceNGUIUpdate;
    private float mRetakeDebounce;

    // More of my stuff
    private ushort CubeValue; //This is a custom CubeValue, used to indentify the hoppers 
                              /*
                              *0 = Void hopper
                                  *1 = Nano hopper
                                  *2 = Small hopper
                                  *3 = Medium hopper
                                  *4 = half hopper
                                  *5 = 75 hopper
                                  *6 = 125 hopper
                                  *7 = 150 hopper
                                  *8 = Huge hopper
                                  *9 = 500 slot OT hopper
                                  *10 = 1000 slot OT hopper
                                  *11 = 2000 slot OT hopper
                                  *12 = Custom hopper
                                  */

    private Color mCubeColor; //What color the hoppers will have ingame (NOT ICON)
    private string HopperName; //This is used for the API
    public eHopperMode HopperMode; //Hopper Mode.
    private bool DebugMode = false;
    private int HopperNumber = 0;
    public string ReadStringComplete; //This is used to print what values are read from the disk (not items)
    public string LatestWriteComplete;//This is used to print what values are written to the disk (not items)
                                      //Unique Void hopper stuff
    private int ItemsDeleted; //This is only used for the Void Hopper, as part of the new MeshText
    private string VoidString; //This is only used for the Void hopper, GetPopUpText()
                               //One Type Stuff
    public bool OT = false; //If its a OT hopper, this is true
    public ushort ExemplarBlockID = 0; //Block id, of the cube set by OT hopper
    public ushort ExemplarBlockValue = 0;//Block value, of the cube set by OT hopper
    public int ExemplarItemID = -1; //Item ID of the item set by OT hopper
    public ItemBase ExemplarItemBase; //This is not really used... But in some cases it is, its a kinda strange balance. This is only used to parse items in a specific way.
    public string ExemplarString = "None"; //This is what the items is checked against.
    public bool ExemplarSet = false;

    //******************** STUFF TO DO ********************

    //******************** EXTRA STORAGE HOPPER STUFF ********************
    //******************** EXTRA STORAGE OT HOPPER STUFF ********************
    public ExtraStorageHoppers(MachineEntityCreationParameters parameters, ushort HopperMaxStorage, Color HopperColor, string HopperName, bool HopperOT) : base(parameters)
    {
        this.mnMaxStorage = 200; //This is the base. It was so earlier that no hopper could set its storage higher.
        this.mrExtractionTime = 15f; //Used for Regular hoppers, not mine :D
        this.mrSpoilTimer = 30f; //How long in sec. it takes for a Organic item to spoil
        this.mrMaxLightDistance = 32f; //How far the player needs to be away before the hopper is unloaded
        this.mPreviousPermissions = eHopperPermissions.eNumPermissions; //This is the permission for hoppers, AddAndRemove, RemoveOnly, AddOnly and Locked
        base.mbNeedsLowFrequencyUpdate = true; //LFU is where machines are found and item are handles along other stuff
        base.mbNeedsUnityUpdate = true; //Handles the models, updating MeshText, Hoover Status....
        this.DebugMode = false; //Debug mode, if a hoppers has debug=false, no error messages, that is related to this hopper will be shown.
        this.ItemsDeleted = 0; //This is only used for the void hopper.
        this.OT = false;
        string key = TerrainData.GetCubeKey(parameters.Cube, parameters.Value);
        if (HopperOT)
        {
            if (HopperMaxStorage > 2000)
            {
                HopperMaxStorage = 2000;
            }
        }
        else
        {
            if (HopperMaxStorage > 200)
            {
                HopperMaxStorage = 200;
            }
        }

        //Void hopper. The TrashCan of fortresscraft
        if (key == "Tricky.VoidHopper")
        {
            this.HopperName = "Void Hopper";
            this.CubeValue = 0; //DONT CHANGE THIS! THIS IS VERY IMPORTANT FOR THE VOID HOPPER
            this.mnMaxStorage = 100;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(.01f, .01f, .01f); //BLACK :O
            this.ItemsDeleted = 0;
            this.OT = false;
        }
        else if (key == "Tricky.NanoStorageHopper")
        {
            this.HopperName = "Nano Storage Hopper";
            this.CubeValue = 1;
            this.mnMaxStorage = 1;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(1.2f, 0.1f, 0.1f);
            this.OT = false;
        }
        else if (key == "Tricky.SmallStorageHopper")
        {
            this.HopperName = "Small Storage Hopper";
            this.CubeValue = 2;
            this.mnMaxStorage = 10;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(0.2f, 0.2f, 0.5f);
            this.OT = false;
        }
        else if (key == "Tricky.MediumStorageHopper")
        {
            this.HopperName = "Medium Storage Hopper";
            this.CubeValue = 3;
            this.mnMaxStorage = 25;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(0.5f, 0.2f, 0.2f);
            this.OT = false;
        }
        else if (key == "Tricky.HalfStorageHopper")
        {
            this.HopperName = "Half Storage Hopper";
            this.CubeValue = 4;
            this.mnMaxStorage = 50;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 0.5f);
            this.OT = false;
        }
        else if (key == "Tricky.75SlotStorageHopper")
        {
            this.HopperName = "75 Slot Storage Hopper";
            this.CubeValue = 5;
            this.mnMaxStorage = 75;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(0.2f, 1.2f, 0.2f);
            this.OT = false;
        }
        else if (key == "Tricky.125SlotStorageHopper")
        {
            this.HopperName = "125 Slot Storage Hopper";
            this.CubeValue = 6;
            this.mnMaxStorage = 125;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(0.2f, 0.5f, 0.2f);
            this.OT = false;
        }
        else if (key == "Tricky.150SlotStorageHopper")
        {
            this.HopperName = "150 Slot Storage Hopper";
            this.CubeValue = 7;
            this.mnMaxStorage = 150;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 0.5f);
            this.OT = false;
        }
        else if (key == "Tricky.HugeStorageHopper")
        {
            this.HopperName = "Huge Storage Hopper";
            this.CubeValue = 8;
            this.mnMaxStorage = 200;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(0.2f, 0.2f, 1.5f);
            this.OT = false;
        }
        //******************** OT HOPPERS **********************
        else if (key == "Tricky.2000SlotHopperx")
        {
            this.HopperName = "2000 Slot OT Storage Hopper";
            this.CubeValue = 9;
            this.mnMaxStorage = 2000;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 0.5f);
            this.OT = true;
        }
        else if (key == "Tricky.1000SlotHopperx")
        {
            this.HopperName = "1000 Slot OT Storage Hopper";
            this.CubeValue = 10;
            this.mnMaxStorage = 1000;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 1.0f);
            this.OT = true;
        }
        else if (key == "Tricky.500SlotHopperx")
        {
            this.HopperName = "500 Slot OT Storage Hopper";
            this.CubeValue = 11;
            this.mnMaxStorage = 500;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 1.5f);
            this.OT = true;
        }
        //******************** CUSTOM HOPPERS **********************
        else {
            //This is where custom hoppers are loaded. All custom hoppers have Cubevalue 9
            this.HopperName = HopperName; //Name of the hopper. (Hotbar, GetPopUpText)
            this.CubeValue = 12;
            this.mnMaxStorage = HopperMaxStorage; //This is the amount of storage this hopper have. 
            this.mrLogisticsDebounce = 0f; //Sets the debounce to 0... If this isn't 0, there might be something wrong
            this.mCubeColor = HopperColor; //This sets the cubes color ingame. 
            this.OT = HopperOT;
        }
        //******************** OTHER HOPPER STUFF **********************
        this.maStorage = new ushort[this.mnMaxStorage]; //The array that contains all the items, Dont use this!
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            this.maStorage[i] = 0;
        }
        this.maItemInventory = new ItemBase[this.mnMaxStorage];
        this.CountFreeSlots();
        this.CheckSegments = new Segment[6];
    }
    //******************** VOID HOPPER FEED HIVEMIND ********************
    private HiveEntity mHive;
    public bool FeedHiveMind = false;
    private int FeedTicker = 0;
    private bool SearchForHivemind()
    {
        if (CentralPowerHub.mClosestHive != null)
        {
            this.mHive = CentralPowerHub.mClosestHive;
            return true;
        }
        return false;
    }
    //******************** HOPPER NUMBER ********************
    public void IncreaseHopperNum()
    {

        SetHopperNum(GetHopperNum() + 1);
    }

    public int GetHopperNum()
    {
        return Variables.HopperNumber;
    }

    public void SetHopperNum(int num)
    {
        this.LogError("HopperNumber set/was: (" + num + "," + GetHopperNum() + ")");
        Variables.HopperNumber = num;
    }

    public void SetHopperNumIfHigher(int num)
    {
        if (num > Variables.HopperNumber)
        {
            SetHopperNum(num);
        }
    }
    //******************** OT Hopper Stuff ********************
    public void SetExemplar(ItemBase lExemplar)
    {
        this.ExemplarString = ItemManager.GetItemName(lExemplar);
        if (lExemplar.mnItemID != -1)
        {
            this.ExemplarItemID = lExemplar.mnItemID;
            if (this.ExemplarItemID == 0)
            {
                Debug.LogError("Error, Exemplar attempted to be set, but no ItemID? " + ItemManager.GetItemName(lExemplar));
            }
            this.ExemplarBlockValue = 0;
            this.ExemplarBlockID = 0;
        }
        else if (lExemplar.mType == ItemType.ItemCubeStack)
        {
            ushort CubeType;
            ushort CubeValue;
            TerrainData.GetCubeForName(this.ExemplarString, out CubeType, out CubeValue);
            this.ExemplarBlockID = CubeType;
            this.ExemplarBlockValue = CubeValue;
            this.ExemplarItemID = -1;
        }
        else
        {
            Debug.LogWarning("Error, unable to set exemplars for type " + ItemManager.GetItemName(lExemplar));
        }
        this.ExemplarItemBase = lExemplar;
        this.ExemplarSet = true;
        this.MarkDirtyDelayed();
    }

    /// <summary>
    /// Will check if the Itembase given, it the same as the Item set in exemplar
    /// </summary>
    /// <returns><c>true</c>, if they are the same, <c>false</c>if they are not</returns>
    /// <param name="lExemplar">Item to check against</param>
    public bool CheckExemplar(ItemBase lExemplar)
    {
        LogError("CheckExemplar Item was called");
        if (this.OT == false)
        {
            LogError("It's not an OT hopper...");
            return true;

        }
        if (ItemManager.GetItemName(lExemplar) == this.ExemplarString)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckExemplar(ushort CubeType, ushort CubeValue)
    {
        LogError("CheckExemplar Cube was called");
        if (this.OT == false)
        {
            LogError("It's not an OT hopper...");
            return true;

        }
        ItemBase lExemplar = new ItemCubeStack(CubeType, CubeValue, 1);
        if (ItemManager.GetItemName(lExemplar) == this.ExemplarString)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PrintExemplar()
    {
        LogError("Cube: (" + ExemplarBlockID + "," + ExemplarBlockValue + ")");
        LogError("Item: (" + ExemplarItemID + ")");
        LogError("String: (" + ExemplarString + ")");
    }
    //******************** DEBUG MODE HANDLER **********************
    //This is what handles the custom debug mode, that allows for specific hopper debugging.
    public void ToggleDebugMode()
    {
        DebugMode = !DebugMode;
        if (DebugMode == true)
        {
            LogError("Debug Mode has been enabled for this storage hopper!");
            LogError(GetHopperPosition());
        }
    }

    public bool GetDebugMode()
    {
        return this.DebugMode;
    }

    public string GetDebugModeStr()
    {
        if (GetDebugMode())
        {
            return "True";
        }
        else {
            return "False";
        }
    }
    //******************** Return information about hopper **********************
    public void PrintStorageInfo()
    {
        LogError("Storage Used: " + this.mnStorageUsed + " / " + this.mnMaxStorage);
        LogError("Storage Free: " + this.mnStorageFree + " / " + this.mnMaxStorage);
    }

    public string PrintBoolean(bool statement)
    {
        if (statement)
        {
            return "True";
        }
        else {
            return "False";
        }
    }

    public string GetHopperPosition()
    {
        return "(" + (base.mnX - WorldScript.mDefaultOffset) + "," + (base.mnY - WorldScript.mDefaultOffset) + "," + (base.mnZ - WorldScript.mDefaultOffset) + ")";
    }
    //Print all hopper information (Position, hopper name, inventory, Exemplar, Hopper Mode, Permission, CubeValue, CubeColor, Hopper Number)
    public void PrintAllHopperInfo()
    {
        Variables.PrintLine();
        Variables.Log("Every info for a hopper was printed");
        Variables.PrintLine();
        LogError("Hopper POS [X,Y,Z]: " + GetHopperPosition());
        LogError("Hopper Name: " + this.HopperName);
        LogError("Hopper OT: " + PrintBoolean(this.OT));
        PrintStorageInfo();
        PrintExemplar();
        LogError("Hopper Permission: " + this.mPermissions);
        LogError("Hopper CubeValue: " + this.CubeValue);
        LogError("Hopper Color: " + this.mCubeColor.ToString());
        LogError("Hopper Number: " + this.HopperNumber);
        Variables.PrintLine();
    }

    public void PrintLastWrite()
    {
        LogError(this.LatestWriteComplete);
    }

    public void PrintRead()
    {
        LogError(this.ReadStringComplete);
    }
    //******************** CHOOSE WHAT MODEL THE HOPPER HAS INGAME **********************

    public override void SpawnGameObject()
    {
        this.HopperNumber = GetHopperNum();
        IncreaseHopperNum();
        mObjectType = SpawnableObjectEnum.LogisticsHopper;
        base.SpawnGameObject();
    }

    //******************** EXTRA MODDING INTERFACES **********************

    public override global::HoloMachineEntity CreateHolobaseEntity(global::Holobase holobase)
    {
        global::HolobaseEntityCreationParameters holobaseEntityCreationParameters = new global::HolobaseEntityCreationParameters(this);
        global::HolobaseVisualisationParameters holobaseVisualisationParameters = holobaseEntityCreationParameters.AddVisualisation(holobase.mPreviewCube);
        holobaseVisualisationParameters.Color = mCubeColor;
        return holobase.CreateHolobaseEntity(holobaseEntityCreationParameters);
    }

    public override string GetPopupText()
    {
        this.mRetakeDebounce -= Time.deltaTime;
        ushort SelectedBlock = WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectBlockType;
        if ((SelectedBlock == GetCubeType("Tricky.ExtraStorageHoppers")))
        {
            ExtraStorageHoppers selectedEntity = (ExtraStorageHoppers)WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity;
            if (selectedEntity != null)
            {

                if (this.CubeValue == 0)
                {
                    VoidString = "Void Hopper\nItems Deleted: " + this.ItemsDeleted + "\nLast Item: " + selectedEntity.mLastItemAdded + "\nFeed Hive Mind (Q):"+PrintBoolean(selectedEntity.FeedHiveMind)+"\nDebugMode: " + GetDebugModeStr();
                }
                string lStr = string.Empty;
                lStr = this.HopperName;
                if (selectedEntity.mLastItemAdded != null)
                {
                    lStr = lStr + "\nLast Item : " + selectedEntity.mLastItemAdded;
                }
                string str2 = lStr;
                if (this.CubeValue != 0)
                {
                    object[] objArray1 = new object[] { str2, "\nUsed:", selectedEntity.mnStorageUsed, ". Free:", selectedEntity.mnStorageFree, "\n(E)Toggle Lock Status : ", selectedEntity.mPermissions };

                    lStr = string.Concat(objArray1);
                    lStr = lStr + "\n(Shift+E)Toggle Vacuum Status : " + (!selectedEntity.mbHooverOn ? "Off" : "On");
                }
                int lnAvailable = 0;
                ItemBase currentHotBarItemOrCubeAsItem = this.GetCurrentHotBarItemOrCubeAsItem(out lnAvailable, true);
                //WILL NOT BE SET TO NULL, IF THERE IS NO ITEM AMOUNTw
                ItemBase currentHotBarItemOrCubeAsItem_NN = this.GetCurrentHotBarItemOrCubeAsItem(out lnAvailable, true);
                if (lnAvailable == 0)
                {
                    currentHotBarItemOrCubeAsItem = null;
                }
                if (((currentHotBarItemOrCubeAsItem != null) && (currentHotBarItemOrCubeAsItem.mType != ItemType.ItemCubeStack)) && (currentHotBarItemOrCubeAsItem.mnItemID == -1))
                {
                    currentHotBarItemOrCubeAsItem = null;
                }
                if (currentHotBarItemOrCubeAsItem != null)
                {
                    int amount = lnAvailable;
                    int mnStorageFree = selectedEntity.mnStorageFree;
                    if (Input.GetKey(KeyCode.LeftShift) && (amount > 10))
                    {
                        amount = 10;
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && (amount > 1))
                    {
                        amount = 1;
                    }
                    if (amount > mnStorageFree)
                    {
                        amount = mnStorageFree;
                    }
                    int maxStackSize = ItemManager.GetMaxStackSize(currentHotBarItemOrCubeAsItem);
                    if (amount > selectedEntity.mnStorageFree)
                    {
                        amount = selectedEntity.mnStorageFree;
                    }
                    ItemManager.SetItemCount(currentHotBarItemOrCubeAsItem, amount);
                    if (this.CubeValue == 0)
                    {
                        if (amount != 0)
                        {
                            lStr = lStr + "\n(T) to send " + currentHotBarItemOrCubeAsItem.GetDisplayString() + " to the void";
                        }

                    }
                    else
                    {
                        if (amount != 0)
                        {
                            lStr = lStr + "\n(T) to store " + currentHotBarItemOrCubeAsItem.GetDisplayString();
                        }
                        else
                        {
                            lStr = lStr + "\nHopper full!";
                        }
                    }
                }
                if (selectedEntity.mnStorageUsed > 0)
                {
                    lStr = lStr + "\n(Q) to retrieve contents!";

                }
                //lStr = lStr + "\n(Shift+X) Hopper Mode: " + this.GetStorageHopperMode(); //This is no longer in use
                if (NetworkManager.mbClientRunning)
                {
                    lStr = lStr + "\nNetworkSync : " + selectedEntity.mrNetworkSyncTimer.ToString("F2");
                }
                //If its an OT hopper, show what item is set at the type
                if (selectedEntity.OT)
                {
                    lStr = lStr + "\n(Shift+X)Set Storage Type : [" + (selectedEntity.ExemplarString) + "]";
                }
                //Add what mode Debug is set to
                lStr = lStr + "\nDebugMode: " + GetDebugModeStr();

                PopUpText = lStr;
                //If its a void hopper, use a different string
                if (selectedEntity.CubeValue == 0)
                {
                    PopUpText = VoidString;
                }
                //If debug mode is enabled, add the position of the hopper at the start of the text
                if (GetDebugMode())
                {
                    PopUpText = "Hopper Number:" + selectedEntity.HopperNumber + "\n" + GetHopperPosition() + "\n" + PopUpText;
                }
                if (AllowMovement)
                {
                    //SHoudl only work if its an OT hopper
                    if (selectedEntity.OT)
                    {
                        if ((Input.GetButtonDown("Build Gun") && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl) && currentHotBarItemOrCubeAsItem_NN != null))
                        {
                            if (this.mnStorageUsed == 0)
                            {
                                //SetExemplar(currentHotBarItemOrCubeAsItem);
                                ExtraStorageHopperWindow.SetNewExamplar(WorldScript.mLocalPlayer, selectedEntity, currentHotBarItemOrCubeAsItem_NN);
                            }
                            else
                            {
                                ExtraStorageHopperWindow.SetNewExamplar_Fail(WorldScript.mLocalPlayer, selectedEntity);
                            }
                            ForceNGUIUpdate = 0.1f;
                            AudioHUDManager.instance.HUDIn();
                        }
                    }

                    //KEY.Q, TAKE ITEMS, INCLUDED CUBES
                    if ((Input.GetButton("Extract")) && selectedEntity.CubeValue != 0 && ExtraStorageHopperWindow.TakeItems(WorldScript.mLocalPlayer, selectedEntity))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.OrePickup();
                        if ((WorldScript.meGameMode == eGameMode.eSurvival) && (SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.RemoveCoalFromHopper))
                        {
                            SurvivalPlayerScript.TutorialSectionComplete();
                        }
                        if (selectedEntity.CubeValue == 0)
                        {
                            selectedEntity.mRetakeDebounce = 1f;
                        }

                    }
                    if ((Input.GetButtonDown("Extract")) && selectedEntity.CubeValue == 0 && (AllowInteracting && ExtraStorageHopperWindow.TakeItems(WorldScript.mLocalPlayer, selectedEntity)))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.OrePickup();
                        if (selectedEntity.CubeValue == 0)
                        {
                            selectedEntity.mRetakeDebounce = 1f;
                        }

                    }
                    //UPDATE HOPPER PERMISSIONS, ADDANDREMOVE, REMOVEONLY, ADDONLY, LOCKED
                    if ((Input.GetButtonDown("Interact") && !Input.GetKey(KeyCode.LeftShift)) && (AllowInteracting && ExtraStorageHopperWindow.TogglePermissions(WorldScript.mLocalPlayer, selectedEntity)))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                        AudioSpeechManager.instance.UpdateStorageHopper(selectedEntity.mPermissions);
                    }
                    // KEY.E + KEY.SHIFT, TOGGLE HOOVER MODE (VACUUM)
                    if ((Input.GetButtonDown("Interact") && Input.GetKey(KeyCode.LeftShift)) && (AllowInteracting && ExtraStorageHopperWindow.ToggleHoover(WorldScript.mLocalPlayer, selectedEntity)))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                    }
                    //CHANGE DEBUG MODE! *NEW
                    if (((Input.GetButtonDown("Build Gun") && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) && AllowInteracting) && ((selectedEntity.mnStorageFree > 0) && (currentHotBarItemOrCubeAsItem != null)))
                    {
                        ExtraStorageHopperWindow.DebugMode(WorldScript.mLocalPlayer, selectedEntity);
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                    }
                    // KEY.T Insert items and cubes. 
                    if ((Input.GetButtonDown("Store") && AllowInteracting) && ((selectedEntity.mnStorageFree > 0) && (currentHotBarItemOrCubeAsItem != null)))
                    {

                        if (this.OT && (this.ExemplarBlockID == 0 && this.ExemplarItemID == -1) && this.mnStorageUsed == 0)
                        {
                            ExtraStorageHopperWindow.SetNewExamplar(WorldScript.mLocalPlayer, selectedEntity, currentHotBarItemOrCubeAsItem);
                        }
                        if (ItemManager.GetItemName(currentHotBarItemOrCubeAsItem) != this.ExemplarString && this.OT)
                        {
                            Variables.LogError("You cannot insert this into an OT hopper, Check the Exemplar!");
                        }
                        else
                        {


                            if (ExtraStorageHopperWindow.StoreItems(WorldScript.mLocalPlayer, selectedEntity, currentHotBarItemOrCubeAsItem))
                            {
                                UnityEngine.Debug.Log("Inserted " + currentHotBarItemOrCubeAsItem);
                                ForceNGUIUpdate = 0.1f;
                                AudioHUDManager.instance.HUDOut();
                                SurvivalHotBarManager.MarkAsDirty();
                                SurvivalHotBarManager.MarkContentDirty();
                            }
                            else
                            {
                                Variables.LogError("Failed to insert " + currentHotBarItemOrCubeAsItem);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            this.mRetakeDebounce = 0f;
        }

        return PopUpText;
    }

    public override void LowFrequencyUpdate()
    {
        //-------------------------------------------------------------
        //HFCHECKER - CUSTOM EXTRASTORAGEHOPPERCODE, THIS IS WHAT MAKES MY HOPPERS FASTER... along other things :P 
        long mnX1 = base.mnX;
        long mnY1 = base.mnY;
        long mnZ1 = base.mnZ;
        int index2 = 0;
        mnX1 -= 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;
        index2 = 1;
        mnX1 += 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;
        index2 = 2;
        mnY1 -= 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;
        index2 = 3;
        mnY1 += 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;
        index2 = 4;
        mnZ1 -= 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;
        index2 = 5;
        mnZ1 += 1L;
        HFChecker(index2, mnX1, mnY1, mnZ1);
        mnX1 = base.mnX;
        mnY1 = base.mnY;
        mnZ1 = base.mnZ;

        //-------------------------------------------------------------
        mbAllowLogistics = true;
        //This makes a shockingly small amount of difference!



        mrTimeUntilPlayerDistanceUpdate -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        if (mrTimeUntilPlayerDistanceUpdate < 0)
        {
            mrPrevDistanceToPlayer = mDistanceToPlayer;
            UpdatePlayerDistanceInfo();
            mrTimeUntilPlayerDistanceUpdate = mDistanceToPlayer / 30;//1 second at 30 metres, .5 at 15 metres (halved from before)
            if (mrTimeUntilPlayerDistanceUpdate > 2) mrTimeUntilPlayerDistanceUpdate = 2;
        }

        mnLowFrequencyUpdates++;
        //These are not used... My hoppers should not use the vacuum function
        //UpdateHoover();

        //UpdatePoweredHopper();//Keep the client sim in step
        //This means that if its a server, the client is not responsible for spoiling stuff
        if (WorldScript.mbIsServer == true)//this is done by the server
        {
            UpdateSpoilage();
        }

        /*mrCurrentPower += mrIsotopeRate * GameManager.mrPreviousUpdateTimeStep;
		mrCurrentPower += mrSolarEfficiency * GameManager.mrPreviousUpdateTimeStep * Mathf.Abs(SurvivalWeatherManager.mrSunAngle);
		if(mrCurrentPower > mrMaxPower) mrCurrentPower = mrMaxPower;*/


        mrReadoutTick -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        //	Debug.Log(mrReadoutTick+"::" + GameManager.mrPreviousUpdateTimeStep.ToString("F2"));
        if (mrReadoutTick < 0.0f)
        {
            mrReadoutTick = 1.0f;
            mnReadouts++;

            //UnityEngine.Debug.Log("SH checking on update "+ mnReadouts + " with " + mnStorageUsed + " storage used");
            //See if anything adjacent wants some of our resources
            //	if (mnStorageUsed > 0)//removed as we both give and take resources
            {

                long checkX = this.mnX;
                long checkY = this.mnY;
                long checkZ = this.mnZ;

                int lnWhich = mnReadouts % 6;

                if (lnWhich == 0) checkX--;
                if (lnWhich == 1) checkX++;
                if (lnWhich == 2) checkY--;
                if (lnWhich == 3) checkY++;
                if (lnWhich == 4) checkZ--;
                if (lnWhich == 5) checkZ++;

                if (CheckSegments[lnWhich] == null)
                {
                    CheckSegments[lnWhich] = AttemptGetSegment(checkX, checkY, checkZ);
                    return;
                }

                if (CheckSegments[lnWhich].mbDestroyed || CheckSegments[lnWhich].mbInitialGenerationComplete == false)
                {
                    CheckSegments[lnWhich] = null;
                    return;
                }


                ushort lCube = CheckSegments[lnWhich].GetCube(checkX, checkY, checkZ);

                if (CubeHelper.HasEntity(lCube) == false) return;//don't bother, it's not a machine entity

                // machines that give us resources
                if (mnStorageFree > 0) CheckSuppliers(CheckSegments[lnWhich], lCube, checkX, checkY, checkZ);

                // machines that take our resources
                if (mnStorageUsed > 0) CheckConsumers(CheckSegments[lnWhich], lCube, checkX, checkY, checkZ);

                //if  (mnReadouts % mnHopperLoops == 0)
                {
                    //both directions
                    //CheckHoppers(checkX, checkY, checkZ, CheckSegments[lnWhich], lCube);
                }
            }
        }

        //Look for adjacent places to transfer storage
        //Maybe only do this if we have power? Maybe.

    }

    public override void OnDelete()
    {
        Variables.Log(this.HopperName + " Was deleted at " + GetHopperPosition());
        if (WorldScript.mbIsServer)
        {
            System.Random random = new System.Random();
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if (this.maStorage[i] != 0)
                {
                    Vector3 velocity = new Vector3(((float)random.NextDouble()) - 0.5f, ((float)random.NextDouble()) - 0.5f, ((float)random.NextDouble()) - 0.5f);
                    ItemManager.DropNewCubeStack(this.maStorage[i], TerrainData.GetDefaultValue(this.maStorage[i]), 1, base.mnX, base.mnY, base.mnZ, velocity);
                }
                if (this.maItemInventory[i] != null)
                {
                    Vector3 vector2 = new Vector3(((float)random.NextDouble()) - 0.5f, ((float)random.NextDouble()) - 0.5f, ((float)random.NextDouble()) - 0.5f);
                    if (this.maItemInventory[i].mType == ItemType.ItemCubeStack)
                    {
                        Debug.LogWarning(string.Concat(new object[] { "Dropping ", i, ". Count", (this.maItemInventory[i] as ItemCubeStack).mnAmount }));
                    }
                    ItemManager.instance.DropItem(this.maItemInventory[i], base.mnX, base.mnY, base.mnZ, vector2);
                    this.maItemInventory[i] = null;
                }
            }
        }
    }

    public override bool ShouldNetworkUpdate()
    {
        return true;
    }

    public override bool ShouldSave()
    {
        return true;
    }

    public override void UnitySuspended()
    {
        this.WorkLight = null;
        this.mHooverPart = null;
        this.mTextMesh = null;
        this.mHopperPart = null;
        this.DropGameObject(); //TEST TO SEE IF IT IMPROVES GPU USAGE
    }

    public override void UnityUpdate()
    {
        //------------------------------------
        this.mrTimeElapsed += Time.deltaTime;
        if (!this.mbLinkedToGO)
        {
            this.LinkToGO();
        }
        else
        {
            if (!base.mbWellBehindPlayer && !base.mSegment.mbOutOfView)
            {
                if ((base.mDistanceToPlayer <= 8f) && (this.mrPrevDistanceToPlayer > 8f))
                {
                    this.mbForceHoloUpdate = true;
                }
                if ((base.mDistanceToPlayer > 8f) && (this.mrPrevDistanceToPlayer <= 8f))
                {
                    this.mbForceHoloUpdate = true;
                }
                if (this.mbForceHoloUpdate)
                {
                    this.SetHoloStatus();
                }
                if (this.mbForceTextUpdate)
                {
                    this.UpdateMeshText();
                }
            }
            this.mnUpdates++;
            this.UpdateLOD();
            this.UpdateWorkLight();
        }
    }

    //******************** HANDLE MODELS ********************
    void UpdateLOD()
    {

        bool lbShowHopper = true;
        if (mDistanceToPlayer > 64) lbShowHopper = false;
        if (mbWellBehindPlayer) lbShowHopper = false;
        if (Mathf.Abs(mVectorToPlayer.y) > 32.0f) lbShowHopper = false;
        if (mSegment.mbOutOfView) lbShowHopper = false;


        //Smartly do this work only when the vis has changed
        if (lbShowHopper != mbShowHopper)
        {
            mbShowHopper = lbShowHopper;
            mbForceHoloUpdate = true;//Ensure the vis is correct for this
            if (lbShowHopper == true)
            {
                if (mHopperPart.activeSelf == false)
                {
                    mHopperPart.SetActive(true);

                    mHopperPart.GetComponent<Renderer>().enabled = true;
                }
            }
            else
            {
                if (mHopperPart.activeSelf == true)
                {
                    mHopperPart.SetActive(false);
                }
                mHopperPart.GetComponent<Renderer>().enabled = false;//hopefully this overrides the LOD stuff - I wish everything used LODs and culled at the same point :/
                                                                     //But if we did that, it's every frame on the main thread and fuck unity in it's stupid face
            }

        }


        UpdateHooverEmission();

        //disable text at unreadable distances
        if (mDistanceToPlayer > 24.0f || mbWellBehindPlayer || mDistanceToPlayer > CamDetail.SegmentDrawDistance - 8)
        {
            if (mTextMesh.GetComponent<Renderer>().enabled == true)
            {
                mbForceHoloUpdate = true;
                mTextMesh.GetComponent<Renderer>().enabled = false;
            }
        }
        else
        {
            if (mTextMesh.GetComponent<Renderer>().enabled == false)
            {
                mbForceHoloUpdate = true;
                mTextMesh.GetComponent<Renderer>().enabled = true;
            }
        }



    }

    void UpdateHooverEmission()
    {

        bool lbAllowHoover = true;

        if (mDistanceToPlayer > 16.0f || mbWellBehindPlayer) lbAllowHoover = false;
        if (mDistanceToPlayer > (CamDetail.SegmentDrawDistance) || Mathf.Abs(mVectorToPlayer.y) > 24.0f || mbWellBehindPlayer) lbAllowHoover = false;

        if (lbAllowHoover == false)
        {
            if (mnHooverEmissionRate > 0)
            {
                mnHooverEmissionRate--;
                mHooverPart.emissionRate = mnHooverEmissionRate;
            }

            //mHooverPart.gameObject.SetActive(false);
        }
        else
        {
            if (mbHooverOn)
            {

                if (mnHooverEmissionRate <= 10)
                {
                    mnHooverEmissionRate++;
                    mHooverPart.emissionRate = mnHooverEmissionRate;
                }
            }
            else
            {
                if (mnHooverEmissionRate > 0)
                {
                    mnHooverEmissionRate--;
                    mHooverPart.emissionRate = mnHooverEmissionRate;
                }

            }
            //mHooverPart.gameObject.SetActive(true);
        }
    }

    void UpdateWorkLight()
    {

        bool lbLightShouldBeEnabled = false;
        if (mnStorageUsed == 0) lbLightShouldBeEnabled = true;
        if (mnStorageFree == 0) lbLightShouldBeEnabled = true;

        if (mbWellBehindPlayer) lbLightShouldBeEnabled = false;

        mrMaxLightDistance += (CamDetail.FPS - mrMaxLightDistance) * Time.deltaTime * 0.1f;//very slow lerp based on framerate
        if (mrMaxLightDistance < 2) mrMaxLightDistance = 2;
        if (mrMaxLightDistance > 64) mrMaxLightDistance = 64;

        if (mDistanceToPlayer > mrMaxLightDistance) lbLightShouldBeEnabled = false;//I wonder if this could be a very smoothed number that follows the framerate...?


        if (lbLightShouldBeEnabled)
        {
            if (WorkLight.enabled == false)
            {
                WorkLight.enabled = true;
                WorkLight.range = 0.05f;//fade up, don't just appear.(this is the minimum not to get immediately turned off)
            }
            //Not a Cryo hopper, simply show full/empty as green/red
            if (mnStorageUsed == 0)
            {
                WorkLight.color = Color.Lerp(WorkLight.color, Color.green, Time.deltaTime);//1 second to smoothly lerp? Maybe?
                WorkLight.range += 0.1f;
            }
            else
            {
                if (mnStorageFree == 0)
                {
                    WorkLight.color = Color.Lerp(WorkLight.color, Color.red, Time.deltaTime);//1 second to smoothly lerp? Maybe?	
                    WorkLight.range += 0.1f;

                }
                else
                {
                    //this should probably actually be lbLightShouldBeEnabled = false
                    WorkLight.color = Color.Lerp(WorkLight.color, Color.cyan, Time.deltaTime);//1 second to smoothly lerp? Maybe?
                    WorkLight.range -= 0.1f;//That's ok, you're working nicely, don't need to indicate
                }
            }


            if (WorkLight.range > 1.0f) WorkLight.range = 1.0f;//this is automatically reduced to 95% a bit below here, so it's technically oscillating a bit
        }
        else
        {
            //Do not disable light directly; light will fade rapidly and disable itself (Light does not need to be disabled, for whatever reason)
        }

        if (WorkLight.enabled)
        {
            if (WorkLight.range < 0.15f)
            {
                WorkLight.enabled = false;//maybe as much as .2? (turning this off sooner gives performance back sooner)
            }
            else
            {
                WorkLight.range *= 0.95f;
            }
        }
    }

    void SetHoloStatus()
    {
        if (mHoloStatus == null) return;//I fail

        if (mbForceHoloUpdate) mPreviousPermissions = eHopperPermissions.eNumPermissions;
        mbForceHoloUpdate = false;

        if (mDistanceToPlayer > 8 || mbWellBehindPlayer)
        {
            if (mHoloStatus.activeSelf) mHoloStatus.SetActive(false);
            return;
        }
        if (mHoloStatus.activeSelf == false)
        {
            mPreviousPermissions = eHopperPermissions.eNumPermissions;
            mHoloStatus.SetActive(true);
        }

        if (mPreviousPermissions != mPermissions)
        {
            mPreviousPermissions = mPermissions;//only do this if the permissions have changed
                                                //cache this if necessary
            if (mPermissions == eHopperPermissions.AddAndRemove) mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, 0.5f);
            if (mPermissions == eHopperPermissions.RemoveOnly) mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, 0.0f);
            if (mPermissions == eHopperPermissions.Locked) mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f, 0.0f);
            if (mPermissions == eHopperPermissions.AddOnly) mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f, 0.5f);
        }
    }

    void UpdateMeshText()
    {
        if (mTextMesh.GetComponent<Renderer>().enabled && mDistanceToPlayer < 12 && this.CubeValue == 0)
        {

            mTextMesh.text = "Void Hopper\nDeleted: " + ItemsDeleted + "\n[" + mLastItemAdded + "]";
            mbForceTextUpdate = false;
        }
        else if (mTextMesh.GetComponent<Renderer>().enabled && mDistanceToPlayer < 12)//we can see text up to 24m right now, but there's little point, it's not readable
        {
            string lText = mPermissions.ToString() + "\n";

            if (mnStorageFree == 0) lText += "Storage full\n";
            else
                if (mnStorageUsed == 0) lText += "Storage Empty\n";
            else
                lText += mnStorageFree.ToString() + " free slots\n";//

            if (mrTimeSinceLogistics > 0.0f)
                lText += "Processing...";
            else
                lText += "[" + mLastItemAdded + "]";

            mTextMesh.text = lText;

            mbForceTextUpdate = false;
        }
    }

    void LinkToGO()
    {
        if (mWrapper == null || !mWrapper.mbHasGameObject)
        {
            return;
        }
        else
        {
            if (mWrapper.mGameObjectList == null) LogError("Ore Extractor missing game object #0?");
            if (mWrapper.mGameObjectList[0].gameObject == null) LogError("Ore Extractor missing game object #0 (GO)?");
            //WorkLight = mWrapper.mGameObjectList[0].gameObject.GetComponentInChildren<Light>();
            WorkLight = mWrapper.mGameObjectList[0].transform.Search("HooverGraphic").GetComponent<Light>();
            if (WorkLight == null) LogError("Storage Hopper has missing light?");
            //if (QualitySettings.GetQualityLevel() >=3) WorkLight.shadows = LightShadows.Hard;//Object casts shadows and we're inside the object. Sad times.

            mHooverPart = mWrapper.mGameObjectList[0].transform.Search("HooverGraphic").GetComponent<ParticleSystem>();
            mHooverPart.emissionRate = 0;
            //mHooverPart= mWrapper.mGameObjectList[0].gameObject.GetComponentInChildren<ParticleSystem>();

            mTextMesh = mWrapper.mGameObjectList[0].gameObject.transform.Search("Storage Text").GetComponent<TextMesh>();//this is like HUNDREDS of times faster than the below call..
                                                                                                                         //.GetComponentInChildren<TextMesh>();s
            mHopperPart = mWrapper.mGameObjectList[0].transform.Find("Hopper").gameObject;

            mHoloStatus = mHopperPart.transform.Find("Holo_Status").gameObject;

            mHoloStatus.SetActive(false);
            mPreviousPermissions = eHopperPermissions.eNumPermissions;//force an update
            SetHoloStatus();

            mbForceTextUpdate = true;

            mbLinkedToGO = true;

            //mHoloMPB = new MaterialPropertyBlock();
            // COLOR STORAGE HOPPER

            MeshRenderer Render2 = this.mHopperPart.GetComponent<MeshRenderer>();
            Render2.material.SetColor("_Color", this.mCubeColor);
        }
    }

    //******************** CUSTMO DEBUG MODE ********************
    //Only log error, if debug is enabled!
    public void LogError(string Error)
    {
        if (this.GetDebugMode())
        {
            LogError_AW(Error);
        }
    }
    //Allways logs an error, regardless of debugmode
    public static void LogError_AW(string Error)
    {
        Debug.LogError(Error);
        Variables.LogError(Error);
    }

    public override void DropGameObject()
    {
        base.DropGameObject();
        mbLinkedToGO = false;
    }

    //******************** FILL/EMPTY HOPPER MODE **********************
    //NO LONGER IN USE...
    //Brilliant function though...
    public void ToggleStorageHopperMode()
    {
        this.HopperMode += 1;
        if (this.HopperMode == eHopperMode.eNum)
        {
            this.HopperMode = eHopperMode.eNone;
        }
        this.MarkDirtyDelayed();
        this.mbForceTextUpdate = true;
        this.mbForceHoloUpdate = true;
        this.RequestImmediateNetworkUpdate();

        FloatingCombatTextManager.instance.QueueText(base.mnX, base.mnY + 1L, base.mnZ, 1f, GetStorageHopperMode(), Color.green, 1.5f).mrStartRadiusRand = 0.25f;
    }

    public string GetStorageHopperMode()
    {
        if (this.HopperMode == eHopperMode.eNone)
        {
            return "None";
        }
        else if (this.HopperMode == eHopperMode.eEmpty)
        {
            return "Empty hoppers";
        }
        else if (this.HopperMode == eHopperMode.eFill)
        {
            return "Fill hoppers";
        }
        else {
            return "";
        }
    }

    //******************** HF CHECKER AND CUSUMER COMMENTED **********************

    public void HFChecker(int index, long x, long y, long z)
    {
        try
        {
            if (this.CheckSegments[index] == null)
            {
                this.CheckSegments[index] = base.AttemptGetSegment(x, y, z);
            }
            else if (this.CheckSegments[index].mbDestroyed || !this.CheckSegments[index].mbInitialGenerationComplete)
            {
                this.CheckSegments[index] = null;
            }
            else
            {
                ushort lType = this.CheckSegments[index].GetCube(x, y, z);
                if (CubeHelper.HasEntity(lType))
                {
                    if (this.mnStorageUsed > 0)
                    {
                        this.HFCheckConsumers(this.CheckSegments[index], lType, x, y, z);
                    }
                }
            }
        }
        catch (Exception)
        {

            throw;
        }

    }

    //HFCheckConsumers is still here, this is the main part that makes my hoppers fast.

    private void HFCheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if (this.mPermissions == eHopperPermissions.Locked)
        {
            return;
        }
        StorageConsumerInterface storageConsumerInterface = checkSegment.SearchEntity(checkX, checkY, checkZ) as StorageConsumerInterface;
        if (storageConsumerInterface != null)
        {
            storageConsumerInterface.ProcessStorageConsumer(this);
        }
        if ((lCube == 513) && WorldScript.mbIsServer && this.mnStorageUsed > 0)
        {
            ConveyorEntity Conveyor = checkSegment.FetchEntity(eSegmentEntity.Conveyor, checkX, checkY, checkZ) as ConveyorEntity;
            Vector3 lVTT = new Vector3(mnX - Conveyor.mnX, mnY - Conveyor.mnY, mnZ - Conveyor.mnZ);
            float lrDot = Vector3.Dot(lVTT, Conveyor.mForwards);
            //Cubes
            //IF its carring a cube, and is not a ACF, and is NOT pointing at the hopper.

            //LogError("lrDot: ("+lrDot+")");
            //LogError ("At Hopper: (" + PrintBoolean((lrDot == 1)) + ")");//Returns true if the conveyor is pointing at the hopper
            bool Ready = (Conveyor.mbReadyToConvey && lrDot != 1 && Conveyor != null && this.mnStorageUsed > 0);
            if (!Ready)
            {
                return;
            }
            bool ReadyExemplar = (Conveyor.ExemplarBlockID != 0 || Conveyor.ExemplarItemID != -1);
            //IF ITS EVERYTHING EXECPT AN ADVANCED CONVEYOR FILTER
            //THIS ALSO HANDLES STAMPER AND SO ON
            if (Conveyor.mValue != 12)
            {
                ItemBase litem = this.RemoveSingleSpecificItemOrCubeRoundRobin((ExtraStorageHoppers.eRequestType)Conveyor.meRequestType);
                if (litem != null)
                {
                    Conveyor.AddItem(litem);
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return;
                }


            }
            //IF ITS A ADVANCED CONVEYOR FILTER
            else if (Conveyor.mValue == 12)
            {
                if (Conveyor.ExemplarItemID != -1 && ReadyExemplar)
                {
                    ItemBase litem = this.RemoveSingleSpecificItemByID(Conveyor.ExemplarItemID, Conveyor.mbInvertExemplar);
                    if (litem != null)
                    {
                        Conveyor.AddItem(litem);
                        this.CountFreeSlots();
                        this.MarkDirtyDelayed();
                        return;
                    }

                }
                else if (Conveyor.ExemplarBlockID != 0 && ReadyExemplar)
                {
                    ItemBase lcube = this.RemoveSingleSpecificCubeStack(new ItemCubeStack(Conveyor.ExemplarBlockID, Conveyor.ExemplarBlockValue, 1), Conveyor.mbInvertExemplar);
                    if (lcube != null)
                    {
                        Conveyor.AddItem(lcube);
                        this.CountFreeSlots();
                        this.MarkDirtyDelayed();
                        return;
                    }
                }
            }
        }

    }

    //No reason to do faster supplying, its already as fast as it gets without skipping conveyors.
    //This is part of the optimise. The machine having items will now add them to the hopper.
    //The hopper will no longer take the item from the entity and then add it to the hopper.
    //This means work will only be done, IF a machine is offloading to the hopper. GOOD

    //******************** PRIVATE TO PUBLIC **********************

    public ushort GetCubeValue()
    {
        return this.CubeValue;
    }

    public int GetCubeType(string key)
    {
        ModCubeMap cube = null;
        ModManager.mModMappings.CubesByKey.TryGetValue(key, out cube);
        if (cube != null)
        {
            return cube.CubeType;
        }
        return 0;
    }

    public eHopperPermissions GetPermissions()
    {
        return mPermissions;
    }

    //******************** HOTBAR **********************

    private ItemBase GetCurrentHotBarItemOrCubeAsItem(out int lnAvailable)
    {
        return this.GetCurrentHotBarItemOrCubeAsItem(out lnAvailable, false);
    }

    private ItemBase GetCurrentHotBarItemOrCubeAsItem(out int lnAvailable, bool lastStackCount)
    {
        if (SurvivalHotBarManager.instance == null)
        {
            UnityEngine.Debug.LogWarning("SurvivalHotBarManager.instance is null??");
            lnAvailable = 0;
            return null;
        }
        SurvivalHotBarManager.HotBarEntry currentHotBarEntry = SurvivalHotBarManager.instance.GetCurrentHotBarEntry();
        if (currentHotBarEntry == null)
        {
            lnAvailable = 0;
            return null;
        }
        if (lastStackCount && (currentHotBarEntry.lastStackCount > 0))
        {
            lnAvailable = currentHotBarEntry.lastStackCount;
        }
        else
        {
            lnAvailable = currentHotBarEntry.count;
        }
        if (currentHotBarEntry.state != SurvivalHotBarManager.HotBarEntryState.Empty)
        {
            if (currentHotBarEntry.cubeType != 0)
            {
                return ItemManager.SpawnCubeStack(currentHotBarEntry.cubeType, currentHotBarEntry.cubeValue, 1);
            }
            if (currentHotBarEntry.itemType >= 0)
            {
                return ItemManager.SpawnItem(currentHotBarEntry.itemType);
            }
            LogError_AW("No cube and no item in hotbar?");
        }
        return null;
    }

    //******************** GET AND GIVE ITEMS (OLD METHOD)**********************

    private void CheckSuppliers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if (this.mPermissions == eHopperPermissions.Locked || this.mPermissions == eHopperPermissions.RemoveOnly)
        {
            return;
        }
        if (this.mnStorageFree <= 0)
        {
            return;
        }
        StorageSupplierInterface storageSupplierInterface = checkSegment.SearchEntity(checkX, checkY, checkZ) as StorageSupplierInterface;
        if (storageSupplierInterface != null)
        {
            storageSupplierInterface.ProcessStorageSupplier(this);
        }
    }

    private void CheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if (this.mPermissions == eHopperPermissions.Locked || this.mPermissions == eHopperPermissions.AddOnly)
        {
            return;
        }
        StorageConsumerInterface storageConsumerInterface = checkSegment.SearchEntity(checkX, checkY, checkZ) as StorageConsumerInterface;
        if (storageConsumerInterface != null)
        {
            storageConsumerInterface.ProcessStorageConsumer(this);
        }
    }

    //******************** ADD ITEMS AND CUBES **********************

    public void AddCube(ushort lType, ushort lValue)
    {
        //If its a Void Hopper (CubeValue = 0) and feed HiveMind is enabled, then this is what feeds the hivemind
        if (this.CubeValue == 0)
        {
            if (this.FeedHiveMind && this.SearchForHivemind())
            {
                this.FeedTicker += 1;
                if (this.FeedTicker > 5)
                {
                    this.FeedTicker = 0;
                    this.mHive.AddCube(lType, lValue);
                }
            }
            this.mLastItemAdded = TerrainData.GetNameForValue(lType, lValue);
            this.ItemsDeleted += 1;
            this.mbForceTextUpdate = true;
            return;
        }
        if (lType == 0)
        {
            LogError("Who and why is someone adding NULL to a Storage Hopper?");
        }
        if (this.mnStorageFree <= 0)
        {
            LogError("Error, can't AddCube " + lType + " to hopper, it's full!");
        }
        else
        {
            int mnMaxStorage = this.mnMaxStorage;
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                ItemBase base2 = this.maItemInventory[i];
                if (base2 == null)
                {
                    if (i < mnMaxStorage)
                    {
                        mnMaxStorage = i;
                    }
                    if (WorldScript.mLocalPlayer.mResearch.IsKnown(lType, 0))
                    {
                        this.mLastItemAdded = TerrainData.mEntries[lType].Name;
                    }
                    else
                    {
                        this.mLastItemAdded = "Unknown Material";
                    }
                    break;
                }
                if (base2.mType == ItemType.ItemCubeStack)
                {
                    ItemCubeStack stack = base2 as ItemCubeStack;
                    if (stack == null)
                    {
                        LogError_AW(string.Concat(new object[] { "Error, failed to convert item into ItemCubeStack", base2.mType, ":", base2.mnItemID }));
                        return;
                    }
                    if ((stack.mCubeType == lType) && (stack.mCubeValue == lValue))
                    {
                        stack.mnAmount++;
                        if (WorldScript.mLocalPlayer.mResearch.IsKnown(lType, 0))
                        {
                            if (lType >= TerrainData.mEntries.Length)
                            {
                                LogError_AW(string.Concat(new object[] { "Error, AddCube tried to get terrain data entry for", lType, " but max was only ", TerrainData.mEntries.Length }));
                                this.mLastItemAdded = "ERROR Unknown cube[" + lType + "] added";
                            }
                            else
                            {
                                TerrainDataEntry entry = TerrainData.mEntries[lType];
                                if (entry == null)
                                {
                                    this.mLastItemAdded = "ERROR Unknown cube[" + lType + "] added";
                                }
                                else
                                {
                                    this.mLastItemAdded = entry.Name;
                                }
                            }
                        }
                        else
                        {
                            this.mLastItemAdded = "Unknown Material";
                        }
                        this.MarkDirtyDelayed();
                        this.CountFreeSlots();
                        return;
                    }
                }
            }
            if (mnMaxStorage == this.mnMaxStorage)
            {
                Debug.Log("Attempted to add to Storage Hopper and failed miserably!");
            }
            else
            {
                this.maItemInventory[mnMaxStorage] = ItemManager.SpawnCubeStack(lType, lValue, 1);
                this.CountFreeSlots();
                this.MarkDirtyDelayed();
            }
        }
    }

    public bool AddItem(ItemBase lItemToAdd)
    {
        //This should be the first to happen. This ensures that NULL items wont be added to the void hopper, since this would create an error. If its null, just return true.
        if (lItemToAdd == null)
        {
            return true;
        }
        this.CountFreeSlots();
        //If its a Void Hopper (CubeValue = 0) and feed HiveMind is enabled, then this is what feeds the hivemind
        if (this.CubeValue == 0)
        {
            if (this.FeedHiveMind && this.SearchForHivemind() && this.FeedTicker > 10)
            {
                //This ensures if you insert a stack of items, every single one will count as an item added, not a hole stack as one item...
                int count = ItemManager.GetCurrentStackSize(lItemToAdd);
                while (count > 0)
                {
                    count -= 1;
                    this.FeedTicker += 1;
                    if (this.FeedTicker > 5)
                    {
                        this.FeedTicker = 0;
                        this.mHive.AddItem(lItemToAdd);
                    }
                }

            }
            //This was broken, but now adds to correct amount everytime you insert stuff into the void hopper
            this.mLastItemAdded = ItemManager.GetItemName(lItemToAdd);
            this.ItemsDeleted += ItemManager.GetCurrentStackSize(lItemToAdd);
            this.mbForceTextUpdate = true;
            return true;
        }
        //If its not a void hopper, and the item is not null, check if there is storage left.
        //If there is storage free, insert the item into the hopper. THis is very usefull, because without it, it would be impossible to store anything in the hoppers.
        if (this.mnStorageFree > 0)
        {
            if (ItemManager.GetCurrentStackSize(lItemToAdd) > this.mnStorageFree)
            {
                return false;
            }
            if ((lItemToAdd.mType == ItemType.ItemStack) && ((lItemToAdd as ItemStack).mnAmount == 0))
            {
                LogError_AW("Error, attempting to add an ItemStack of ZERO to the SH?![" + ItemManager.GetItemName(lItemToAdd) + "]");
                return false;
            }
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                ItemBase existingItem = this.maItemInventory[i];
                if (((existingItem != null) && (existingItem.mType == lItemToAdd.mType)) && ItemManager.StackWholeItems(existingItem, lItemToAdd, false))
                {
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    if (WorldScript.mLocalPlayer.mResearch.IsKnown(lItemToAdd))
                    {
                        this.mLastItemAdded = ItemManager.GetItemName(lItemToAdd);
                    }
                    else
                    {
                        this.mLastItemAdded = "Unknown Material";
                    }
                    return true;
                }
            }
            for (int j = 0; j < this.mnMaxStorage; j++)
            {
                if (this.maItemInventory[j] == null)
                {
                    this.maItemInventory[j] = lItemToAdd;
                    if (WorldScript.mLocalPlayer.mResearch.IsKnown(lItemToAdd))
                    {
                        this.mLastItemAdded = ItemManager.GetItemName(lItemToAdd);
                    }
                    else
                    {
                        this.mLastItemAdded = "Unknown Material";
                    }
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return true;
                }
            }
        }
        return false;
    }

    //******************** REMOVE ITEMS AND CUBES **********************

    private ItemBase DecrementInventorySlot(int i)
    {
        try
        {
            ItemBase base2 = null;
            if (this.maItemInventory[i].mType == ItemType.ItemCubeStack)
            {
                ItemBase item = this.maItemInventory[i];
                ItemCubeStack stack = item as ItemCubeStack;
                base2 = ItemManager.CloneItem(item);
                (base2 as ItemCubeStack).mnAmount = 1;
                stack.mnAmount--;
                if (stack.mnAmount <= 0)
                {
                    this.maItemInventory[i] = null;
                }
                this.CountFreeSlots();
                this.MarkDirtyDelayed();
                return base2;
            }
            if (this.maItemInventory[i] is ItemStack)
            {
                if ((this.maItemInventory[i] as ItemStack).mnAmount == 0)
                {
                    return null;
                }
                if ((this.maItemInventory[i] as ItemStack).mnAmount > 1)
                {
                    ItemStack stack1 = this.maItemInventory[i] as ItemStack;
                    stack1.mnAmount--;
                    base2 = ItemManager.CloneItem(this.maItemInventory[i]);
                    (base2 as ItemStack).mnAmount = 1;
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return base2;
                }
                base2 = this.maItemInventory[i];
                this.maItemInventory[i] = null;
                this.CountFreeSlots();
                this.MarkDirtyDelayed();
                return base2;
            }
            base2 = this.maItemInventory[i];
            this.maItemInventory[i] = null;
            this.CountFreeSlots();
            this.MarkDirtyDelayed();
            return base2;
        }
        catch (Exception)
        {

            throw;
        }

    }

    public ItemBase RemoveFirstInventoryItem()
    {
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maItemInventory[i] != null)
            {
                ItemBase base2 = this.maItemInventory[i];
                this.maItemInventory[i] = null;
                this.CountFreeSlots();
                this.MarkDirtyDelayed();
                if (this.mnStorageUsed == 0)
                {
                    this.mLastItemAdded = "Empty";
                }
                this.RequestImmediateNetworkUpdate();
                return base2;
            }
        }
        return null;
    }

    public ItemBase RemoveFirstInventoryItemOrDecrementStack()
    {
        if (this.mnStorageUsed == 0)
        {
            LogError_AW("Attempted to remove item but hopper was empty - are you dumb?");
        }
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maItemInventory[i] != null)
            {
                try
                {
                    ItemBase base2 = ItemManager.CloneItem(this.maItemInventory[i]);
                    if (base2.mType == ItemType.ItemStack)
                    {
                        (base2 as ItemStack).mnAmount = 1;
                        ItemStack stack1 = this.maItemInventory[i] as ItemStack;
                        stack1.mnAmount--;
                        if ((this.maItemInventory[i] as ItemStack).mnAmount <= 0)
                        {
                            this.maItemInventory[i] = null;
                        }
                    }
                    else if (base2.mType == ItemType.ItemCubeStack)
                    {
                        (base2 as ItemCubeStack).mnAmount = 1;
                        ItemCubeStack stack2 = this.maItemInventory[i] as ItemCubeStack;
                        stack2.mnAmount--;
                        if ((this.maItemInventory[i] as ItemCubeStack).mnAmount <= 0)
                        {
                            this.maItemInventory[i] = null;
                        }
                    }
                    else
                    {
                        this.maItemInventory[i] = null;
                    }
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    if (this.mnStorageUsed == 0)
                    {
                        this.mLastItemAdded = "Empty";
                    }
                    return base2;
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if (this.maStorage[j] != 0)
            {
                ItemBase base3 = ItemManager.SpawnCubeStack(this.maStorage[j], TerrainData.GetDefaultValue(this.maStorage[j]), 1);
                Debug.LogWarning("SH returning cubestack as item!");
                this.maStorage[j] = 0;
                this.CountFreeSlots();
                this.MarkDirtyDelayed();
                if (this.mnStorageUsed == 0)
                {
                    this.mLastItemAdded = "Empty";
                }
                return base3;
            }
        }
        return null;
    }

    public bool RemoveInventoryCube(ushort lType)
    {
        if (this.mnStorageUsed != 0)
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if (this.maStorage[i] == lType)
                {
                    this.maStorage[i] = 0;
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    if (this.mnStorageUsed == 0)
                    {
                        this.mLastItemAdded = "Empty";
                    }
                    return true;
                }
            }
            for (int j = 0; j < this.mnMaxStorage; j++)
            {
                if ((this.maItemInventory[j] != null) && (this.maItemInventory[j].mType == ItemType.ItemCubeStack))
                {
                    ItemCubeStack stack = this.maItemInventory[j] as ItemCubeStack;
                    if ((stack.mnAmount > 0) && (stack.mCubeType == lType))
                    {
                        stack.mnAmount--;
                        if (stack.mnAmount <= 0)
                        {
                            this.maItemInventory[j] = null;
                        }
                        this.CountFreeSlots();
                        this.MarkDirtyDelayed();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public int RemoveInventoryCube(ushort lType, ushort lValue, int amount)
    {
        if (this.mnStorageUsed == 0)
        {
            return 0;
        }
        int num = amount;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maStorage[i] == lType)
            {
                this.maStorage[i] = 0;
                num--;
                if (num == 0)
                {
                    break;
                }
            }
            ItemBase base2 = this.maItemInventory[i];
            if ((base2 != null) && (base2.mnItemID == -1))
            {
                ItemCubeStack stack = base2 as ItemCubeStack;
                if ((stack.mCubeType == lType) && (stack.mCubeValue == lValue))
                {
                    if (stack.mnAmount <= num)
                    {
                        this.maItemInventory[i] = null;
                        num -= stack.mnAmount;
                        if (num != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        stack.mnAmount -= num;
                        num = 0;
                    }
                    break;
                }
            }
        }
        if (num < amount)
        {
            this.CountFreeSlots();
            this.MarkDirtyDelayed();
            if (this.mnStorageUsed == 0)
            {
                this.mLastItemAdded = "Empty";
            }
        }
        return (amount - num);
    }

    public int RemoveInventoryItem(int itemID, int count)
    {
        int num = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            ItemBase base2 = this.maItemInventory[i];
            if ((base2 != null) && (base2.mnItemID == itemID))
            {
                if (base2.mType == ItemType.ItemCubeStack)
                {
                    LogError_AW("Errror, RemoveInventoryItem does not support cubestacks");
                }
                if (base2.mType == ItemType.ItemStack)
                {
                    ItemStack stack = base2 as ItemStack;
                    if (stack.mnAmount <= (count - num))
                    {
                        num += stack.mnAmount;
                        stack.mnAmount = 0;
                        this.maItemInventory[i] = null;
                    }
                    else
                    {
                        stack.mnAmount -= count - num;
                        num = count;
                    }
                }
                else
                {
                    num++;
                    this.maItemInventory[i] = null;
                }
                if (num >= count)
                {
                    break;
                }
            }
        }
        if (num > 0)
        {
            this.CountFreeSlots();
            this.MarkDirtyDelayed();
            if (this.mnStorageUsed == 0)
            {
                this.mLastItemAdded = "Empty";
            }
        }
        return num;
    }

    public ItemBase RemoveSingleSpecificCubeStack(ItemCubeStack lItem, bool lbInvertSearch = false)
    {
        if (lItem == null)
        {
            LogError_AW("There's probably a good reason why RemoveSingleSpecificCubeStack is looking for a null Item");
            return null;
        }
        if (this.mnStorageUsed != 0)
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if ((this.maItemInventory[i] != null) && (this.maItemInventory[i].mType == ItemType.ItemCubeStack))
                {
                    ItemCubeStack stack = this.maItemInventory[i] as ItemCubeStack;
                    bool flag = false;
                    if ((!lbInvertSearch && (stack.mCubeType == lItem.mCubeType)) && (stack.mCubeValue == lItem.mCubeValue))
                    {
                        flag = true;
                    }
                    if (lbInvertSearch)
                    {
                        if (stack.mCubeType != lItem.mCubeType)
                        {
                            flag = true;
                        }
                        if (stack.mCubeValue != lItem.mCubeValue)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        ItemBase base2 = this.DecrementInventorySlot(i);
                        if (base2 != null)
                        {
                            this.CountFreeSlots();
                            this.MarkDirtyDelayed();
                            return base2;
                        }
                    }
                }
            }
        }
        return null;
    }

    public ItemBase RemoveSingleSpecificItem(eRequestType lType)
    {
        if (lType == eRequestType.eNone)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        if (lType == eRequestType.eOreOnly)
        {
            return null;
        }
        if (lType == eRequestType.eGarbage)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        ItemBase base2 = null;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (((this.maItemInventory[i] != null) && (this.maItemInventory[i].mType != ItemType.ItemCubeStack)) && (lType == eRequestType.eAny))
            {
                base2 = this.DecrementInventorySlot(i);
                if (base2 != null)
                {
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return base2;
                }
            }
        }
        return base2;
    }

    public ItemBase RemoveSingleSpecificItemByID(int lnItemID, bool lbInvertSearch = false)
    {
        if (lnItemID == -1)
        {
            LogError_AW("There's probably a good reason why RemoveSingleSpecificItemByID is looking for ItemID -1");
            return null;
        }
        if (this.mnStorageUsed != 0)
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if (this.maItemInventory[i] != null)
                {
                    if (lbInvertSearch)
                    {
                        if (this.maItemInventory[i].mnItemID != lnItemID)
                        {
                            ItemBase base2 = this.DecrementInventorySlot(i);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                    }
                    else if (this.maItemInventory[i].mnItemID == lnItemID)
                    {
                        ItemBase base3 = this.DecrementInventorySlot(i);
                        if (base3 != null)
                        {
                            this.CountFreeSlots();
                            this.MarkDirtyDelayed();
                            return base3;
                        }
                    }
                }
            }
        }
        return null;
    }

    public ItemBase RemoveSingleSpecificItemOrCube(eRequestType lType)
    {
        if (lType == eRequestType.eNone)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        if (lType == eRequestType.eOreOnly)
        {
            return null;
        }
        if (lType == eRequestType.eGarbage)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        ItemBase base2 = null;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if ((this.maItemInventory[i] != null) && (lType == eRequestType.eAny))
            {
                base2 = this.DecrementInventorySlot(i);
                if (base2 != null)
                {
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return base2;
                }
            }
        }
        return base2;
    }

    public ItemBase RemoveSingleSpecificItemOrCubeRoundRobin(eRequestType lType)
    {
        if (lType == eRequestType.eNone)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        if (lType == eRequestType.eOreOnly)
        {
            return null;
        }
        if (lType == eRequestType.eGarbage)
        {
            return null;
        }
        if (lType == eRequestType.eHighCalorieOnly)
        {
            return null;
        }
        ItemBase base2 = null;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            this.mnRoundRobinOffset++;
            this.mnRoundRobinOffset = this.mnRoundRobinOffset % this.mnMaxStorage;
            int mnRoundRobinOffset = this.mnRoundRobinOffset;
            if (this.maItemInventory[mnRoundRobinOffset] != null)
            {
                if (lType == eRequestType.eAny)
                {
                    base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                    if (base2 != null)
                    {
                        this.CountFreeSlots();
                        this.MarkDirtyDelayed();
                        return base2;
                    }
                }
                else
                {
                    if (this.maItemInventory[mnRoundRobinOffset].mType == ItemType.ItemStack)
                    {
                        int mnItemID = this.maItemInventory[mnRoundRobinOffset].mnItemID;
                        if (((lType == eRequestType.eOrganic) && (mnItemID >= 0xfa0)) && (mnItemID <= 0x1005))
                        {
                            base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                        if (lType == eRequestType.eBarsOnly)
                        {
                            bool flag = false;
                            if (mnItemID == ItemEntries.CopperBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.TinBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.IronBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.LithiumBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.GoldBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.NickelBar)
                            {
                                flag = true;
                            }
                            if (mnItemID == ItemEntries.TitaniumBar)
                            {
                                flag = true;
                            }
                            if (flag)
                            {
                                base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                                if (base2 != null)
                                {
                                    this.CountFreeSlots();
                                    this.MarkDirtyDelayed();
                                    return base2;
                                }
                            }
                        }
                        if (lType == eRequestType.eAnyCraftedItem)
                        {
                            base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                    }
                    if (lType == eRequestType.eAnyCraftedItem)
                    {
                        if (this.maItemInventory[mnRoundRobinOffset].mType == ItemType.ItemSingle)
                        {
                            base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                        if (this.maItemInventory[mnRoundRobinOffset].mType == ItemType.ItemDurability)
                        {
                            base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                        if (this.maItemInventory[mnRoundRobinOffset].mType == ItemType.ItemCharge)
                        {
                            base2 = this.DecrementInventorySlot(mnRoundRobinOffset);
                            if (base2 != null)
                            {
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return base2;
                            }
                        }
                    }
                }
            }
        }
        return base2;
    }

    //******************** HANDLE ORGANIC STUFF **********************

    public static int GetBarTypeFromOreType(ushort lType)
    {
        List<CraftData> recipesForSet = CraftData.GetRecipesForSet("Smelter");
        if (recipesForSet != null)
        {
            foreach (CraftData data in recipesForSet)
            {
                if (data.Costs[0].CubeType == lType)
                {
                    return data.CraftableItemType;
                }
            }
            LogError_AW("Error, unable to locate ItemID for Bar from type " + lType);
        }
        else
        {
            LogError_AW("Error, recipes for smelter were not found. Note this can happen in editor when opening the handbook.");
        }
        return ItemEntry.GetIDFromName("Copper Bar", true);
    }

    //******************** CHECK HOPPER INVENTORY **********************

    public int ContainsOre(ushort lSearchType, bool knownOnly, List<CraftData> recipes, out ushort lBestChoice)
    {
        if (lSearchType != 0)
        {
            lBestChoice = lSearchType;
            return this.CountHowManyOfOreType(lSearchType);
        }
        int num = 0;
        lBestChoice = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if ((this.maStorage[i] == 0) || !CubeHelper.IsIngottableOre(this.maStorage[i]))
            {
                continue;
            }
            bool flag = true;
            if (recipes != null)
            {
                flag = false;
                foreach (CraftData data in recipes)
                {
                    foreach (CraftCost cost in data.Costs)
                    {
                        if (cost.CubeType == this.maStorage[i])
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
            }
            if (flag)
            {
                ushort defaultValue = TerrainData.GetDefaultValue(this.maStorage[i]);
                if (!knownOnly || WorldScript.mLocalPlayer.mResearch.IsKnown(this.maStorage[i], defaultValue))
                {
                    int num4 = this.CountHowManyOfOreType(lSearchType);
                    if (num4 > num)
                    {
                        lBestChoice = this.maStorage[i];
                        num = num4;
                    }
                }
            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if (((this.maItemInventory[j] == null) || (this.maItemInventory[j].mType != ItemType.ItemCubeStack)) || ((this.maItemInventory[j] as ItemCubeStack).mnAmount <= 0))
            {
                continue;
            }
            ItemCubeStack stack = this.maItemInventory[j] as ItemCubeStack;
            if (stack == null)
            {
                LogError_AW("Error stack was null in Contains Ore?");
                continue;
            }
            if (WorldScript.mLocalPlayer.mResearch == null)
            {
                LogError_AW("mResearch stack was null in Contains Ore?");
                continue;
            }
            if ((stack.mnItemID != lBestChoice) && CubeHelper.IsIngottableOre(stack.mCubeType))
            {
                bool flag2 = true;
                if (recipes != null)
                {
                    flag2 = false;
                    foreach (CraftData data2 in recipes)
                    {
                        foreach (CraftCost cost2 in data2.Costs)
                        {
                            if (cost2.CubeType == stack.mCubeType)
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            break;
                        }
                    }
                }
                if (flag2 && (!knownOnly || WorldScript.mLocalPlayer.mResearch.IsKnown(stack.mCubeType, 0)))
                {
                    int num6 = this.CountHowManyOfOreType(stack.mCubeType);
                    if (num6 > num)
                    {
                        lBestChoice = stack.mCubeType;
                        num = num6;
                    }
                }
            }
        }
        return num;
    }

    private int ReturnFreeSlots()
    {
        int mnStorageFree = this.mnStorageFree;
        //LogValue("***OLD*** mnStorageFree", this.mnStorageFree);
        this.mnStorageUsed = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maStorage[i] != 0)
            {
                this.mnStorageUsed++;
            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if (this.maItemInventory[j] != null)
            {
                ItemBase base2 = this.maItemInventory[j];
                if (base2.mType == ItemType.ItemStack)
                {
                    this.mnStorageUsed += (base2 as ItemStack).mnAmount;
                }
                else if (base2.mType == ItemType.ItemCubeStack)
                {
                    this.mnStorageUsed += (base2 as ItemCubeStack).mnAmount;
                }
                else
                {
                    this.mnStorageUsed++;
                }
            }
        }
        if (this.mnStorageUsed > this.mnMaxStorage)
        {
            LogError_AW(string.Concat(new object[] { "Storage hopper has overflowed! ", this.mnStorageUsed, "/", this.mnMaxStorage, ".", Environment.StackTrace, "Last Item was ", this.mLastItemAdded }));
        }
        this.mnStorageFree = this.mnMaxStorage - this.mnStorageUsed;
        //LogValue("***NEW*** mnStorageFree ", this.mnStorageFree);
        if (mnStorageFree != this.mnStorageFree)
        {
            this.mbForceTextUpdate = true;
        }
        return mnStorageFree;
    }

    private void CountFreeSlots()
    {
        int mnStorageFree = this.mnStorageFree;
        //LogValue("***OLD*** mnStorageFree", this.mnStorageFree);
        this.mnStorageUsed = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maStorage[i] != 0)
            {
                this.mnStorageUsed++;
            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if (this.maItemInventory[j] != null)
            {
                ItemBase base2 = this.maItemInventory[j];
                if (base2.mType == ItemType.ItemStack)
                {
                    this.mnStorageUsed += (base2 as ItemStack).mnAmount;
                }
                else if (base2.mType == ItemType.ItemCubeStack)
                {
                    this.mnStorageUsed += (base2 as ItemCubeStack).mnAmount;
                }
                else
                {
                    this.mnStorageUsed++;
                }
            }
        }
        if (this.mnStorageUsed > this.mnMaxStorage)
        {
            LogError_AW(string.Concat(new object[] { "Storage hopper has overflowed! ", this.mnStorageUsed, "/", this.mnMaxStorage, ".", Environment.StackTrace, "Last Item was ", this.mLastItemAdded }));
        }
        this.mnStorageFree = this.mnMaxStorage - this.mnStorageUsed;
        //LogValue("***NEW*** mnStorageFree ", this.mnStorageFree);
        if (mnStorageFree != this.mnStorageFree)
        {
            this.mbForceTextUpdate = true;
        }
    }

    public int CountHowManyOfItem(int itemID)
    {
        int num = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            ItemBase base2 = this.maItemInventory[i];
            if ((base2 != null) && (base2.mnItemID == itemID))
            {
                ItemStack stack = base2 as ItemStack;
                if (stack != null)
                {
                    num += stack.mnAmount;
                }
                else
                {
                    num++;
                }
            }
        }
        return num;
    }

    public int CountHowManyOfOreType(ushort lType)
    {
        if (!CubeHelper.IsOre(lType))
        {
            // LogError_AW("This is only for Ores and other things that we DO NOT CARE ABOUT THE VALUE OF");
        }
        int num = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maStorage[i] == lType)
            {
                num++;
            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if ((this.maItemInventory[j] != null) && (this.maItemInventory[j].mType == ItemType.ItemCubeStack))
            {
                ItemCubeStack stack = this.maItemInventory[j] as ItemCubeStack;
                if (stack.mCubeType == lType)
                {
                    num += stack.mnAmount;
                }
            }
        }
        return num;
    }

    public int CountHowManyOfType(ushort lType, ushort lValue)
    {
        int num = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            if (this.maStorage[i] == lType)
            {
                num++;
            }
        }
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            if ((this.maItemInventory[j] != null) && (this.maItemInventory[j].mType == ItemType.ItemCubeStack))
            {
                ItemCubeStack stack = this.maItemInventory[j] as ItemCubeStack;
                if ((lValue == 0xffff) && (stack.mCubeType == lType))
                {
                    num += stack.mnAmount;
                }
                if ((stack.mCubeType == lType) && (stack.mCubeValue == lValue))
                {
                    num += stack.mnAmount;
                }
            }
        }
        return num;
    }

    private bool AttemptToSpoilOrganicItem(int i)
    {
        if ((this.maItemInventory[i] != null) && (((this.maItemInventory[i].mnItemID >= 0xfa0) && (this.maItemInventory[i].mnItemID <= 0xfaa)) && ((this.maItemInventory[i].mnItemID % 2) == 1)))
        {
            this.mrSpoilTimer = 30f;
            this.DecrementInventorySlot(i);
            ItemBase lItemToAdd = ItemManager.SpawnItem(0x1004);
            this.AddItem(lItemToAdd);
            return true;
        }
        return false;
    }

    //******************** GET ITEM/CUBE INFORMATION **********************

    public void GetSpecificCube(eRequestType lType, out ushort cubeType, out ushort cubeValue)
    {
        if (lType == eRequestType.eNone)
        {
            cubeType = 0;
            cubeValue = 0;
        }
        else if (((this.mnStorageUsed == 0) || (lType == eRequestType.eBarsOnly)) || (lType == eRequestType.eAnyCraftedItem))
        {
            cubeType = 0;
            cubeValue = 0;
        }
        else
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if ((((this.maStorage[i] != 0) && ((lType != eRequestType.eHighCalorieOnly) || CubeHelper.IsHighCalorie(this.maStorage[i]))) && (((lType != eRequestType.eOreOnly) || CubeHelper.IsSmeltableOre(this.maStorage[i])) && ((lType != eRequestType.eGarbage) || CubeHelper.IsGarbage(this.maStorage[i])))) && ((((lType != eRequestType.eCrystals) || (this.maStorage[i] == 0x98)) && ((lType != eRequestType.eGems) || (this.maStorage[i] == 0xa2))) && (((lType != eRequestType.eBioMass) || (this.maStorage[i] == 0x99)) && ((lType != eRequestType.eSmeltable) || CubeHelper.IsIngottableOre(this.maStorage[i])))))
                {
                    ushort type = this.maStorage[i];
                    this.RemoveInventoryCube(this.maStorage[i]);
                    if (this.mnStorageUsed == 0)
                    {
                        this.mLastItemAdded = "Empty";
                    }
                    cubeType = type;
                    cubeValue = TerrainData.GetDefaultValue(type);
                    return;
                }
            }
            for (int j = 0; j < this.mnMaxStorage; j++)
            {
                if ((this.maItemInventory[j] != null) && (this.maItemInventory[j].mType == ItemType.ItemCubeStack))
                {
                    ItemCubeStack stack = this.maItemInventory[j] as ItemCubeStack;
                    if (((((lType != eRequestType.eHighCalorieOnly) || CubeHelper.IsHighCalorie(stack.mCubeType)) && ((lType != eRequestType.eOreOnly) || CubeHelper.IsSmeltableOre(stack.mCubeType))) && (((lType != eRequestType.eGarbage) || CubeHelper.IsGarbage(stack.mCubeType)) && ((lType != eRequestType.eCrystals) || (stack.mCubeType == 0x98)))) && ((((lType != eRequestType.eGems) || (stack.mCubeType == 0xa2)) && ((lType != eRequestType.eBioMass) || (stack.mCubeType == 0x99))) && ((lType != eRequestType.eSmeltable) || CubeHelper.IsIngottableOre(stack.mCubeType))))
                    {
                        this.RemoveInventoryCube(stack.mCubeType, stack.mCubeValue, 1);
                        if (this.mnStorageUsed == 0)
                        {
                            this.mLastItemAdded = "Empty";
                        }
                        cubeType = stack.mCubeType;
                        cubeValue = stack.mCubeValue;
                        return;
                    }
                }
            }
            cubeType = 0;
            cubeValue = 0;
        }
    }

    public void GetSpecificCubeRoundRobin(eRequestType lType, out ushort cubeType, out ushort cubeValue)
    {
        if (lType == eRequestType.eNone)
        {
            cubeType = 0;
            cubeValue = 0;
        }
        else if (((this.mnStorageUsed == 0) || (lType == eRequestType.eBarsOnly)) || (lType == eRequestType.eAnyCraftedItem))
        {
            cubeType = 0;
            cubeValue = 0;
        }
        else
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                this.mnRoundRobinOffset++;
                this.mnRoundRobinOffset = this.mnRoundRobinOffset % this.mnMaxStorage;
                int mnRoundRobinOffset = this.mnRoundRobinOffset;
                if (this.maStorage[mnRoundRobinOffset] != 0)
                {
                    if ((((lType != eRequestType.eOrganic) && ((lType != eRequestType.eHighCalorieOnly) || CubeHelper.IsHighCalorie(this.maStorage[mnRoundRobinOffset]))) && (((lType != eRequestType.eOreOnly) || CubeHelper.IsSmeltableOre(this.maStorage[mnRoundRobinOffset])) && ((lType != eRequestType.eGarbage) || CubeHelper.IsGarbage(this.maStorage[mnRoundRobinOffset])))) && ((((lType != eRequestType.eCrystals) || (this.maStorage[mnRoundRobinOffset] == 0x98)) && ((lType != eRequestType.eGems) || (this.maStorage[mnRoundRobinOffset] == 0xa2))) && (((lType != eRequestType.eBioMass) || (this.maStorage[mnRoundRobinOffset] == 0x99)) && ((lType != eRequestType.eSmeltable) || CubeHelper.IsIngottableOre(this.maStorage[mnRoundRobinOffset])))))
                    {
                        ushort type = this.maStorage[mnRoundRobinOffset];
                        this.RemoveInventoryCube(this.maStorage[mnRoundRobinOffset]);
                        if (this.mnStorageUsed == 0)
                        {
                            this.mLastItemAdded = "Empty";
                        }
                        cubeType = type;
                        cubeValue = TerrainData.GetDefaultValue(type);
                        return;
                    }
                }
                else if ((this.maItemInventory[mnRoundRobinOffset] != null) && (this.maItemInventory[mnRoundRobinOffset].mType == ItemType.ItemCubeStack))
                {
                    ItemCubeStack stack = this.maItemInventory[mnRoundRobinOffset] as ItemCubeStack;
                    if ((((lType != eRequestType.eOrganic) && ((lType != eRequestType.eHighCalorieOnly) || CubeHelper.IsHighCalorie(stack.mCubeType))) && (((lType != eRequestType.eOreOnly) || CubeHelper.IsSmeltableOre(stack.mCubeType)) && ((lType != eRequestType.eGarbage) || CubeHelper.IsGarbage(stack.mCubeType)))) && ((((lType != eRequestType.eCrystals) || (stack.mCubeType == 0x98)) && ((lType != eRequestType.eGems) || (stack.mCubeType == 0xa2))) && (((lType != eRequestType.eBioMass) || (stack.mCubeType == 0x99)) && ((lType != eRequestType.eSmeltable) || CubeHelper.IsIngottableOre(stack.mCubeType)))))
                    {
                        this.RemoveInventoryCube(stack.mCubeType, stack.mCubeValue, 1);
                        if (this.mnStorageUsed == 0)
                        {
                            this.mLastItemAdded = "Empty";
                        }
                        cubeType = stack.mCubeType;
                        cubeValue = stack.mCubeValue;
                        return;
                    }
                }
            }
            cubeType = 0;
            cubeValue = 0;
        }
    }

    //******************** HANDLE OUTPUT **********************
    //IS THIS NEEDED? WHAT HAPPENS IF I REMOVE IT?
    //TODO: TEST THIS
    private void GetManufacturingPlantOutput(ManufacturingPlant plant)
    {
        if (plant.mOutputHopper != null)
        {
            ItemBase partialInventory;
            if (ItemManager.GetCurrentStackSize(plant.mOutputHopper) > this.mnStorageFree)
            {
                partialInventory = plant.GetPartialInventory(this.mnStorageFree);
            }
            else
            {
                partialInventory = plant.GetWholeInventory();
            }
            this.RequestImmediateNetworkUpdate();
            plant.RequestImmediateNetworkUpdate();
            this.AddItem(partialInventory);
        }
    }

    //******************** RETURN CONSTANTS **********************

    public float GetMaximumDeliveryRate()
    {
        return 500f;
    }

    public override int GetVersion()
    {

        return 1;
    }

    //******************** ITEM INTERACTIONS **********************

    public void IterateContents(IterateItem itemFunc)
    {
        if (itemFunc != null)
        {
            for (int i = 0; i < this.mnMaxStorage; i++)
            {
                if ((this.maItemInventory[i] != null) && !itemFunc(this.maItemInventory[i]))
                {
                    return;
                }
            }
        }
    }

    public void IterateContents(global::IterateItem itemFunc, object state)
    {
        if (itemFunc == null) return;


        for (int i = 0; i < mnMaxStorage; i++)
        {
            ItemBase item = maItemInventory[i];
            if (item == null) continue;
            if (!itemFunc(maItemInventory[i], state))
                return;
        }

    }

    public delegate bool IterateItem(ItemBase item);

    //******************** Hopper Modes ********************

    public void ToggleHoover()
    {
        this.mbHooverOn = !this.mbHooverOn;
        this.MarkDirtyDelayed();
        this.mbForceTextUpdate = true;
        this.RequestImmediateNetworkUpdate();
    }

    public void TogglePermissions()
    {
        mPermissions += 1;
        if (mPermissions == eHopperPermissions.eNumPermissions)
        {
            mPermissions = eHopperPermissions.AddAndRemove;
        }

        this.MarkDirtyDelayed();
        this.mbForceTextUpdate = true;
        this.mbForceHoloUpdate = true;
        this.RequestImmediateNetworkUpdate();

        FloatingCombatTextManager.instance.QueueText(base.mnX, base.mnY + 1L, base.mnZ, 1f, GetPermissions().ToString(), Color.green, 1.5f).mrStartRadiusRand = 0.25f;
    }

    //******************** UPDATE ********************

    private void UpdateSpoilage()
    {
        this.mrSpoilTimer = 30f;
    }

    //******************** READ/WRITE ********************

    public override void Read(BinaryReader reader, int entityVersion)
    {
        if (this.OT)
        {
            this.ExemplarItemID = reader.ReadInt32();               //1
            this.ExemplarString = reader.ReadString();              //2
            this.ExemplarBlockID = reader.ReadUInt16();             //3
            this.ExemplarBlockValue = reader.ReadUInt16();          //4
            if (this.ExemplarItemID != -1 || this.ExemplarBlockID != 0)
            {
                this.ExemplarSet = true;
            }
        }
        int index = -1;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            this.maStorage[i] = reader.ReadUInt16();
            if (this.maStorage[i] != 0)
            {
                index = i;
            }
        }
        eHopperPermissions mPermissions = this.mPermissions;
        this.mPermissions = (eHopperPermissions)reader.ReadInt32();
        if (((NetworkManager.mbClientRunning && (FloatingCombatTextManager.instance != null)) && (this.mbLinkedToGO && (this.mPermissions != mPermissions))) && (base.mDistanceToPlayer < 32f))
        {
            FloatingCombatTextManager.instance.QueueText(base.mnX, base.mnY + 1L, base.mnZ, 1f, this.mPermissions.ToString(), Color.cyan, 1.5f).mrStartRadiusRand = 0.25f;
        }
        this.mbHooverOn = reader.ReadBoolean();
        this.DebugMode = reader.ReadBoolean(); //Will remember if debug mode was enabled. (this allows for testing machine with debugmode on load) :D
        this.FeedHiveMind = reader.ReadBoolean();
        this.FeedTicker = reader.ReadByte();
        reader.ReadInt32();//NOT IN USE
        reader.ReadSingle(); //OLD POWER SINGLE
        reader.ReadSingle(); //OLD TEMP SINGLE
        if (this.CubeValue == 0)
        {
            this.ItemsDeleted = reader.ReadInt32(); //ONLY USED FOR VOID HOPPER
        }
        this.HopperNumber = reader.ReadInt32(); //IMPORTANT FOR DEBUGGING - SEE WRITE
        SetHopperNumIfHigher(this.HopperNumber); //Will make sure that 2 hoppers dont have the same number. 
        reader.ReadInt32();//NOT IN USE
        this.ReadStringComplete = "HooverOn: " + PrintBoolean(this.mbHooverOn);
        this.ReadStringComplete = this.ReadStringComplete + "\n" + "DebugMode: " + PrintBoolean(this.DebugMode);
        this.ReadStringComplete = this.ReadStringComplete + "\n" + "Items Deleted: " + this.ItemsDeleted;
        this.ReadStringComplete = this.ReadStringComplete + "\n" + "Hopper Number: " + this.HopperNumber;
        this.CountFreeSlots();
        if (entityVersion != 0)
        {
            int num3 = -1;
            for (int j = 0; j < this.mnMaxStorage; j++)
            {
                this.maItemInventory[j] = ItemFile.DeserialiseItem(reader);
                if (this.maItemInventory[j] != null)
                {
                    num3 = j;
                }
            }
            this.CountFreeSlots();
            if (num3 >= 0)
            {
                ItemBase item = this.maItemInventory[num3];
                if (WorldScript.mbHasPlayer)
                {
                    if ((WorldScript.mLocalPlayer.mResearch != null) && WorldScript.mLocalPlayer.mResearch.IsKnown(item))
                    {
                        this.mLastItemAdded = ItemManager.GetItemName(item);
                    }
                    else
                    {
                        this.mLastItemAdded = "Unknown Material";
                    }
                }
            }
            else if (index >= 0)
            {
                if (((WorldScript.mLocalPlayer == null) || (WorldScript.mLocalPlayer.mResearch == null)) || WorldScript.mLocalPlayer.mResearch.IsKnown(this.maStorage[index], 0))
                {
                    this.mLastItemAdded = TerrainData.GetNameForValue(this.maStorage[index], 0);
                }
                else
                {
                    this.mLastItemAdded = "Unknown Material";
                }
            }
            this.mbForceHoloUpdate = true;
            this.mbForceTextUpdate = true;
        }
    }

    public override void Write(BinaryWriter writer)
    {
        if (this.OT)
        {
            writer.Write(this.ExemplarItemID);                      //1
            writer.Write(this.ExemplarString);                      //2
            writer.Write(this.ExemplarBlockID);                     //3
            writer.Write(this.ExemplarBlockValue);                  //4
        }
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            writer.Write(this.maStorage[i]);
        }
        writer.Write((int)this.mPermissions);
        writer.Write(this.mbHooverOn);
        int num2 = 0;
        writer.Write(this.GetDebugMode()); //This is used to make the game remember if debugmode was enabled - See Read
        writer.Write(this.FeedHiveMind);//This is a toggle Option!
        writer.Write(this.FeedTicker);
        writer.Write(num2);
        writer.Write(num2);//OLD POWER SINGLE -- NOT IN USE
        writer.Write(num2);//OLD TEMP SINGLE -- NOT IN USE
        if (this.CubeValue == 0)
        {
            writer.Write(this.ItemsDeleted); //MOSTLY ONLY USED FOR VOID HOPPER
        }
        writer.Write(this.HopperNumber);//HOPPER NUMBER, IMPORTANT FOR DEBUGGING!
        writer.Write(num2);//NOT IN USE
        this.LatestWriteComplete = "Permissions: " + this.mPermissions.ToString();
        this.LatestWriteComplete = this.LatestWriteComplete + "Hoover Mode: " + PrintBoolean(this.mbHooverOn);
        this.LatestWriteComplete = this.LatestWriteComplete + "Debug Mode: " + PrintBoolean(this.GetDebugMode());
        this.LatestWriteComplete = this.LatestWriteComplete + "Items Deleted: " + this.ItemsDeleted;
        this.LatestWriteComplete = this.LatestWriteComplete + "Hopper Number: " + this.HopperNumber;
        for (int j = 0; j < this.mnMaxStorage; j++)
        {
            ItemFile.SerialiseItem(this.maItemInventory[j], writer);
        }
    }

    //******************** COMMUNITY ITEM INTERFACE (IN USE?) ********************

    public bool HasItems()
    {
        if (this.mnStorageUsed > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasItem(ItemBase item)
    {
        var cube = item as ItemCubeStack;
        if (this.CountHowManyOfItem(item.mnItemID) > 0 || this.CountHowManyOfType(cube.mCubeType, cube.mCubeValue) > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HasItems(ItemBase item, out int amount)
    {
        var isCube = item.mType == ItemType.ItemCubeStack;
        ItemCubeStack cube = null;
        if (isCube)
        {
            cube = item as ItemCubeStack;
        }
        amount = !isCube ? this.CountHowManyOfItem(item.mnItemID)
                         : this.CountHowManyOfType(cube.mCubeType, cube.mCubeValue);
        return amount > 0;
    }

    public bool HasFreeSpace(uint amount)
    {
        if (this.mnStorageFree >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetFreeSpace()
    {
        return this.mnStorageFree;
    }

    public bool GiveItem(ItemBase item)
    {
        return this.AddItem(item);
    }

    public ItemBase TakeItem(ItemBase item)
    {
        if (item.mType == ItemType.ItemCubeStack)
        {
            var cube = item as ItemCubeStack;
            return this.RemoveSingleSpecificCubeStack(cube);
        }
        else
        {
            return this.RemoveSingleSpecificItemByID(item.mnItemID);
        }
    }

    public ItemBase TakeAnyItem()
    {
        return this.RemoveSingleSpecificItemOrCube(eRequestType.eAny);
    }

    //********************************************************************************
    //******************** STORAGEUSERINTERFACE (NEW STORAGE API) ********************
    //********************************************************************************

    //******************** GET HOPPER STATUS ********************
    public bool IsEmpty()
    {
        return mnStorageUsed <= 0;
    }

    public bool IsFull()
    {
        return mnStorageFree <= 0;
    }

    public bool IsNotEmpty()
    {
        return mnStorageUsed > 0;
    }

    public bool IsNotFull()
    {
        return mnStorageFree > 0;
    }

    public bool InventoryExtractionPermitted
    {
        get { return true; } //THIS SHOULD ALWAYS BE TRUR UNLESS THE HOPPER IS FULL, THAT IS UP TO THE ENTITY TO CHECK
    }

    public int CountCubes(ushort cube, ushort value)
    {
        return CountHowManyOfType(cube, value);
    }

    public int CountItems(InventoryExtractionOptions options)
    {
        // Use the full extract function to count using all provided options.
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        if (!TryExtract(options.RequestType, options.ExemplarItemID, options.ExemplarBlockID, options.ExemplarBlockValue, options.InvertExemplar,
            options.MinimumAmount, options.MaximumAmount, options.KnownItemsOnly,
            true, false, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount))
        {
            // Failed to extract anything
            return 0;
        }

        return returnedAmount;
    }

    public int CountItems(int itemId)
    {
        return CountHowManyOfItem(itemId);
    }

    public int CountItems(int itemId, ushort cube, ushort value)
    {
        if (itemId >= 0)
        {
            return CountHowManyOfItem(itemId);
        }
        else
        {
            return CountHowManyOfType(cube, value);
        }
    }

    public int TotalCapacity
    {
        get { return mnMaxStorage; }
    }

    public int UsedCapacity
    {
        get { return mnStorageUsed; }
    }

    public int RemainingCapacity
    {
        get { return mnStorageFree; }
    }

    //******************** TRY TO DO STUFF ********************

    public bool TryExtract(InventoryExtractionOptions options, ref InventoryExtractionResults results)
    {
        ItemBase item;
        ushort cube;
        ushort value;
        int amount;
        if (!this.TryExtract(options.RequestType, options.ExemplarItemID, options.ExemplarBlockID, options.ExemplarBlockValue, options.InvertExemplar, options.MinimumAmount, options.MaximumAmount, options.KnownItemsOnly, false, false, options.ConvertToItem, out item, out cube, out value, out amount))
        {
            results.Item = null;
            results.Amount = 0;
            results.Cube = 0;
            results.Value = 0;
            return false;
        }
        results.Cube = cube;
        results.Value = value;
        results.Amount = amount;
        results.Item = item;
        return true;
    }

    public bool TryExtract(eHopperRequestType lType, int exemplarItemId, ushort exemplarCubeType, ushort exemplarCubeValue, bool invertExemplar, int minimumAmount, int maximumAmount, bool knownItemsOnly, bool countOnly, bool trashItems, bool convertCubesToItems, out ItemBase returnedItem, out ushort returnedCubeType, out ushort returnedCubeValue, out int returnedAmount)
    {
        if (lType == eHopperRequestType.eNone && exemplarItemId == -1 && exemplarCubeType == 0)
        {
            returnedItem = null;
            returnedCubeType = 0;
            returnedCubeValue = 0;
            returnedAmount = 0;
            return false;
        }
        int num = 0;
        int num2 = maximumAmount;
        bool flag = false;
        ItemBase itemBase = null;
        int num3 = 0;
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            this.mnRoundRobinOffset++;
            this.mnRoundRobinOffset %= this.mnMaxStorage;
            int num4 = this.mnRoundRobinOffset;
            ItemBase itemBase2 = this.maItemInventory[num4];
            if (itemBase2 != null)
            {
                if (exemplarItemId >= 0)
                {
                    if (itemBase2.mnItemID == exemplarItemId)
                    {
                        if (invertExemplar)
                        {
                            goto IL_55C;
                        }
                    }
                    else if (!invertExemplar)
                    {
                        goto IL_55C;
                    }
                }
                else if (exemplarCubeType > 0)
                {
                    if (itemBase2 is ItemCubeStack)
                    {
                        ItemCubeStack itemCubeStack = (ItemCubeStack)itemBase2;
                        if (itemCubeStack.mCubeType == exemplarCubeType && (itemCubeStack.mCubeValue == exemplarCubeValue || exemplarCubeValue == 65535))
                        {
                            if (invertExemplar)
                            {
                                goto IL_55C;
                            }
                        }
                        else if (!invertExemplar)
                        {
                            goto IL_55C;
                        }
                    }
                    else if (!invertExemplar)
                    {
                        goto IL_55C;
                    }
                }
                if (lType != eHopperRequestType.eAny)
                {
                    bool flag2 = false;
                    if (itemBase2.mType != ItemType.ItemCubeStack)
                    {
                        int mnItemID = itemBase2.mnItemID;
                        if (lType == eHopperRequestType.eOrganic && mnItemID >= 4000 && mnItemID <= 4101)
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eBarsOnly)
                        {
                            bool flag3 = false;
                            if (mnItemID == ItemEntries.CopperBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.TinBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.IronBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.LithiumBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.GoldBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.NickelBar)
                            {
                                flag3 = true;
                            }
                            if (mnItemID == ItemEntries.TitaniumBar)
                            {
                                flag3 = true;
                            }
                            if (flag3)
                            {
                                flag2 = true;
                            }
                        }
                        if (lType == eHopperRequestType.eAnyCraftedItem)
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eResearchable)
                        {
                            ItemEntry itemEntry = ItemEntry.mEntries[mnItemID];
                            if (itemEntry != null && itemEntry.DecomposeValue > 0)
                            {
                                flag2 = true;
                            }
                        }
                    }
                    else
                    {
                        ItemCubeStack itemCubeStack2 = (ItemCubeStack)itemBase2;
                        if (lType == eHopperRequestType.eResearchable)
                        {
                            TerrainDataEntry terrainDataEntry = global::TerrainData.mEntries[(int)itemCubeStack2.mCubeType];
                            if (terrainDataEntry != null && terrainDataEntry.DecomposeValue > 0)
                            {
                                flag2 = true;
                            }
                        }
                        if (lType == eHopperRequestType.eHighCalorieOnly && CubeHelper.IsHighCalorie(itemCubeStack2.mCubeType))
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eOreOnly && CubeHelper.IsSmeltableOre(itemCubeStack2.mCubeType))
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eGarbage && CubeHelper.IsGarbage(itemCubeStack2.mCubeType))
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eCrystals && itemCubeStack2.mCubeType == 152)
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eGems && itemCubeStack2.mCubeType == 162)
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eBioMass && itemCubeStack2.mCubeType == 153)
                        {
                            flag2 = true;
                        }
                        if (lType == eHopperRequestType.eSmeltable && CubeHelper.IsIngottableOre(itemCubeStack2.mCubeType))
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        goto IL_55C;
                    }
                }
                if (!knownItemsOnly || WorldScript.mLocalPlayer.mResearch.IsKnown(itemBase2))
                {
                    if (countOnly)
                    {
                        num += ItemManager.GetCurrentStackSize(itemBase2);
                    }
                    else
                    {
                        if (itemBase == null)
                        {
                            itemBase = itemBase2;
                        }
                        else if (!trashItems)
                        {
                            if (itemBase.mType != itemBase2.mType)
                            {
                                goto IL_55C;
                            }
                            if (itemBase.mType == ItemType.ItemCubeStack)
                            {
                                ItemCubeStack itemCubeStack3 = (ItemCubeStack)itemBase;
                                ItemCubeStack itemCubeStack4 = (ItemCubeStack)itemBase2;
                                if (itemCubeStack3.mCubeType != itemCubeStack4.mCubeType)
                                {
                                    goto IL_55C;
                                }
                                if (exemplarCubeValue != 65535 && itemCubeStack3.mCubeValue != itemCubeStack4.mCubeValue)
                                {
                                    goto IL_55C;
                                }
                            }
                            else if (itemBase.mnItemID != itemBase2.mnItemID)
                            {
                                goto IL_55C;
                            }
                        }
                        if (itemBase2.mType == ItemType.ItemCubeStack)
                        {
                            int mnAmount = ((ItemCubeStack)itemBase2).mnAmount;
                            if (mnAmount <= num2)
                            {
                                num2 -= mnAmount;
                                num += mnAmount;
                                if (minimumAmount - num - mnAmount > 0)
                                {
                                    ExtraStorageHoppers.mRemoveCache[num3] = num4;
                                    num3++;
                                }
                                else
                                {
                                    this.maItemInventory[num4] = null;
                                }
                                flag = true;
                            }
                            else
                            {
                                ((ItemCubeStack)itemBase2).mnAmount -= num2;
                                num += num2;
                                num2 = 0;
                            }
                        }
                        else if (itemBase2.mType == ItemType.ItemStack)
                        {
                            int mnAmount2 = ((ItemStack)itemBase2).mnAmount;
                            if (mnAmount2 > num2)
                            {
                                ((ItemStack)itemBase2).mnAmount -= num2;
                                num += num2;
                                break;
                            }
                            num2 -= mnAmount2;
                            num += mnAmount2;
                            if (minimumAmount - num - mnAmount2 > 0)
                            {
                                ExtraStorageHoppers.mRemoveCache[num3] = num4;
                                num3++;
                            }
                            else
                            {
                                this.maItemInventory[num4] = null;
                            }
                            flag = true;
                        }
                        else
                        {
                            if (!trashItems)
                            {
                                returnedItem = itemBase;
                                num = 1;
                                this.maItemInventory[num4] = null;
                                flag = true;
                                break;
                            }
                            num++;
                            num2--;
                            if (minimumAmount - num - 1 > 0)
                            {
                                ExtraStorageHoppers.mRemoveCache[num3] = num4;
                                num3++;
                            }
                            else
                            {
                                this.maItemInventory[num4] = null;
                            }
                            flag = true;
                        }
                        if (num2 <= 0)
                        {
                            break;
                        }
                    }
                }
            }
            IL_55C:;
        }
        for (int j = 0; j < num3; j++)
        {
            this.maItemInventory[ExtraStorageHoppers.mRemoveCache[j]] = null;
        }
        if (itemBase != null && !countOnly)
        {
            if (!trashItems)
            {
                returnedItem = itemBase;
                if (flag)
                {
                    if (itemBase.mType == ItemType.ItemCubeStack)
                    {
                        ItemCubeStack itemCubeStack5 = (ItemCubeStack)itemBase;
                        itemCubeStack5.mnAmount = num;
                        returnedItem = itemCubeStack5;
                        returnedCubeType = itemCubeStack5.mCubeType;
                        returnedCubeValue = itemCubeStack5.mCubeValue;
                        returnedAmount = num;
                    }
                    else if (itemBase.mType == ItemType.ItemStack)
                    {
                        ItemStack itemStack = (ItemStack)itemBase;
                        itemStack.mnAmount = num;
                        returnedItem = itemStack;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = num;
                    }
                    else
                    {
                        returnedItem = itemBase;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = 1;
                    }
                }
                else if (itemBase.mType == ItemType.ItemCubeStack)
                {
                    ItemCubeStack itemCubeStack6 = (ItemCubeStack)itemBase;
                    if (convertCubesToItems)
                    {
                        ItemCubeStack itemCubeStack7 = (ItemCubeStack)ItemManager.CloneItem(itemCubeStack6, num);
                        returnedItem = itemCubeStack7;
                    }
                    else
                    {
                        returnedItem = null;
                    }
                    returnedCubeType = itemCubeStack6.mCubeType;
                    returnedCubeValue = itemCubeStack6.mCubeValue;
                    returnedAmount = num;
                }
                else if (itemBase.mType == ItemType.ItemStack)
                {
                    ItemStack itemStack2 = (ItemStack)ItemManager.CloneItem(itemBase, num);
                    returnedItem = itemStack2;
                    returnedCubeType = 0;
                    returnedCubeValue = 0;
                    returnedAmount = num;
                }
                else
                {
                    LogError_AW("Non-stack matched item was not removed from the hopper inventory during extract?");
                    returnedItem = itemBase;
                    returnedCubeType = 0;
                    returnedCubeValue = 0;
                    returnedAmount = 1;
                }
            }
            else
            {
                returnedItem = null;
                returnedCubeType = 0;
                returnedCubeValue = 0;
                returnedAmount = num;
            }
            this.CountFreeSlots();
            this.MarkDirtyDelayed();
            //this.LogisticsOperation();
            this.RequestImmediateNetworkUpdate();
            return true;
        }
        if (countOnly)
        {
            returnedItem = null;
            returnedCubeType = 0;
            returnedCubeValue = 0;
            returnedAmount = num;
            return num > 0;
        }
        returnedItem = null;
        returnedCubeType = 0;
        returnedCubeValue = 0;
        returnedAmount = 0;
        return false;
    }
    /// <summary>
	/// Attempts to deliver the specified item or cube from the source entity to the consumer
	/// of this interface. If the delivery is successful <c>true</c> will be returned.
	/// </summary>
	/// <returns><c>true</c>, if delivery of item was successful, <c>false</c> otherwise.</returns>
	/// <param name="sourceEntity">Source entity.</param>
	/// <param name="item">Item.</param>
	/// <param name="cubeType">Cube type.</param>
	/// <param name="cubeValue">Cube value.</param>
	/// <param name="sendImmediateNetworkUpdate">Sends an immediate network update.</param>
	public bool TryDeliverItem(StorageUserInterface sourceEntity, ItemBase item, ushort cubeType, ushort cubeValue, bool sendImmediateNetworkUpdate)
    {
        //this.LogError("TryDeliverItem was called by " + sourceEntity);
        if (mnStorageFree > 0)
        {
            if (item != null)
            {
                if (!CheckExemplar(item))
                {
                    return false;
                }
                AddItem(item);
            }
            else
            {
                if (!CheckExemplar(cubeType, cubeValue))
                {
                    return false;
                }
                AddCube(cubeType, cubeValue);
            }

            if (sendImmediateNetworkUpdate)
            {
                RequestImmediateNetworkUpdate();
            }
            return true;
        }

        return false;
    }

    public bool TryExtractCubes(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, -1, cube, value, false, amount, amount, false, false, false, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public bool TryExtractItems(StorageUserInterface sourceEntity, int itemId, int amount, out ItemBase item)
    {
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, itemId, 0, 0, false, amount, amount, false, false, false, true, out item, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public bool TryExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount, out ItemBase item)
    {
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, itemId, cube, value, false, amount, amount, false, false, false, true, out item, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public bool TryExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, itemId, cube, value, false, amount, amount, false, false, true, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public bool TryExtractItems(StorageUserInterface sourceEntity, int itemId, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, itemId, 0, 0, false, amount, amount, false, false, true, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public int TryPartialExtractCubes(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        TryExtract(eHopperRequestType.eAny, -1, cube, value, false, amount, amount, false, false, false, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);

        return returnedAmount;
    }

    public int TryPartialExtractItems(StorageUserInterface sourceEntity, int itemId, int amount, out ItemBase item)
    {
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        TryExtract(eHopperRequestType.eAny, itemId, 0, 0, false, amount, amount, false, false, false, true, out item, out returnedCubeType, out returnedCubeValue, out returnedAmount);

        return returnedAmount;
    }

    public int TryPartialExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount, out ItemBase item)
    {
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        TryExtract(eHopperRequestType.eAny, itemId, cube, value, false, amount, amount, false, false, false, true, out item, out returnedCubeType, out returnedCubeValue, out returnedAmount);

        return returnedAmount;
    }

    public int TryPartialExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        TryExtract(eHopperRequestType.eAny, itemId, cube, value, false, amount, amount, false, false, false, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);

        return returnedAmount;
    }

    public int TryPartialExtractItems(StorageUserInterface sourceEntity, int itemId, int amount)
    {
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        TryExtract(eHopperRequestType.eAny, itemId, 0, 0, false, amount, amount, false, false, true, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);

        return returnedAmount;
    }

    public bool TryExtractAny(StorageUserInterface sourceEntity, int amount, out ItemBase item)
    {
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        return TryExtract(eHopperRequestType.eAny, -1, 0, 0, false, amount, amount, false, false, false, true, out item, out returnedCubeType, out returnedCubeValue, out returnedAmount);
    }

    public bool TryInsert(InventoryInsertionOptions options, ref InventoryInsertionResults results)
    {
        LogError("This is called![1]");
        // Check we have not run out of space.
        if (mnStorageFree <= 0)
        {
            return false;
        }
        //IF FALSE 
        //IF its a OT hopper, its allways true
        //IF its true exemplar its allways true
        //IF its both its false
        if (CheckExemplar(options.Item) == false)
        {
            return false;
        }
        if (options.Item != null)
        {
            // Get the number of items represented by this item.
            int totalItemCount = ItemManager.GetCurrentStackSize(options.Item);

            int insertItemCount = totalItemCount;

            ItemBase item;
            if (totalItemCount > mnStorageFree)
            {
                if (!options.AllowPartialInsertion)
                {
                    // Partial insertion is not allowed.
                    return false;
                }

                insertItemCount = mnStorageFree;
                item = ItemManager.CloneItem(options.Item, insertItemCount);
            }
            else
            {
                item = options.Item;
            }

            if (false == AddItem(item))
            {
                // This should not happen. Race condition?
                return false;
            }

            // Only create results if required.
            if (results == null)
                results = new InventoryInsertionResults();

            results.AmountInserted = insertItemCount;
            results.AmountRemaining = totalItemCount - insertItemCount;
            return false;
        }
        else
        {
            if (options.Amount == 1)
            {
                AddCube(options.Cube, options.Value);

                if (results == null)
                    results = new InventoryInsertionResults();

                results.AmountInserted = 1;
                return true;
            }
            else
            {
                // TODO: More efficient way of inserting multiple items.
                if (options.Amount > mnStorageFree && !options.AllowPartialInsertion)
                {
                    return false;
                }

                int remaining = options.Amount;

                while (remaining > 0 && mnStorageFree > 0)
                {
                    AddCube(options.Cube, options.Value);
                    remaining--;
                }

                if (results == null)
                    results = new InventoryInsertionResults();

                results.AmountInserted = options.Amount - remaining;
                results.AmountRemaining = remaining;
                return true;
            }
        }

        //		if (maAttachedHoppers[0].mnStorageFree <=0)
        //		{
        //			LogError_AW("Derp, how did a Quarry pick a full hopper to empty into?");
        //		}
        //		else
        //		{
        //			maAttachedHoppers[0].AddCube(mCarryCube,mCarryValue);
        //		}
    }

    public bool TryInsert(StorageUserInterface sourceEntity, ItemBase item)
    {
        if (CheckExemplar(item) == false)
        {
            return false;
        }
        return AddItem(item);
    }

    public bool TryInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        if (mnStorageFree < amount)
        {
            return false;
        }
        if (CheckExemplar(cube, value) == false)
        {
            return false;
        }
        if (amount == 1)
        {
            AddCube(cube, value);
            return true;
        }

        int remaining = amount;

        while (remaining > 0 && mnStorageFree > 0)
        {
            AddCube(cube, value);
            remaining--;
        }

        return true;
    }

    public int TryPartialInsert(StorageUserInterface sourceEntity, ref ItemBase item, bool alwaysCloneItem, bool updateSourceItem)
    {
        int currentStackSize = ItemManager.GetCurrentStackSize(item);
        int num = currentStackSize;
        if (this.mnStorageFree == 0)
        {
            return 0;
        }
        if (CheckExemplar(item) == false)
        {
            return 0;
        }
        ItemBase lItemToAdd;
        if (currentStackSize > this.mnStorageFree)
        {
            num = this.mnStorageFree;
            lItemToAdd = ItemManager.CloneItem(item, num);
        }
        else if (alwaysCloneItem)
        {
            lItemToAdd = ItemManager.CloneItem(item, currentStackSize);
        }
        else
        {
            lItemToAdd = item;
        }
        if (!this.AddItem(lItemToAdd))
        {
            return 0;
        }
        if (updateSourceItem)
        {
            if (currentStackSize > this.mnStorageFree)
            {
                ItemManager.SetItemCount(item, currentStackSize - num);
            }
            else
            {
                item = null;
            }
        }
        return num;
    }

    public int TryPartialInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        if (CheckExemplar(cube, value) == false)
        {
            return amount;
        }

        int remaining = amount;

        while (remaining > 0 && mnStorageFree > 0)
        {
            AddCube(cube, value);
            remaining--;
        }

        return amount - remaining;
    }

    // ***************************************************************************************************************************************
    // convenience function for unloading to cargo lifts
    public int UnloadToList(List<ItemBase> cargoList, int amountToExtract)
    {
        // dump our cargo into the given list, until the amount is met
        int amountLeft = amountToExtract;
        for (int i = 0; i < maItemInventory.Length; i++)
        {
            ItemBase item = maItemInventory[i];

            if (item == null) continue;

            int count = ItemManager.GetCurrentStackSize(item);

            if (count > 0)
            {
                if (count > amountLeft)
                {
                    // crop old item
                    ItemManager.SetItemCount(item, count - amountLeft);

                    // create new item
                    ItemBase newItem = ItemManager.CloneItem(item);
                    ItemManager.SetItemCount(newItem, amountLeft);

                    ItemManager.FitCargo(cargoList, newItem);

                    FinaliseHopperChange();

                    // we're done
                    return amountToExtract;
                }
                else
                {
                    // add entire item
                    amountLeft -= count;
                    ItemManager.FitCargo(cargoList, item);
                    maItemInventory[i] = null;
                }
            }
        }
        FinaliseHopperChange();
        return amountToExtract - amountLeft;
    }
    // ***************************************************************************************************************************************
    void FinaliseHopperChange()
    {
        MarkDirtyDelayed();
        RequestImmediateNetworkUpdate();
        CountFreeSlots();
    }

    //***************************************************************************************************************************************
    //********************************************************************************
    //******************** STORAGEUSERINTERFACE (NEW STORAGE API) ********************
    //********************************************************************************
    //******************** ENUM'S ********************
    public enum eRequestType
    {
        eAny,
        eHighCalorieOnly,
        eGarbage,
        eOreOnly,
        eBarsOnly,
        eAnyCraftedItem,
        eCrystals,
        eBioMass,
        eNone,
        eGems,
        eOrganic,
        eSmeltable,
        eNum
    }

    public enum eHopperMode
    {
        eNone,
        eEmpty,
        eFill,
        eNum
    }


}


