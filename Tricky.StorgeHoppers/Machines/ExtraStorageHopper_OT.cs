using FortressCraft.Community;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class ExtraStorageHoppers_OT : global::MachineEntity, CommunityItemInterface
{
    //MY STUFF
    private string ModName = Variables.ModName;
    private string ModVersion = Variables.ModVersion;
    private string PopUpText;
    //STORAGE HOPPER STUFF
    private Segment[] CheckSegments;
    public static ushort CRYO_HOPPER = 3;
    private Segment[,,] HooverSegment;
    public ItemBase[] maItemInventory;
    public ushort[] maStorage;
    public bool mbAllowLogistics;
    private bool mbForceHoloUpdate;
    private bool mbForceTextUpdate;
    private bool mbHooverEmissionOff;
    public bool mbHooverOn;
    private bool mbLinkedToGO;
    private bool mbShowHopper;
    private bool mbTutorialComplete;
    private MaterialPropertyBlock mHoloMPB;
    private GameObject mHoloStatus;
    private ParticleSystem mHooverPart;
    private GameObject mHopperPart;
    public string mLastItemAdded;
    private int mnHooverEmissionRate;
    private int mnHopperLoops;
    private int mnLowFrequencyUpdates;
    private int mnMaxStorage;
    private int mnReadouts;
    private int mnRoundRobinOffset;
    public int mnStorageFree;
    public int mnStorageUsed;
    static int[] mRemoveCache = new int[100];
    private int mnUpdates;
    public static ushort MOTORISED_HOPPER = 4;
    public eHopperPermissions mPermissions = eHopperPermissions.AddAndRemove;
    private eHopperPermissions mPreviousPermissions;
    public float mrCurrentPower;
    public float mrCurrentTemperature;
    public float mrExtractionTime;
    private float mrLogisticsDebounce;
    private float mrMaxLightDistance;
    private float mrMaxPower;
    public float mrNormalisedPower;
    public float mrPowerUsage;
    public float mrPPS;
    private float mrPrevDistanceToPlayer;
    public float mrReadoutTick;
    private float mrSleep;
    private float mrSpoilTimer;
    private float mrTimeElapsed;
    private float mrTimeSinceLogistics;
    private float mrTimeUntilFlash;
    private float mrTimeUntilPlayerDistanceUpdate;
    private TextMesh mTextMesh;
    public static int PowerPerItem = 5;
    public static float SAFE_COLD_TEMP = -250f;
    public const ushort SPOILED_ORGANICS = 0x1004;
    private GameObject TutorialEffect;
    private Light WorkLight;

    //UI STUFF
    public static bool AllowBuilding = true;
    public static bool AllowInteracting = true;
    public static bool AllowLooking = true;
    public static bool AllowMovement = true;
    public GameObject BlockSelectPanel;
    public static bool CrossHairShown;
    public static bool CursorShown = false;
    public const float DEFAULT_MACHINE_POPUP_TIME = 0.75f;
    public static float DisallowBuildingTimer;
    public ParticleSystem EnergyAddParticles;
    public ParticleSystem EnergyDrainParticles;
    public static float ForceNGUIUpdate;
    public static bool GamePaused;
    public const float HELD_BUTTON_SYNC_PERIOD = 1f;
    public static bool HotBarShown = true;
    public static bool HudShown = true;
    public static UIManager instance;
    public static bool mbEditingTextField;
    public bool mbMenuBlurEnabled;
    public static bool mbResearchUpdated;
    public ChatPanelScript mChatPanel;
    public ConfirmationPanelScript mConfirmationPanel;
    public GameObject mConversionPanel;
    public CraftingPanelLabel mCraftingPanel;
    public DeathPanelScript mDeathPanel;
    public GameObject mDLC_Panel;
    public DragAndDropManager mDragAndDropManager;
    public GameObject mDragBackgroundNet;
    public UISprite mDragIcon;
    public static UIInput mEditTextBox;
    public ExitPanelScript mExitPanel;
    public GenericMachinePanelScript mGenericMachinePanel;
    public HandbookPanelScript mHelpPanel;
    public IntroPanelScript mIntroPanel;
    public InventoryPanelScript mInventoryPanel;
    public MultiplayerClientPanelScript mMultiplayerClientPanel;
    public MultiplayerHostPanelScript mMultiplayerHostPanel;
    public MultiplayerNonHostPanelScript mMultiplayerNonHostPanel;
    public ResearchPanelScript mResearchPanel;
    private float mrEstimatedConversionTime;
    private float mRetakeDebounce;
    public static float mrHandbookDisplayDelay;
    private float mrSIPPos;
    [HideInInspector]
    public float mrSIPTime;
    private static float mrWorkshopFader;
    private float mrWorldConversionPreviousProgress;
    public ItemSplitPanelScript mSplitPanel;
    public UICamera mUICamera;
    private static readonly object mUiRulesLock = new object();
    public const string NO_SPRITE = "NoIcon";
    public static bool OverrideAllowInteractingForInventory = false;
    public static bool PanelsAllowedToUpdate = true;
    public GameObject SkinsPanel1;
    public GameObject SkinsPanel3;
    public static bool SteadyCamActive = false;
    public GameObject Survival_Info_Panel;
    public UILabel Survival_Info_Panel_Label;
    public static int UI_UpdateRate = 1;
    public const string UNKNOWN_SPRITE = "Unknown";
    public static bool UsingUIUpdateRate = false;
    public GameObject Waypoint_Label_Panel;
    public GameObject Waypoint_Name_Label;
    public static Vector3 Waypoint_Scale;
    public GameObject WorkshopBehaviourPanel;
    public GameObject WorkshopDetailPanel;

    // More of my stuff
    private ushort CubeValue;
    private Color mCubeColor;
    public int ItemsDeleted;
    private string HopperName;
    private float Delay = 0f;

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
        this.mrMaxPower = 1000f;
        this.mnMaxStorage = 2001;
        this.mrExtractionTime = 15f;
        this.mrPowerUsage = 0.1f;
        this.mrSpoilTimer = 30f;
        this.mnHopperLoops = 10;
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
        this.mrMaxPower = 0f;
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
        this.MarkDirtyDelayed();
    }

    public bool CheckExemplar(ItemBase lExemplar)
    {
        if (ItemManager.GetItemName(lExemplar) == this.ExemplarString)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public ushort GetCubeValue()
    {
        return this.CubeValue;
    }

    public override global::HoloMachineEntity CreateHolobaseEntity(global::Holobase holobase)
    {
        global::HolobaseEntityCreationParameters holobaseEntityCreationParameters = new global::HolobaseEntityCreationParameters(this);
        global::HolobaseVisualisationParameters holobaseVisualisationParameters = holobaseEntityCreationParameters.AddVisualisation(holobase.mPreviewCube);
        holobaseVisualisationParameters.Color = mCubeColor;
        return holobase.CreateHolobaseEntity(holobaseEntityCreationParameters);
    }

    public override void SpawnGameObject()
    {
        base.mObjectType = SpawnableObjectEnum.LogisticsHopper;
        base.SpawnGameObject();
    }

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
                    if (!CheckExemplar(currentHotBarItemOrCubeAsItem))
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
                            SetExemplar(currentHotBarItemOrCubeAsItem);
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
    //HIGH FREQUENCY
    private int GetBestChoice(ushort lSearchType, out ushort lType)
    {
        int num = -1;
        ushort lBestChoice = 0;
        ushort num3 = 0;
        List<CraftData> recipesForSet = CraftData.GetRecipesForSet("Smelter");
        int num9 = this.ContainsOre(lSearchType, true, recipesForSet, out lBestChoice);
        if (lSearchType == 0)
        {
            if ((lBestChoice != 0) && (num9 > num))
            {
                num = num9;
                num3 = lBestChoice;
            }
        }
        else
        {
            Debug.LogWarning("Warning, GetOre called with specific search type, but no code to handle this!");
        }
        if (num > 0)
        {
            if (num3 == 0)
            {
                Debug.LogError("Error, located best type of ore type NULL?");
            }
            lType = num3;
            return num;
        }
        lType = 0;
        return 0;
    }

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

    private void HFCheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if ((GetPermissions() != eHopperPermissions.Locked) && (GetPermissions() != eHopperPermissions.AddOnly))
        {
            //PowerHub
            if ((lCube == 0x1f6) && WorldScript.mbIsServer)
            {
                CentralPowerHub hub = checkSegment.FetchEntity(eSegmentEntity.CentralPowerHub, checkX, checkY, checkZ) as CentralPowerHub;
                if ((hub != null) && hub.WantsToConsumeResources())
                {
                    for (int j = 0; j < this.mnMaxStorage; j++)
                    {
                        if (((this.maItemInventory[j] != null) && (this.maItemInventory[j].mType == ItemType.ItemCubeStack)) && ((this.maItemInventory[j] as ItemCubeStack).mnAmount > 0))
                        {
                            ushort mCubeType = (this.maItemInventory[j] as ItemCubeStack).mCubeType;
                            if (!CubeHelper.IsSmeltableOre(mCubeType) && CubeHelper.IsHighCalorie(mCubeType))
                            {
                                hub.AddResourceToConsume(mCubeType);
                                ItemCubeStack stack1 = this.maItemInventory[j] as ItemCubeStack;
                                stack1.mnAmount--;
                                if ((this.maItemInventory[j] as ItemCubeStack).mnAmount <= 0)
                                {
                                    this.maItemInventory[j] = null;
                                }
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return;
                            }
                        }
                    }
                }
            }
            //Generator
            if ((lCube == 0x1fd) && WorldScript.mbIsServer)
            {
                PyrothermicGenerator generator = checkSegment.FetchEntity(eSegmentEntity.PyrothermicGenerator, checkX, checkY, checkZ) as PyrothermicGenerator;
                if ((generator != null) && generator.mbReadyForResource)
                {
                    for (int m = 0; m < this.mnMaxStorage; m++)
                    {
                        if (((this.maItemInventory[m] != null) && (this.maItemInventory[m].mType == ItemType.ItemCubeStack)) && ((this.maItemInventory[m] as ItemCubeStack).mnAmount > 0))
                        {
                            ushort num6 = (this.maItemInventory[m] as ItemCubeStack).mCubeType;
                            if (!CubeHelper.IsSmeltableOre(num6) && CubeHelper.IsHighCalorie(num6))
                            {
                                generator.AddResourceToConsume(num6);
                                ItemCubeStack stack2 = this.maItemInventory[m] as ItemCubeStack;
                                stack2.mnAmount--;
                                if ((this.maItemInventory[m] as ItemCubeStack).mnAmount <= 0)
                                {
                                    this.maItemInventory[m] = null;
                                }
                                this.CountFreeSlots();
                                this.MarkDirtyDelayed();
                                return;
                            }
                        }
                    }
                }
            }

            //Smelter
            if ((lCube == 512) && WorldScript.mbIsServer)
            {
                OreSmelter Smelter = checkSegment.FetchEntity(eSegmentEntity.OreSmelter, checkX, checkY, checkZ) as OreSmelter;
                ushort CubeType;
                GetBestChoice(0, out CubeType);
                if (((Smelter.meState == OreSmelter.eState.eWaitingOnMatTrigger) || (Smelter.meState == OreSmelter.eState.eObtainingMats)) && (this.CountHowManyOfOreType(CubeType) >= Smelter.mnOrePerBar) && (CubeType != 0) && (Smelter.mnOreCount == 0))
                {

                    Smelter.AddCubeTypeOre(CubeType, Smelter.mnOrePerBar);
                    this.RemoveInventoryCube(CubeType, 4095, Smelter.mnOrePerBar);
                    this.CountFreeSlots();
                    this.MarkDirtyDelayed();
                    return;
                }
            }
            //Matter Mover
            if ((lCube == 512) && WorldScript.mbIsServer)
            {
                MatterMover MMover = checkSegment.FetchEntity(eSegmentEntity.MatterMover, checkX, checkY, checkZ) as MatterMover;
            }
        }
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
    //HIGH FREQUENCY
    private void HFCheckSuppliers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if ((GetPermissions() != eHopperPermissions.Locked) && (GetPermissions() != eHopperPermissions.RemoveOnly))
        {
            //Ore Extractor HF
            if ((lCube == 0x1f7) && (this.mnStorageFree > 0))
            {
                OreExtractor extractor = checkSegment.FetchEntity(eSegmentEntity.OreExtractor, checkX, checkY, checkZ) as OreExtractor;
                if ((extractor != null) && (extractor.mnStoredOre > 0) && (extractor.mnOreType == this.ExemplarBlockID))
                {
                    int mnStorageFree = extractor.mnStoredOre / 4;
                    if (mnStorageFree < 1)
                    {
                        mnStorageFree = 1;
                    }
                    if (mnStorageFree > this.mnStorageFree)
                    {
                        mnStorageFree = this.mnStorageFree;
                    }
                    for (int i = 0; i < mnStorageFree; i++)
                    {
                        this.AddCube(extractor.mnOreType, TerrainData.GetDefaultValue(extractor.mnOreType));
                        extractor.mnStoredOre--;
                        if (this.mnStorageFree == 0)
                        {
                            break;
                        }
                    }
                    this.mrReadoutTick = 0f;
                    this.mnReadouts--;
                }
            }
            //Ore smelter (basic included)
            if ((lCube == 512) && (this.mnStorageFree > 0))
            {
                OreSmelter Smelter = checkSegment.FetchEntity(eSegmentEntity.OreSmelter, checkX, checkY, checkZ) as OreSmelter;
                ItemBase FinnishedBar = Smelter.mOutputHopper;
                if ((Smelter != null) && (Smelter.mOutputHopper != null) && ((Smelter.mOutputHopper as ItemStack).mnAmount > 0) && (Smelter.mOutputHopper.mnItemID == this.ExemplarItemID))
                {
                    this.AddItem(FinnishedBar);
                    Smelter.mOutputHopper = null;
                    Smelter.MarkDirtyDelayed();

                }
            }
        }
        //Conveyors
        if ((lCube == 513) && (this.mnStorageFree > 0) && (GetPermissions() != eHopperPermissions.Locked))
        {
            ConveyorEntity Conveyor = checkSegment.FetchEntity(eSegmentEntity.Conveyor, checkX, checkY, checkZ) as ConveyorEntity;
            Vector3 lVTT = new Vector3(mnX - Conveyor.mnX, mnY - Conveyor.mnY, mnZ - Conveyor.mnZ);
            float lrDot = Vector3.Dot(lVTT, Conveyor.mForwards);
            if ((Conveyor != null) && (Conveyor.IsCarryingCargo()) && (lrDot == 1) && (this.mnStorageFree > 0))
            {
                ushort CubeType;
                ushort CubeValue;
                TerrainData.GetCubeForName(this.ExemplarString, out CubeType, out CubeValue);
                if ((Conveyor.mCarriedCube != 0) && (Conveyor.mrCarryTimer != 0f || Conveyor.mbConveyorBlocked) && (Conveyor.mCarriedCube == CubeType) && (Conveyor.mCarriedValue == CubeValue))
                {
                    this.AddCube(Conveyor.mCarriedCube, Conveyor.mCarriedValue);
                    Conveyor.RemoveCube();
                    Conveyor.FinaliseOffloadingCargo();
                }
                else if ((Conveyor.mCarriedItem != null) && (Conveyor.mrCarryTimer != 0f || Conveyor.mbConveyorBlocked) && (ItemManager.GetItemName(Conveyor.mCarriedItem) == this.ExemplarString))
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

    private void CheckConsumers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if ((GetPermissions() != eHopperPermissions.Locked) && (GetPermissions() != eHopperPermissions.AddOnly))
        {

        }
    }

    private void CheckConveyor(long checkX, long checkY, long checkZ, Segment checkSegment, ushort lCube)
    {

    }

    private void CheckSuppliers(Segment checkSegment, ushort lCube, long checkX, long checkY, long checkZ)
    {
        if ((GetPermissions() != eHopperPermissions.Locked) && (GetPermissions() != eHopperPermissions.RemoveOnly))
        {

            if ((lCube == 0x20a) && (this.mnStorageFree > 0))
            {
                RefineryController controller = checkSegment.FetchEntity(eSegmentEntity.RefineryController, checkX, checkY, checkZ) as RefineryController;
                if ((controller != null) && (controller.mOutputHopper != null))
                {
                    ItemBase partialInventory;
                    if (ItemManager.GetCurrentStackSize(controller.mOutputHopper) > this.mnStorageFree)
                    {
                        partialInventory = controller.GetPartialInventory(this.mnStorageFree);
                    }
                    else
                    {
                        partialInventory = controller.GetWholeInventory();
                    }
                    this.AddItem(partialInventory);
                }
            }
            if ((lCube == 520) && (this.mnStorageFree > 0))
            {
                ManufacturingPlant plant = checkSegment.FetchEntity(eSegmentEntity.ManufacturingPlant, checkX, checkY, checkZ) as ManufacturingPlant;
                if (plant != null)
                {
                    this.GetManufacturingPlantOutput(plant);
                }
            }
            if ((lCube == 0x209) && (this.mnStorageFree > 0))
            {
                ManufacturingPlantModule module = checkSegment.FetchEntity(eSegmentEntity.ManufacturingPlantModule, checkX, checkY, checkZ) as ManufacturingPlantModule;
                if ((module != null) && (module.mPlant != null))
                {
                    this.GetManufacturingPlantOutput(module.mPlant);
                }
            }
        }
    }

    private void ConfigTutorial()
    {
        if (!this.mbTutorialComplete && (WorldScript.meGameMode == eGameMode.eSurvival))
        {
            if (SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.RemoveCoalFromHopper)
            {
                if (this.TutorialEffect == null)
                {
                    this.TutorialEffect = (GameObject)UnityEngine.Object.Instantiate(SurvivalSpawns.instance.EmptySH, (base.mWrapper.mGameObjectList[0].gameObject.transform.position + Vector3.up) + Vector3.up, Quaternion.identity);
                    this.TutorialEffect.SetActive(true);
                }
            }
            else if (this.TutorialEffect != null)
            {
                UnityEngine.Object.Destroy(this.TutorialEffect);
                this.TutorialEffect = null;
                this.mbTutorialComplete = true;
            }
        }
    }

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

    public bool DeliverPower(float amount)
    {
        if (amount > this.GetRemainingPowerCapacity())
        {
            amount = this.GetRemainingPowerCapacity();
        }
        this.mrCurrentPower += amount;
        this.MarkDirtyDelayed();
        return true;
    }

    public override void DropGameObject()
    {
        base.DropGameObject();
        this.mbLinkedToGO = false;
        UnityEngine.Object.Destroy(this.TutorialEffect);
    }

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

    public float GetMaximumDeliveryRate()
    {
        return 500f;
    }

    public float GetMaxPower()
    {
        return this.mrMaxPower;
    }

    public float GetRemainingPowerCapacity()
    {
        return (this.mrMaxPower - this.mrCurrentPower);
    }

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

    public override int GetVersion()
    {

        return 1;
    }

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

    private void LinkToGO()
    {
        if ((base.mWrapper != null) && base.mWrapper.mbHasGameObject)
        {
            if (base.mWrapper.mGameObjectList == null)
            {
                Debug.LogError("Ore Extractor missing game object #0?");
            }
            if (base.mWrapper.mGameObjectList[0].gameObject == null)
            {
                Debug.LogError("Ore Extractor missing game object #0 (GO)?");
            }
            this.WorkLight = base.mWrapper.mGameObjectList[0].transform.Search("HooverGraphic").GetComponent<Light>();
            if (this.WorkLight == null)
            {
                Debug.LogError("Storage Hopper has missing light?");
            }
            this.mHooverPart = base.mWrapper.mGameObjectList[0].transform.Search("HooverGraphic").GetComponent<ParticleSystem>();
            this.mHooverPart.emissionRate = 0f;
            this.mTextMesh = base.mWrapper.mGameObjectList[0].gameObject.transform.Search("Storage Text").GetComponent<TextMesh>();
            this.mHopperPart = base.mWrapper.mGameObjectList[0].transform.Find("Hopper").gameObject;
            this.mHoloStatus = this.mHopperPart.transform.Find("Holo_Status").gameObject;
            this.mHoloStatus.SetActive(false);
            this.mPreviousPermissions = eHopperPermissions.eNumPermissions;
            this.SetHoloStatus();
            this.mbForceTextUpdate = true;
            this.mbLinkedToGO = true;
            // COLOR STORAGE HOPPER
            MeshRenderer lRenderer = this.mHopperPart.GetComponent<MeshRenderer>();
            MeshRenderer Render2 = this.mHopperPart.GetComponent<MeshRenderer>();
            Render2.material.SetColor("_Color", this.mCubeColor);

        }
    }

    public void LogisticsOperation()
    {
        this.mrTimeSinceLogistics = this.mrLogisticsDebounce;
        if (this.mrTimeSinceLogistics > 0f)
        {
            this.mbAllowLogistics = false;
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
    //LowFrequencyUpdate
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
        if (this.mrTimeSinceLogistics > 0f)
        {
            this.mrTimeSinceLogistics -= LowFrequencyThread.mrPreviousUpdateTimeStep;
            this.mbAllowLogistics = false;
        }
        else
        {
            this.mbAllowLogistics = true;
        }

        this.mrTimeUntilPlayerDistanceUpdate -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        if (this.mrTimeUntilPlayerDistanceUpdate < 0f)
        {
            this.mrPrevDistanceToPlayer = base.mDistanceToPlayer;
            this.UpdatePlayerDistanceInfo();
            this.mrTimeUntilPlayerDistanceUpdate = base.mDistanceToPlayer / 30f;
            if (this.mrTimeUntilPlayerDistanceUpdate > 2f)
            {
                this.mrTimeUntilPlayerDistanceUpdate = 2f;
            }
        }
        this.mnLowFrequencyUpdates++;
        //this.UpdateHoover();
        this.UpdatePoweredHopper();
        if (WorldScript.mbIsServer)
        {
            this.UpdateSpoilage();
        }
        this.mrNormalisedPower = this.mrCurrentPower / this.mrMaxPower;
        this.mrReadoutTick -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        if (this.mrReadoutTick < 0f)
        {
            this.mrReadoutTick = 1f;
            this.mnReadouts++;
            long mnX = base.mnX;
            long mnY = base.mnY;
            long mnZ = base.mnZ;
            int index = this.mnReadouts % 6;
            switch (index)
            {
                case 0:
                    mnX -= 1L;
                    break;

                case 1:
                    mnX += 1L;
                    break;

                case 2:
                    mnY -= 1L;
                    break;

                case 3:
                    mnY += 1L;
                    break;

                case 4:
                    mnZ -= 1L;
                    break;

                case 5:
                    mnZ += 1L;
                    break;
            }
            if (this.CheckSegments[index] == null)
            {
                this.CheckSegments[index] = base.AttemptGetSegment(mnX, mnY, mnZ);
            }
            else if (this.CheckSegments[index].mbDestroyed || !this.CheckSegments[index].mbInitialGenerationComplete)
            {
                this.CheckSegments[index] = null;
            }
            else
            {
                ushort lType = this.CheckSegments[index].GetCube(mnX, mnY, mnZ);
                if (CubeHelper.HasEntity(lType))
                {
                    if (this.mnStorageFree > 0)
                    {
                        this.CheckSuppliers(this.CheckSegments[index], lType, mnX, mnY, mnZ);
                    }
                    if (this.mnStorageUsed > 0)
                    {
                        this.CheckConsumers(this.CheckSegments[index], lType, mnX, mnY, mnZ);
                    }
                }
            }
        }
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

    private void SetHoloStatus()
    {
        if (this.CubeValue != 2)
        {
            if (this.mHoloStatus != null)
            {
                if (this.mbForceHoloUpdate)
                {
                    this.mPreviousPermissions = eHopperPermissions.eNumPermissions;
                }
                this.mbForceHoloUpdate = false;
                if ((base.mDistanceToPlayer > 8f) || base.mbWellBehindPlayer)
                {
                    if (this.mHoloStatus.activeSelf)
                    {
                        this.mHoloStatus.SetActive(false);
                    }
                }
                else
                {
                    if (!this.mHoloStatus.activeSelf)
                    {
                        this.mPreviousPermissions = eHopperPermissions.eNumPermissions;
                        this.mHoloStatus.SetActive(true);
                    }
                    if (this.mPreviousPermissions != this.mPermissions)
                    {
                        this.mPreviousPermissions = this.mPermissions;
                        if (this.mPermissions == eHopperPermissions.AddAndRemove)
                        {
                            this.mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f, 0.5f);
                        }
                        if (this.mPermissions == eHopperPermissions.RemoveOnly)
                        {
                            this.mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0f, 0f);
                        }
                        if (this.mPermissions == eHopperPermissions.Locked)
                        {
                            this.mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f, 0f);
                        }
                        if (this.mPermissions == eHopperPermissions.AddOnly)
                        {
                            this.mHoloStatus.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f, 0.5f);
                        }
                    }
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

    public void ToggleHoover()
    {
        this.mbHooverOn = false;
        this.MarkDirtyDelayed();
        this.mbForceTextUpdate = true;
        this.RequestImmediateNetworkUpdate();
        
    }

    public void TogglePermissions()
    {
        this.mPermissions += 1;
        if (this.mPermissions == eHopperPermissions.eNumPermissions)
        {
            this.mPermissions = eHopperPermissions.AddAndRemove;
        }
        if (this.mPermissions > eHopperPermissions.eNumPermissions)
        {
            this.mPermissions = eHopperPermissions.RemoveOnly;
        }

        this.MarkDirtyDelayed();
        this.mbForceTextUpdate = true;
        this.mbForceHoloUpdate = true;
        this.RequestImmediateNetworkUpdate();

        FloatingCombatTextManager.instance.QueueText(base.mnX, base.mnY + 1L, base.mnZ, 1f, this.mPermissions.ToString(), Color.green, 1.5f).mrStartRadiusRand = 0.25f;
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
            this.ConfigTutorial();
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

    private void UpdateLOD()
    {
        bool flag = true;
        if (base.mDistanceToPlayer > 64f)
        {
            flag = false;
        }
        if (base.mbWellBehindPlayer)
        {
            flag = false;
        }
        if (Mathf.Abs(this.mVectorToPlayer.y) > 32f)
        {
            flag = false;
        }
        if (base.mSegment.mbOutOfView)
        {
            flag = false;
        }
        if (flag != this.mbShowHopper)
        {
            this.mbShowHopper = flag;
            this.mbForceHoloUpdate = true;
            if (flag)
            {
                if (!this.mHopperPart.activeSelf)
                {
                    this.mHopperPart.SetActive(true);
                    this.mHopperPart.GetComponent<Renderer>().enabled = true;
                }
            }
            else
            {
                if (this.mHopperPart.activeSelf)
                {
                    this.mHopperPart.SetActive(false);
                }
                this.mHopperPart.GetComponent<Renderer>().enabled = false;
            }
        }
        if (((base.mDistanceToPlayer > 24f) || base.mbWellBehindPlayer) || (base.mDistanceToPlayer > (CamDetail.SegmentDrawDistance - 8f)))
        {
            if (this.mTextMesh.GetComponent<Renderer>().enabled)
            {
                this.mbForceHoloUpdate = true;
                this.mTextMesh.GetComponent<Renderer>().enabled = false;
            }
        }
        else if (!this.mTextMesh.GetComponent<Renderer>().enabled)
        {
            this.mbForceHoloUpdate = true;
            this.mTextMesh.GetComponent<Renderer>().enabled = true;
        }
    }

    private void UpdateMeshText()
    {
        if (this.mTextMesh.GetComponent<Renderer>().enabled && (base.mDistanceToPlayer < 12f))
        {
            string str = this.mPermissions.ToString() + "\n";
            if (this.mnStorageFree == 0)
            {
                str = str + "Storage full\n";
            }
            else if (this.mnStorageUsed == 0)
            {
                str = str + "Storage Empty\n";
            }
            else
            {
                str = str + this.mnStorageFree.ToString() + " free slots\n";
            }
            if (this.mrTimeSinceLogistics > 0f)
            {
                str = str + "Processing...";
            }
            else
            {
                str = str + "[" + this.mLastItemAdded + "]";
            }
            this.mTextMesh.text = str;
            this.mbForceTextUpdate = false;
        }
    }

    private void UpdatePoweredHopper()
    {

    }

    private void UpdateSpoilage()
    {
        this.mrSpoilTimer = 30f;
    }

    private void UpdateWorkLight()
    {
        bool flag = false;
        if (this.mnStorageUsed == 0)
        {
            flag = true;
        }
        if (this.mnStorageFree == 0)
        {
            flag = true;
        }
        if (base.mValue == CRYO_HOPPER)
        {
            flag = true;
        }
        if (base.mbWellBehindPlayer)
        {
            flag = false;
        }
        this.mrMaxLightDistance += ((CamDetail.FPS - this.mrMaxLightDistance) * Time.deltaTime) * 0.1f;
        if (this.mrMaxLightDistance < 2f)
        {
            this.mrMaxLightDistance = 2f;
        }
        if (this.mrMaxLightDistance > 64f)
        {
            this.mrMaxLightDistance = 64f;
        }
        if (base.mDistanceToPlayer > this.mrMaxLightDistance)
        {
            flag = false;
        }
        if (flag)
        {
            if (!this.WorkLight.enabled)
            {
                this.WorkLight.enabled = true;
                this.WorkLight.range = 0.05f;
            }
            if (base.mValue == CRYO_HOPPER)
            {
                if (this.mrCurrentTemperature > SAFE_COLD_TEMP)
                {
                    this.WorkLight.color = Color.Lerp(this.WorkLight.color, Color.red, Time.deltaTime);
                    this.WorkLight.range += 0.1f;
                }
                else
                {
                    this.WorkLight.color = Color.Lerp(this.WorkLight.color, Color.cyan, Time.deltaTime);
                    this.WorkLight.range += 0.1f;
                }
            }
            else if (this.mnStorageUsed == 0)
            {
                this.WorkLight.color = Color.Lerp(this.WorkLight.color, Color.green, Time.deltaTime);
                this.WorkLight.range += 0.1f;
            }
            else if (this.mnStorageFree == 0)
            {
                this.WorkLight.color = Color.Lerp(this.WorkLight.color, Color.red, Time.deltaTime);
                this.WorkLight.range += 0.1f;
            }
            else
            {
                this.WorkLight.color = Color.Lerp(this.WorkLight.color, Color.cyan, Time.deltaTime);
                this.WorkLight.range -= 0.1f;
            }
            if (this.WorkLight.range > 1f)
            {
                this.WorkLight.range = 1f;
            }
        }
        if (this.WorkLight.enabled)
        {
            if (this.WorkLight.range < 0.15f)
            {
                this.WorkLight.enabled = false;
            }
            else
            {
                this.WorkLight.range *= 0.95f;
            }
        }
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return ((base.mValue == CRYO_HOPPER) || (base.mValue == MOTORISED_HOPPER));
    }

    public void AddText(ref string OriText, object TextToAdd, bool print)
    {
        string tmpText = OriText;
        tmpText = tmpText + "\n" + TextToAdd.ToString();
        OriText = tmpText;
        if (print)
        {
            Variables.Log(OriText);
        }
    }

    public void printLocation(BinaryWriter writer)
    {
        // Variables.LogValue("BinaryWriter Writer Location", writer.BaseStream.Position.ToString());
    }

    public void printLocation(BinaryReader Reader)
    {
        // Variables.LogValue("BinaryReader Reader Location", Reader.BaseStream.Position.ToString());
    }

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
        Variables.Log("["+this.HopperName+"] Writer done in " + stopwatch.ElapsedMilliseconds + ", writerLocation = " + writer.BaseStream.Position);
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
        Variables.Log("["+this.HopperName+"] Reading done in " + stopwatch.ElapsedMilliseconds + ", readerLocation = " + reader.BaseStream.Position);

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

    /* --------------------------------------------------------------------------------------------------- */

    public eHopperPermissions GetPermissions()
    {
        return mPermissions;
    }

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

            LogisticsOperation();
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
        if (mnStorageFree > 0)
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
        if (mnStorageFree <= 0)
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
        return AddItem(item);
    }

    public bool TryInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
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

    public int TryPartialInsert(StorageUserInterface sourceEntity, ItemBase item)
    {
        int totalItemCount = ItemManager.GetCurrentStackSize(item);

        int insertItemCount = totalItemCount;

        ItemBase copyItem;
        if (totalItemCount > mnStorageFree)
        {
            insertItemCount = mnStorageFree;
            copyItem = ItemManager.CloneItem(item, insertItemCount);
        }
        else
        {
            copyItem = item;
        }

        if (false == AddItem(copyItem))
        {
            // This should not happen. Race condition?
            return 0;
        }

        return insertItemCount;
    }

    public int TryPartialInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount)
    {
        int remaining = amount;

        while (remaining > 0 && mnStorageFree > 0)
        {
            AddCube(cube, value);
            remaining--;
        }

        return amount - remaining;
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
    // ***************************************************************************************************************************************
    void FinaliseHopperChange()
    {
        MarkDirtyDelayed();
        RequestImmediateNetworkUpdate();
        CountFreeSlots();
    }
    // ***************************************************************************************************************************************
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

    public delegate bool IterateItem(ItemBase item);
}


