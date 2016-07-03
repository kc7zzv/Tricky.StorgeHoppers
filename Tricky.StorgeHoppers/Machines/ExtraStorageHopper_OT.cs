using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class ExtraStorageHoppers_OT : global::MachineEntity, ItemConsumerInterface, StorageMachineInterface, StorageInventoryIterationInterface
{
    //MY STUFF
    private string ModName = Variables.ModName;
    private string ModVersion = Variables.ModVersion;
    private string PopUpText;
    //STORAGE HOPPER STUFF
    private Segment[] CheckSegments;
    private Segment[,,] HooverSegment;
    public ItemBase[] maItemInventory;
    public ushort[] maStorage;
    public bool mbAllowLogistics;
    private bool mbForceHoloUpdate;
    private bool mbForceTextUpdate;
    public bool mbHooverOn;
    private bool mbLinkedToGO;
    private bool mbShowHopper;
    private GameObject mHoloStatus;
    private ParticleSystem mHooverPart;
    private GameObject mHopperPart;
    public string mLastItemAdded;
    private int mnHooverEmissionRate;
    private int mnLowFrequencyUpdates;
    private int mnMaxStorage;
    private int mnReadouts;
    private int mnRoundRobinOffset;
    public int mnStorageFree;
    public int mnStorageUsed;
    static int[] mRemoveCache = new int[100];
    private int mnUpdates;
    public eHopperPermissions mPermissions = eHopperPermissions.AddAndRemove;
    private eHopperPermissions mPreviousPermissions;
    public float mrCurrentPower;
    public float mrCurrentTemperature;
    public float mrExtractionTime;
    private float mrLogisticsDebounce;
    private float mrMaxLightDistance;
    private float mrPrevDistanceToPlayer;
    public float mrReadoutTick;
    private float mrSpoilTimer;
    private float mrTimeElapsed;
    private float mrTimeSinceLogistics;
    private float mrTimeUntilPlayerDistanceUpdate;
    private TextMesh mTextMesh;
    private Light WorkLight;

    //UI STUFF
    public static bool AllowBuilding = true;
    public static bool AllowInteracting = true;
    public static bool AllowLooking = true;
    public static bool AllowMovement = true;
    public static float ForceNGUIUpdate;
    private float mRetakeDebounce;
    // More of my stuff
    private ushort CubeValue;
    private Color mCubeColor;
	private int ItemsDeleted;
    private string HopperName;

    //One Type Stuff
    public ushort ExemplarBlockID = 0;
    public ushort ExemplarBlockValue = 0;
    public int ExemplarItemID = -1;
    public ItemBase ExemplarItemBase;
    public string ExemplarString = "None";

    //FUNCTIONS
    public ExtraStorageHoppers_OT(Segment segment, long x, long y, long z, ushort cube, byte flags, ushort lValue, bool lbFromDisk) : base(eSegmentEntity.Mod, SpawnableObjectEnum.LogisticsHopper, x, y, z, cube, flags, lValue, Vector3.zero, segment)
    {
        this.HopperName = "New Hopper";
        this.mnMaxStorage = 2001;
        this.mrExtractionTime = 15f;
        this.mrSpoilTimer = 30f;
        this.mrMaxLightDistance = 32f;
        this.mPreviousPermissions = eHopperPermissions.eNumPermissions;
        base.mbNeedsLowFrequencyUpdate = true;
        base.mbNeedsUnityUpdate = true;

        this.maStorage = new ushort[this.mnMaxStorage];
        for (int i = 0; i < this.mnMaxStorage; i++)
        {
            this.maStorage[i] = 0;
        }
        this.maItemInventory = new ItemBase[this.mnMaxStorage];
        string key = TerrainData.GetCubeKey(cube, lValue);
        if (key == "Tricky.2000SlotHopper")
        {
            this.HopperName = "2000 Slot OT Storage Hopper";
            this.CubeValue = 1;
            this.mnMaxStorage = 2000;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 0.5f);
        }
        if (key == "Tricky.1000SlotHopper")
        {
            this.HopperName = "1000 Slot OT Storage Hopper";
            this.CubeValue = 2;
            this.mnMaxStorage = 1000;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 1.0f);
        }
        if (key == "Tricky.500SlotHopper")
        {
            this.HopperName = "500 Slot OT Storage Hopper";
            this.CubeValue = 3;
            this.mnMaxStorage = 500;
            this.mrLogisticsDebounce = 0f;
            this.mCubeColor = new Color(2f, 2f, 1.5f);
        }
        this.CountFreeSlots();
        this.CheckSegments = new Segment[6];
    }

    

    public override string GetPopupText()
    {
        this.mRetakeDebounce -= Time.deltaTime;
        ushort SelectedBlock = WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectBlockType;
        if ((SelectedBlock == GetCubeType("Tricky.ExtraStorageHoppers_OT")))
        {
            ExtraStorageHoppers_OT selectedEntity = (ExtraStorageHoppers_OT)WorldScript.instance.localPlayerInstance.mPlayerBlockPicker.selectedEntity;
            if (selectedEntity != null)
            {
                string lStr = string.Empty;
                lStr = this.HopperName;
                if (selectedEntity.mLastItemAdded != null)
                {
                    lStr = lStr + "\nLast Item : " + selectedEntity.mLastItemAdded;
                }
                string str2 = lStr;
                object[] objArray1 = new object[] { str2, "\nUsed:", selectedEntity.mnStorageUsed, ". Free:", selectedEntity.mnStorageFree, "\n(E)Toggle Lock Status : ", selectedEntity.mPermissions };

                lStr = string.Concat(objArray1);
                lStr = lStr + "\n(Shift+X)Set Storage Type : [" + (this.ExemplarString) + "]";
                int lnAvailable = 0;
                ItemBase currentHotBarItemOrCubeAsItem = this.GetCurrentHotBarItemOrCubeAsItem(out lnAvailable, true);
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
                    int mnStorageFree = selectedEntity.mnMaxStorage - selectedEntity.mnStorageUsed;
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
                    ItemManager.SetItemCount(currentHotBarItemOrCubeAsItem, amount);
                    if (!this.CheckExemplar(currentHotBarItemOrCubeAsItem))
                    {
                        //lStr = lStr + "\nUnable to add " + currentHotBarItemOrCubeAsItem.GetDisplayString() + " because its not the correct type";
                    }

                    else {
                        int maxStackSize = ItemManager.GetMaxStackSize(currentHotBarItemOrCubeAsItem);
                        if (amount > maxStackSize)
                        {
                            amount = maxStackSize;
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
                }
                if ((selectedEntity.mnStorageUsed > 0) && (this.CubeValue != 2))
                {
                    lStr = lStr + "\n(Q) to retrieve contents!";
                }
                if (NetworkManager.mbClientRunning)
                {
                    lStr = lStr + "\nNetworkSync : " + selectedEntity.mrNetworkSyncTimer.ToString("F2");
                }
                PopUpText = lStr;
                if (AllowMovement)
                {

                    if ((Input.GetButton("Extract")) && ExtraStorageHopperWindow_OT.TakeItems(WorldScript.mLocalPlayer, selectedEntity))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.OrePickup();
                        if ((WorldScript.meGameMode == eGameMode.eSurvival) && (SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.RemoveCoalFromHopper))
                        {
                            SurvivalPlayerScript.TutorialSectionComplete();
                        }
                        this.mRetakeDebounce = 0.5f;
                    }
                    if ((Input.GetButtonDown("Build Gun") && Input.GetKey(KeyCode.LeftShift) && currentHotBarItemOrCubeAsItem != null))
                    {

                        if (this.mnStorageUsed == 0)
                        {
                            this.SetExemplar(currentHotBarItemOrCubeAsItem);
                            ExtraStorageHopperWindow_OT.SetNewExamplar(WorldScript.mLocalPlayer, selectedEntity, currentHotBarItemOrCubeAsItem);
                        }
                        else
                        {
                            ExtraStorageHopperWindow_OT.SetNewExamplar_Fail(WorldScript.mLocalPlayer, selectedEntity);
                        }
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                    }

                    if ((Input.GetButtonDown("Interact") && Input.GetKey(KeyCode.LeftShift)) && (AllowInteracting && ExtraStorageHopperWindow_OT.ToggleHoover(WorldScript.mLocalPlayer, selectedEntity)))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                    }

                    if ((Input.GetButtonDown("Interact") && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl)) && (AllowInteracting && ExtraStorageHopperWindow_OT.TogglePermissions(WorldScript.mLocalPlayer, selectedEntity)))
                    {
                        ForceNGUIUpdate = 0.1f;
                        AudioHUDManager.instance.HUDIn();
                        AudioSpeechManager.instance.UpdateStorageHopper(selectedEntity.mPermissions);
                    }
                    if ((Input.GetButtonDown("Store") && AllowInteracting) && ((selectedEntity.mnStorageFree > 0) && (currentHotBarItemOrCubeAsItem != null)) && (CheckExemplar(currentHotBarItemOrCubeAsItem)))
                    {
                        if (ExtraStorageHopperWindow_OT.StoreItems(WorldScript.mLocalPlayer, selectedEntity, currentHotBarItemOrCubeAsItem))
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
        else
        {
            this.mRetakeDebounce = 0f;
        }

        return PopUpText;
    }

    public override void SpawnGameObject()
    {
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

    public override void LowFrequencyUpdate()
    {
        //-------------------------------------------------------------
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


        if (mrTimeSinceLogistics > 0.0f)
        {
            mrTimeSinceLogistics -= LowFrequencyThread.mrPreviousUpdateTimeStep;
            mbAllowLogistics = false;
        }
        else
        {
            mbAllowLogistics = true;
        }

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

        UpdateHoover();

        UpdatePoweredHopper();//Keep the client sim in step

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

            //In museum mode, we continually empty out slots, just in case;this is hooked up to the 1 second readout tick 
            //Which is actually the tick where we look for places to hand out stuff. 
#if MUSEUM
	//	public ItemBase[] maItemInventory;
		maItemInventory[0] = null;
		maStorage[99] = eCubeTypes.NULL;//this allows it to fill up first
		CountFreeSlots();
#endif


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

    //******************** HF CHECKER AND CUSUMER COMMENTED **********************

    private void HFCheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        //Conveyor
        //lCube == 513
        if ((lCube == 513) && WorldScript.mbIsServer)
        {
            ConveyorEntity Conveyor = checkSegment.FetchEntity(eSegmentEntity.Conveyor, checkX, checkY, checkZ) as ConveyorEntity;
            Vector3 lVTT = new Vector3(mnX - Conveyor.mnX, mnY - Conveyor.mnY, mnZ - Conveyor.mnZ);
            float lrDot = Vector3.Dot(lVTT, Conveyor.mForwards);
            //Cubes
            if ((Conveyor != null) && Conveyor.mbReadyToConvey && (lrDot != 1) && (!Conveyor.IsCarryingCargo()) && (Conveyor.mValue != 12))
            {
                if (this.mnStorageUsed > 0)
                {
                    ushort cube;
                    ushort value;
                    this.GetSpecificCube((ExtraStorageHoppers_OT.eRequestType) Conveyor.meRequestType, out cube, out value);
                    if (cube != 0)
                    {
                        Conveyor.AddCube(cube, value, 1f);
                        this.CountFreeSlots();
                        this.MarkDirtyDelayed();
                        return;
                    }

                }
            }
            else if ((Conveyor != null) && Conveyor.mbReadyToConvey && (lrDot != 1) && (!Conveyor.IsCarryingCargo()) && (Conveyor.mValue == 12) && (this.CountHowManyOfType(Conveyor.ExemplarBlockID, Conveyor.ExemplarBlockValue) > 0))
            {

                if (this.mnStorageUsed > 0)
                {
                    if (Conveyor.ExemplarBlockID != 0)
                    {

                        ItemCubeStack lItem = ItemManager.SpawnCubeStack(Conveyor.ExemplarBlockID, Conveyor.ExemplarBlockValue, 1);
                        Conveyor.AddItem(this.RemoveSingleSpecificCubeStack(lItem, Conveyor.mbInvertExemplar));
                        this.CountFreeSlots();

                        this.MarkDirtyDelayed();
                        return;
                    }

                }
            }
            if ((Conveyor != null) && Conveyor.mbReadyToConvey && (lrDot != 1) && (!Conveyor.IsCarryingCargo()) && (Conveyor.mValue != 12) && (this.mnStorageUsed > 0))
            {
                ItemBase item = this.RemoveSingleSpecificItemOrCubeRoundRobin((ExtraStorageHoppers_OT.eRequestType) Conveyor.meRequestType);
                if (item != null)
                {
                    Conveyor.AddItem(item);
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                }
            }
            else if ((Conveyor != null) && (Conveyor.mbReadyToConvey) && (lrDot != 1) && (!Conveyor.IsCarryingCargo()) && (Conveyor.mValue == 12) && (this.mnStorageUsed > 0) && (Conveyor.ExemplarItemID != -1) && (this.CountHowManyOfType(Conveyor.ExemplarBlockID, Conveyor.ExemplarBlockValue) > 0))
            {
                ItemBase item = this.RemoveSingleSpecificItemByID(Conveyor.ExemplarItemID, Conveyor.mbInvertExemplar);
                if (item != null)
                {
                    Conveyor.AddItem(item);
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                }
            }
        }
    }

    private void HFCheckSuppliers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if ((this.mPermissions != eHopperPermissions.Locked) && (this.mPermissions != eHopperPermissions.RemoveOnly))
        {
            //Conveyors
            if ((lCube == 513) && (this.mnStorageFree > 0) && (this.mPermissions != eHopperPermissions.Locked))
            {
                ConveyorEntity Conveyor = checkSegment.FetchEntity(eSegmentEntity.Conveyor, checkX, checkY, checkZ) as ConveyorEntity;
                Vector3 lVTT = new Vector3(mnX - Conveyor.mnX, mnY - Conveyor.mnY, mnZ - Conveyor.mnZ);
                float lrDot = Vector3.Dot(lVTT, Conveyor.mForwards);
                if ((Conveyor != null) && (Conveyor.IsCarryingCargo()) && (lrDot == 1) && (this.mnStorageFree > 0))
                {
                    ushort CubeType;
                    ushort CubeValue;
                    TerrainData.GetCubeForName(this.ExemplarString, out CubeType, out CubeValue);
                    if ((Conveyor.mCarriedCube != 0) && (Conveyor.mrCarryTimer <= 0.2f) && (Conveyor.mCarriedCube == CubeType) && (Conveyor.mCarriedValue == CubeValue))
                    {
                        this.AddCube(Conveyor.mCarriedCube, Conveyor.mCarriedValue);
                        Conveyor.RemoveCube();
                        Conveyor.FinaliseOffloadingCargo();
                    }
                    else if ((Conveyor.mCarriedItem != null) && (Conveyor.mrCarryTimer <= 0.2f) && (ItemManager.GetItemName(Conveyor.mCarriedItem) == this.ExemplarString))
                    {
                        this.AddItem(Conveyor.mCarriedItem);
                        Conveyor.RemoveItem();
                        Conveyor.FinaliseOffloadingCargo();
                    }
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return;
                }
            }
        }
    }

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
                    if (this.mnStorageFree > 0)
                    {
                        this.HFCheckSuppliers(this.CheckSegments[index], lType, x, y, z);
                    }
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
            UnityEngine.Debug.LogError("No cube and no item in hotbar?");
        }
        return null;
    }

    //******************** GET AND GIVE ITEMS **********************

    void CheckSuppliers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if (mPermissions == eHopperPermissions.Locked || mPermissions == eHopperPermissions.RemoveOnly)
            return;

        // No point calling any suppliers if we have no space left anyway.
        if (mnStorageFree <= 0)
            return;

        var targetEntity = checkSegment.SearchEntity(checkX, checkY, checkZ) as StorageSupplierInterface;
    }

    void CheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if (mPermissions == eHopperPermissions.Locked || mPermissions == eHopperPermissions.AddOnly)
            return;

        var targetEntity = checkSegment.SearchEntity(checkX, checkY, checkZ) as StorageConsumerInterface;
    }

    //******************** ADD ITEMS AND CUBES **********************

    public void AddCube(ushort lType, ushort lValue)
    {
        if (lType == 0)
        {
            Debug.LogError("Who and why is someone adding NULL to a Storage Hopper?");
        }
        if (this.mnStorageFree <= 0)
        {
            Debug.LogError("Error, can't AddCube " + lType + " to hopper, it's full!");
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
                        Debug.LogError(string.Concat(new object[] { "Error, failed to convert item into ItemCubeStack", base2.mType, ":", base2.mnItemID }));
                        return;
                    }
                    if ((stack.mCubeType == lType) && (stack.mCubeValue == lValue))
                    {
                        stack.mnAmount++;
                        if (WorldScript.mLocalPlayer.mResearch.IsKnown(lType, 0))
                        {
                            if (lType >= TerrainData.mEntries.Length)
                            {
                                Debug.LogError(string.Concat(new object[] { "Error, AddCube tried to get terrain data entry for", lType, " but max was only ", TerrainData.mEntries.Length }));
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
        if (lItemToAdd.mnItemID != ExemplarItemID)
        {
            return false;
        }
        if (lItemToAdd == null)
        {
            return true;
        }
        this.CountFreeSlots();
        if (this.mnStorageFree > 0)
        {
            if (ItemManager.GetCurrentStackSize(lItemToAdd) > this.mnStorageFree)
            {
                return false;
            }
            if ((lItemToAdd.mType == ItemType.ItemStack) && ((lItemToAdd as ItemStack).mnAmount == 0))
            {
                Debug.LogError("Error, attempting to add an ItemStack of ZERO to the SH?![" + ItemManager.GetItemName(lItemToAdd) + "]");
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
            Debug.LogError("Attempted to remove item but hopper was empty - are you dumb?");
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
                    Debug.LogError("Errror, RemoveInventoryItem does not support cubestacks");
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
            Debug.LogError("There's probably a good reason why RemoveSingleSpecificCubeStack is looking for a null Item");
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
            Debug.LogError("There's probably a good reason why RemoveSingleSpecificItemByID is looking for ItemID -1");
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
            Debug.LogError("Error, unable to locate ItemID for Bar from type " + lType);
        }
        else
        {
            Debug.LogError("Error, recipes for smelter were not found. Note this can happen in editor when opening the handbook.");
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
                Debug.LogError("Error stack was null in Contains Ore?");
                continue;
            }
            if (WorldScript.mLocalPlayer.mResearch == null)
            {
                Debug.LogError("mResearch stack was null in Contains Ore?");
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
            Debug.LogError(string.Concat(new object[] { "Storage hopper has overflowed! ", this.mnStorageUsed, "/", this.mnMaxStorage, ".", Environment.StackTrace, "Last Item was ", this.mLastItemAdded }));
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
            Debug.LogError(string.Concat(new object[] { "Storage hopper has overflowed! ", this.mnStorageUsed, "/", this.mnMaxStorage, ".", Environment.StackTrace, "Last Item was ", this.mLastItemAdded }));
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
            // Debug.LogError("This is only for Ores and other things that we DO NOT CARE ABOUT THE VALUE OF");
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


    //******************** HANDLE MODELS  ********************

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
        if (mTextMesh.GetComponent<Renderer>().enabled && mDistanceToPlayer < 12)//we can see text up to 24m right now, but there's little point, it's not readable
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
            if (mWrapper.mGameObjectList == null) Debug.LogError("Ore Extractor missing game object #0?");
            if (mWrapper.mGameObjectList[0].gameObject == null) Debug.LogError("Ore Extractor missing game object #0 (GO)?");
            //WorkLight = mWrapper.mGameObjectList[0].gameObject.GetComponentInChildren<Light>();
            WorkLight = mWrapper.mGameObjectList[0].transform.Search("HooverGraphic").GetComponent<Light>();
            if (WorkLight == null) Debug.LogError("Storage Hopper has missing light?");
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

            MeshRenderer lRenderer = this.mHopperPart.GetComponent<MeshRenderer>();
            MeshRenderer Render2 = this.mHopperPart.GetComponent<MeshRenderer>();
            Render2.material.SetColor("_Color", this.mCubeColor);
        }
    }

    public override void DropGameObject()
    {
        base.DropGameObject();
        mbLinkedToGO = false;
    }

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

    private void UpdateHoover()
    {
        if (WorldScript.mbIsServer && this.mbHooverOn)
        {
            if (this.HooverSegment == null)
            {
                this.HooverSegment = new Segment[3, 3, 3];
            }
            if ((this.mnStorageFree > 0) && this.mbHooverOn)
            {
                SegmentUpdater.mnNumHoovers++;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            if (this.HooverSegment[i + 1, j + 1, k + 1] == null)
                            {
                                long x = base.mnX + (i * 0x10);
                                long y = base.mnY + (j * 0x10);
                                long z = base.mnZ + (k * 0x10);
                                Segment segment = base.AttemptGetSegment(x, y, z);
                                if (((segment != null) && segment.mbInitialGenerationComplete) && !segment.mbDestroyed)
                                {
                                    this.HooverSegment[i + 1, j + 1, k + 1] = segment;
                                }
                                return;
                            }
                            DroppedItemData data = ItemManager.instance.UpdateCollectionSpecificSegment(base.mnX, base.mnY + 1L, base.mnZ, new Vector3(0.5f, 0f, 0.5f), 12f, 1f, 2f, this.HooverSegment[i + 1, j + 1, k + 1], this.mnStorageFree);
                            if ((data != null) && !this.AddItem(data.mItem))
                            {
                                ItemManager.instance.DropItem(data.mItem, base.mnX, base.mnY + 1L, base.mnZ, Vector3.up);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }


    private void UpdatePoweredHopper()
    {

    }

    private void UpdateSpoilage()
    {
        this.mrSpoilTimer = 30f;
    }

    //******************** READ/WRITE ********************

    public override void Write(BinaryWriter writer)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        writer.Write(this.ExemplarItemID);                      //1
        Variables.LogValue("*Writer* this.ExemplarItemID", this.ExemplarItemID);
        writer.Write(this.ExemplarString);                      //2
        Variables.LogValue("*Writer* this.ExemplarString", this.ExemplarString);
        writer.Write(this.ExemplarBlockID);                     //3
        Variables.LogValue("*Writer* this.ExemplarBlockID", this.ExemplarBlockID);
        writer.Write(this.ExemplarBlockValue);                  //4
        Variables.LogValue("*Writer* this.ExemplarBlockValue", this.ExemplarBlockValue);
        writer.Write(this.mnStorageUsed);                       //5
        writer.Write((uint)this.mPermissions); 		            //6
        writer.Write(this.mbHooverOn);				            //7
        writer.Write(this.mrCurrentPower);			            //8
        writer.Write(this.mrCurrentTemperature);                //9
        writer.Write((byte)0);                                  //10
        writer.Write((byte)0);                                  //11
        writer.Write((byte)0);                                  //12
        writer.Write(0);							            //13
        writer.Write(0);							            //14
        writer.Write(0);							            //15
        writer.Write(0);                                        //16
        stopwatch.Stop();
        Variables.Log("[" + this.HopperName + "] Writer done in " + stopwatch.ElapsedMilliseconds + ", writerLocation = " + writer.BaseStream.Position);
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        this.ExemplarItemID = reader.ReadInt32();               //1
        Variables.LogValue("*Reader* this.ExemplarItemID", this.ExemplarItemID);
        this.ExemplarString = reader.ReadString();              //2
        Variables.LogValue("*Reader* this.ExemplarString", this.ExemplarString);
        this.ExemplarBlockID = reader.ReadUInt16();             //3
        Variables.LogValue("*Reader* this.ExemplarBlockID", this.ExemplarBlockID);
        this.ExemplarBlockValue = reader.ReadUInt16();          //4
        Variables.LogValue("*Reader* this.ExemplarBlockValue", this.ExemplarBlockValue);
        int StorageUsed = reader.ReadInt32();                   //5
        this.mPermissions = (eHopperPermissions)reader.ReadUInt16();  //6
        this.mbHooverOn = reader.ReadBoolean();                 //7
        this.mrCurrentPower = reader.ReadSingle();              //8
        this.mrCurrentTemperature = reader.ReadSingle();        //9
        reader.ReadByte();                                      //10
        reader.ReadByte();                                      //11       
        reader.ReadByte();                                      //12        
        reader.ReadInt32();                                     //13
        reader.ReadInt32();                                     //14       
        reader.ReadInt32();                                     //15
        reader.ReadInt32();                                     //16
        stopwatch.Stop();
        Variables.Log("[" + this.HopperName + "] Reading done in " + stopwatch.ElapsedMilliseconds + ", readerLocation = " + reader.BaseStream.Position);

        //ALL THE READING DONE
        this.mnStorageUsed = StorageUsed;
        this.mnStorageFree = this.mnMaxStorage - StorageUsed;
        if (this.mnStorageUsed > this.mnMaxStorage)
        {
            this.mnStorageUsed = this.mnMaxStorage;
        }
        if (StorageUsed > 0)
        {
            if (this.ExemplarItemID > 0)
            {
                int ItemID = ItemEntry.GetIDFromName(this.ExemplarString, true);
                ItemBase HopperItem = ItemManager.SpawnItem(ItemID);
                ItemManager.SetItemCount(HopperItem, this.mnStorageUsed);
                this.AddItem(HopperItem);
            }
            else if (this.ExemplarBlockID != 0)
            {
                ushort CubeType;
                ushort CubeValue;
                TerrainData.GetCubeForName(this.ExemplarString, out CubeType, out CubeValue);
                //CubeValue = TerrainData.GetDefaultValue(CubeType);
                ItemCubeStack HopperCube = ItemManager.SpawnCubeStack(CubeType, CubeValue, 1);
                ItemManager.SetItemCount(HopperCube, this.mnStorageUsed);
                this.AddItem(HopperCube);
            }
        }
        this.CountFreeSlots();
        if (this.mrCurrentTemperature > 1000f)
        {
            this.mrCurrentTemperature = 1000f;
        }
        if (this.mrCurrentTemperature < -1000f)
        {
            this.mrCurrentTemperature = -1000f;
        }
        if (float.IsNaN(this.mrCurrentTemperature))
        {
            this.mrCurrentTemperature = 0f;
        }
        if (float.IsInfinity(this.mrCurrentTemperature))
        {
            this.mrCurrentTemperature = 0f;
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
        get { return mbAllowLogistics; }
    }

    public int CountCubes(ushort cube, ushort value)
    {
        return CountHowManyOfType(cube, value);
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
        ItemBase returnedItem;
        ushort returnedCubeType;
        ushort returnedCubeValue;
        int returnedAmount;

        if (!TryExtract(options.RequestType, options.ExemplarItemID, options.ExemplarBlockID, options.ExemplarBlockValue, options.InvertExemplar,
            options.MinimumAmount, options.MaximumAmount, options.KnownItemsOnly,
            false, false, options.ConvertToItem, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount))
        {
            // Failed to extract anything
            results.Item = null;
            results.Amount = 0;
            results.Cube = 0;
            results.Value = 0;
            return false;
        }

        results.Cube = returnedCubeType;
        results.Value = returnedCubeValue;
        results.Amount = returnedAmount;
        results.Item = returnedItem;

        return true;
    }

    public bool TryExtract(eHopperRequestType lType, int exemplarItemId, ushort exemplarCubeType, ushort exemplarCubeValue, bool invertExemplar, int minimumAmount, int maximumAmount, bool knownItemsOnly, bool countOnly, bool trashItems, bool convertCubesToItems, out ItemBase returnedItem, out ushort returnedCubeType, out ushort returnedCubeValue, out int returnedAmount)
    {
        // If request type is none, and no exemplar supplied, then return nothing.
        if (lType == eHopperRequestType.eNone && exemplarItemId == -1 && exemplarCubeType == 0)
        {
            returnedItem = null;
            returnedCubeType = 0;
            returnedCubeValue = 0;
            returnedAmount = 0;
            return false;
        }

        int numberFound = 0;
        int numberRemaining = maximumAmount;

        bool matchedItemRemoved = false;
        ItemBase matchedItem = null;

        int itemsToRemove = 0;


        // Now begin searching though the inventory looking for something which matches the filters provided.
        for (int l = 0; l < mnMaxStorage; l++)
        {
            // This offset ensures that each new requests at where the previous one finished, ensuring a more even return of items.
            mnRoundRobinOffset++;
            mnRoundRobinOffset %= mnMaxStorage;

            int i = mnRoundRobinOffset;

            // Grab the item from this inventory slot.
            ItemBase currentItem = maItemInventory[i];

            // No item in this slot.
            if (currentItem == null)
                continue;

            // If an exemplar item id has been supplied check against it now.
            if (exemplarItemId >= 0)
            {
                // Look for a specific item
                if (currentItem.mnItemID == exemplarItemId)
                {
                    // Found the item we want!
                    if (invertExemplar)
                    {
                        // Actually, just found the item we DON'T want.
                        continue;
                    }
                }
                else
                {
                    // Is this not the item we want, unless we have inverted the examplar.
                    if (!invertExemplar)
                        continue;
                }
            }
            else if (exemplarCubeType > 0)
            {
                // Look for a specific cube
                if (currentItem is ItemCubeStack)
                {
                    ItemCubeStack cubeStack = (ItemCubeStack)currentItem;

                    if (cubeStack.mCubeType == exemplarCubeType && (cubeStack.mCubeValue == exemplarCubeValue || exemplarCubeValue == ushort.MaxValue))
                    {
                        // Found the cube we want!
                        if (invertExemplar)
                        {
                            // Actually, just the found cube we DON'T want.
                            continue;
                        }
                    }
                    else
                    {
                        // This not the cube we want, unless we have inverted the examplar.
                        if (!invertExemplar)
                            continue;
                    }
                }
                else
                {
                    // This not the cube we want, unless we have inverted the examplar.
                    if (!invertExemplar)
                        continue;
                }
            }

            // Now check against the filters.
            if (lType != eHopperRequestType.eAny)
            {
                bool matchedRequest = false;

                // We are looking to limit to specific item types
                if (currentItem.mType != ItemType.ItemCubeStack)
                {
                    int lnID = currentItem.mnItemID;

                    if (lType == eHopperRequestType.eOrganic)
                    {
                        //Ruined/Pristine parts
                        if (lnID >= 4000 && lnID <= 4101) // TODO: Add an IsOrganic tag to the terrain
                        {
                            matchedRequest = true;
                        }
                    }

                    if (lType == eHopperRequestType.eBarsOnly)
                    {
                        bool lbIsBar = false;
                        if (lnID == ItemEntries.CopperBar) lbIsBar = true;
                        if (lnID == ItemEntries.TinBar) lbIsBar = true;
                        if (lnID == ItemEntries.IronBar) lbIsBar = true;
                        if (lnID == ItemEntries.LithiumBar) lbIsBar = true;
                        if (lnID == ItemEntries.GoldBar) lbIsBar = true;
                        if (lnID == ItemEntries.NickelBar) lbIsBar = true;
                        if (lnID == ItemEntries.TitaniumBar) lbIsBar = true;

                        if (lbIsBar == true)
                        {
                            matchedRequest = true;
                        }
                    }

                    if (lType == eHopperRequestType.eAnyCraftedItem)
                    {
                        //we could do an entire check here, but for NOW, just return 'any item', on the basis it was PROBABLY crafted
                        matchedRequest = true;
                    }

                    if (lType == eHopperRequestType.eResearchable)
                    {
                        ItemEntry entry = ItemEntry.mEntries[lnID];

                        if (entry != null && entry.DecomposeValue > 0)
                        {
                            matchedRequest = true;
                        }
                    }
                }
                else
                {
                    ItemCubeStack lStack = (ItemCubeStack)currentItem;

                    if (lType == eHopperRequestType.eResearchable)
                    {
                        // Decomposible cubes types have already been handled in a previous call to GetSpecificCubeRoundRobin (yes this is all really horrible)
                        TerrainDataEntry entry = TerrainData.mEntries[lStack.mCubeType];

                        if (entry != null && entry.DecomposeValue > 0)
                        {
                            matchedRequest = true;
                        }
                    }

                    // And the rest.
                    //if (lType == eHopperRequestType.eOrganic) continue;//Unsure if we have any organic blocks yet?

                    if (lType == eHopperRequestType.eHighCalorieOnly && CubeHelper.IsHighCalorie(lStack.mCubeType))
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eOreOnly && CubeHelper.IsSmeltableOre(lStack.mCubeType))
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eGarbage && CubeHelper.IsGarbage(lStack.mCubeType))
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eCrystals && lStack.mCubeType == eCubeTypes.OreCrystal)
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eGems && lStack.mCubeType == eCubeTypes.Crystal)
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eBioMass && lStack.mCubeType == eCubeTypes.OreBioMass)
                        matchedRequest = true;

                    if (lType == eHopperRequestType.eSmeltable && CubeHelper.IsIngottableOre(lStack.mCubeType))
                        matchedRequest = true;
                }

                if (false == matchedRequest)
                {
                    // We failed to match this filter, move onwards.
                    continue;
                }
            }

            if (knownItemsOnly)
            {
                // Check research
                if (!WorldScript.mLocalPlayer.mResearch.IsKnown(currentItem))
                {
                    // This item is not known to the player.
                    continue;
                }
            }

            // If we get to here then this is an item that we want. Yay.
            if (countOnly)
            {
                // If we are only looking for a count then add up the stack size.
                numberFound += ItemManager.GetCurrentStackSize(currentItem);
            }
            else
            {
                // Take out the items required.
                if (matchedItem == null)
                {
                    // We have found our first matching item! Set it here so that we can make sure anything else we get out is the same.
                    matchedItem = currentItem;
                }
                else
                {
                    // This is not the first matching item we have found.
                    if (!trashItems)
                    {
                        // If we are being asked to return the items then everything we take out must be the same type.
                        if (matchedItem.mType != currentItem.mType)
                        {
                            // Different type of item, we cannot get more of this type.
                            continue;
                        }

                        if (matchedItem.mType == ItemType.ItemCubeStack)
                        {
                            ItemCubeStack matchedCubeStack = (ItemCubeStack)matchedItem;
                            ItemCubeStack currentCubeStack = (ItemCubeStack)currentItem;

                            if (matchedCubeStack.mCubeType != currentCubeStack.mCubeType)
                                continue; // Different cube type.

                            if (exemplarCubeValue != ushort.MaxValue) // If this is supplied then we don't give a crap what the value is.
                            {
                                if (matchedCubeStack.mCubeValue != currentCubeStack.mCubeValue)
                                    continue; // Different cube value.
                            }
                        }
                        else
                        {
                            if (matchedItem.mnItemID != currentItem.mnItemID)
                            {
                                // Not the same item
                                continue;
                            }
                        }
                    }
                    else
                    {
                        // We are not being asked to return the items, there is no need for everything to be the same type.
                    }
                }

                // Remove this item.
                if (currentItem.mType == ItemType.ItemCubeStack)
                {
                    int stackSize = ((ItemCubeStack)currentItem).mnAmount;

                    if (stackSize <= numberRemaining)
                    {
                        // Remove this entire stack.
                        numberRemaining -= stackSize;
                        numberFound += stackSize;

                        if (minimumAmount - numberFound - stackSize > 0)
                        {
                            // This is a problem. This stack does not meet the minimum requirement for anything
                            // to be extracted. We can't remove it from storage right now because we cannot be
                            // sure we'll find enough other items.
                            mRemoveCache[itemsToRemove] = i;
                            itemsToRemove++;
                        }
                        else
                        {
                            maItemInventory[i] = null;
                        }

                        matchedItemRemoved = true;
                    }
                    else
                    {
                        // Remove part of this stack.
                        ((ItemCubeStack)currentItem).mnAmount -= numberRemaining;
                        numberFound += numberRemaining;
                        numberRemaining = 0;
                    }
                }
                else if (currentItem.mType == ItemType.ItemStack)
                {
                    int stackSize = ((ItemStack)currentItem).mnAmount;

                    if (stackSize <= numberRemaining)
                    {
                        // Remove this entire stack.
                        numberRemaining -= stackSize;
                        numberFound += stackSize;

                        if (minimumAmount - numberFound - stackSize > 0)
                        {
                            // This is a problem. This stack does not meet the minimum requirement for anything
                            // to be extracted. We can't remove it from storage right now because we cannot be
                            // sure we'll find enough other items.
                            mRemoveCache[itemsToRemove] = i;
                            itemsToRemove++;
                        }
                        else
                        {
                            maItemInventory[i] = null;
                        }

                        matchedItemRemoved = true;
                    }
                    else
                    {
                        // Remove part of this stack.
                        ((ItemStack)currentItem).mnAmount -= numberRemaining;
                        numberFound += numberRemaining;
                        numberRemaining = 0;
                        break;
                    }
                }
                else
                {
                    if (!trashItems)
                    {
                        // If we are returning items then we cannot return multiple non-stackable items in a single request.
                        returnedItem = matchedItem;
                        numberFound = 1;
                        numberRemaining = 0;
                        maItemInventory[i] = null;
                        break;
                    }
                    else
                    {
                        // We are trashing items from the hopper.
                        numberFound++;
                        numberRemaining--;

                        if (minimumAmount - numberFound - 1 > 0)
                        {
                            // This is a problem. This stack does not meet the minimum requirement for anything
                            // to be extracted. We can't remove it from storage right now because we cannot be
                            // sure we'll find enough other items.
                            mRemoveCache[itemsToRemove] = i;
                            itemsToRemove++;
                        }
                        else
                        {
                            maItemInventory[i] = null;
                        }

                        matchedItemRemoved = true;
                    }
                }

                if (numberRemaining <= 0)
                {
                    // We are done.
                    break;
                }
            }
        }

        // If we had minimum amounts that stopped us from removing items immediately then do that now.
        for (int j = 0; j < itemsToRemove; j++)
        {
            maItemInventory[mRemoveCache[j]] = null;
        }

        // If we successfully found an item return the details.
        if (matchedItem != null && !countOnly)
        {
            // We found something.
            if (!trashItems)
            {
                // We have been asked to return the items to the caller.
                returnedItem = matchedItem;

                if (matchedItemRemoved)
                {
                    if (matchedItem.mType == ItemType.ItemCubeStack)
                    {
                        ItemCubeStack cubeStack = (ItemCubeStack)matchedItem;

                        // Ensure the cube stack has the correct total number found.
                        cubeStack.mnAmount = numberFound;
                        returnedItem = cubeStack;
                        returnedCubeType = cubeStack.mCubeType;
                        returnedCubeValue = cubeStack.mCubeValue;
                        returnedAmount = numberFound;
                    }
                    else if (matchedItem.mType == ItemType.ItemStack)
                    {
                        ItemStack itemStack = (ItemStack)matchedItem;

                        // Ensure the item stack has the correct total number found.
                        itemStack.mnAmount = numberFound;
                        returnedItem = itemStack;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = numberFound;
                    }
                    else
                    {
                        returnedItem = matchedItem;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = 1;
                    }
                }
                else
                {
                    if (matchedItem.mType == ItemType.ItemCubeStack)
                    {
                        ItemCubeStack cubeStack = (ItemCubeStack)matchedItem;

                        // We may need to spawn a cube stack here
                        if (convertCubesToItems)
                        {
                            ItemStack itemStack = (ItemStack)ItemManager.CloneItem(cubeStack, numberFound);
                        }
                        else
                        {
                            returnedItem = null;
                        }

                        returnedCubeType = cubeStack.mCubeType;
                        returnedCubeValue = cubeStack.mCubeValue;
                        returnedAmount = numberFound;
                    }
                    else if (matchedItem.mType == ItemType.ItemStack)
                    {
                        ItemStack itemStack = (ItemStack)ItemManager.CloneItem(matchedItem, numberFound);

                        // Ensure the item stack has the correct total number found.
                        returnedItem = itemStack;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = numberFound;
                    }
                    else
                    {
                        // This should not happen!
                        Debug.LogError("Non-stack matched item was not removed from the hopper inventory during extract?");
                        returnedItem = matchedItem;
                        returnedCubeType = 0;
                        returnedCubeValue = 0;
                        returnedAmount = 1;
                    }
                }
            }
            else
            {
                // We don't need to return the items, just the amount removed from the storage.
                returnedItem = null;
                returnedCubeType = 0;
                returnedCubeValue = 0;
                returnedAmount = numberFound;
            }

            CountFreeSlots();
            MarkDirtyDelayed();

            //LogisticsOperation();
            RequestImmediateNetworkUpdate();

            return true;
        }
        else
        {
            // Did not find anything.
            returnedItem = null;
            returnedCubeType = 0;
            returnedCubeValue = 0;
            returnedAmount = 0;
            return false;
        }

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
        if (mnStorageFree > 0 && (CheckExemplar(item) || CheckExemplar(cubeType,cubeValue)))
        {
            if (item != null)
            {
                AddItem(item);
            }
            else
            {
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

        bool success = TryExtract(eHopperRequestType.eAny, -1, cube, value, false, amount, amount, false, false, false, false, out returnedItem, out returnedCubeType, out returnedCubeValue, out returnedAmount);

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
        // Check we have not run out of space.
        if (mnStorageFree <= 0 && (CheckExemplar(options.Item)))
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
        //			Debug.LogError("Derp, how did a Quarry pick a full hopper to empty into?");
        //		}
        //		else
        //		{
        //			maAttachedHoppers[0].AddCube(mCarryCube,mCarryValue);
        //		}
    }

    public bool TryInsert(StorageUserInterface sourceEntity, ItemBase item)
    {
        if (!CheckExemplar(item))
        {
            return false;
        }

        return AddItem(item);
    }

    public bool TryInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        if (!CheckExemplar(cube, value))
        {
            return false;
        }

        if (amount == 1)
        {
            AddCube(cube, value);
            return true;
        }

        if (mnStorageFree < amount)
            return false;

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
        if (!CheckExemplar(item))
        {
            return 0;
        }
        int currentStackSize = ItemManager.GetCurrentStackSize(item);
        int num = currentStackSize;
        if (this.mnStorageFree == 0)
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
        if (!CheckExemplar(cube,value))
        {
            return 0;
        }
        int remaining = amount;
        while (remaining > 0 && mnStorageFree > 0)
        {
            AddCube(cube, value);
            remaining--;
        }

        return amount - remaining;
    }

    //******************** RETURN INVENTORY INFORMATION ********************

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
    public enum ePermissions
    {
        AddAndRemove,
        RemoveOnly,
        AddOnly,
        Locked,
        eNumPermissions
    }

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
}


