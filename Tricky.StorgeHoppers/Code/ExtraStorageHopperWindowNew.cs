using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

public class ExtraStorageHopperWindowNew : BaseMachineWindow
{
    public const string InterfaceName = "ExtraStorageHopperWindowNew";
    public const string InterfaceSetAddRemove = "SetAddRemove";
    public const string InterfaceSetAddOnly = "SetAddOnly";
    public const string InterfaceSetRemoveOnly = "SetRemoveOnly";
    public const string InterfaceSetLocked = "SetLocked";
    public const string InterfaceToggleHoover = "ToggleHoover";
    public const string InterfaceTakeItems = "TakeItems";
    public const string InterfaceStoreItems = "StoreItems";
    public static bool dirty;
    public static bool networkRedraw;
    public int SlotCount;
    public override void SpawnWindow(SegmentEntity targetEntity)
    {
        ExtraStorageHoppers hopper = targetEntity as ExtraStorageHoppers;
        if (hopper == null)
        {
            GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }
        float x = GenericMachinePanelScript.instance.Label_Holder.transform.position.x;
        float y = GenericMachinePanelScript.instance.Label_Holder.transform.position.y;
        GenericMachinePanelScript.instance.Label_Holder.transform.position = new Vector3(x, y, 69.3f);
        string title = "UNKNOWN TYPE";
        if (hopper.GetHopperName() != "")
        {
            title = hopper.GetHopperName();
        }
        this.manager.SetTitle(title);
        int num = 0;
        int num2 = 40;
        int num3 = 50;
        int num4 = 60;
        int num5 = 60;
        int num6 = 40;
        int num7 = num2 + num3 + 50 + num6 + 50;
        this.manager.AddTabButton("AddRemoveButton", "Add and Remove", true, 40, 0);
        this.manager.AddTabButton("LockedButton", "Locked", true, 160, 0);
        this.manager.AddTabButton("AddOnlyButton", "Add Only", true, 40, 40);
        this.manager.AddTabButton("RemoveOnlyButton", "Remove Only", true, 160, 40);
        this.manager.AddButton("ToggleHoover", "Toggle Vacuum", 30, 90);
        this.manager.AddBigLabel("HooverStatus", "Vacuum: Off", Color.white, 180, 90);
        this.manager.AddButton("ToggleShare", "Toggle Share", 30, 140);
        this.manager.AddBigLabel("ShareStatus", "Share Mode: Off", Color.white, 180, 140);
        this.manager.AddBigLabel("UsedStorage", "8888/8888", Color.white, 10, 190);
        this.SlotCount = this.CountUnique(hopper);
        for (int i = 0; i <= this.SlotCount; i++)
        {
            if (hopper.IsFull() && i == this.SlotCount)
            {
                break;
            }
            int num8 = i / 5;
            int num9 = i % 5;
            this.manager.AddIcon("ItemSlot" + i, "empty", Color.white, num9 * num5 + 10, num8 * num4 + num7 + num);
            this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "StackSize" + i, string.Empty, Color.white, false, num9 * num5 + 33, num8 * num4 + num7 + 22);
        }
        ExtraStorageHopperWindowNew.dirty = true;
    }
    public static void SetNewExamplar(Player player, ExtraStorageHoppers hopper, ItemBase itemToSet)
    {
        hopper.SetExemplar(itemToSet);
        FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "Set The Type to " + ItemManager.GetItemName(itemToSet), Color.blue, 1.5f);
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetExemplar", null, itemToSet, hopper, 0f);
        }
    }

    public static void SetNewExamplar_Fail(Player player, ExtraStorageHoppers hopper)
    {
        FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "Error: Hopper not empty!", Color.blue, 1.5f);
    }

    public override void UpdateMachine(SegmentEntity targetEntity)
    {
        ExtraStorageHoppers hopper = targetEntity as ExtraStorageHoppers;
        if (hopper == null)
        {
            GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }
        if (ExtraStorageHopperWindowNew.networkRedraw)
        {
            this.Redraw(targetEntity);
            ExtraStorageHopperWindowNew.networkRedraw = false;
        }
        if (targetEntity.mbNetworkUpdated)
        {
            ExtraStorageHopperWindowNew.dirty = true;
            targetEntity.mbNetworkUpdated = false;
        }
        if (ExtraStorageHopperWindowNew.dirty)
        {
            this.WindowUpdate(hopper);
        }
    }

    private void WindowUpdate(ExtraStorageHoppers hopper)
    {
        switch (hopper.mPermissions)
        {
            case eHopperPermissions.AddAndRemove:
                this.manager.UpdateTabButton("AddRemoveButton", false);
                this.manager.UpdateTabButton("AddOnlyButton", true);
                this.manager.UpdateTabButton("RemoveOnlyButton", true);
                this.manager.UpdateTabButton("LockedButton", true);
                break;
            case eHopperPermissions.RemoveOnly:
                this.manager.UpdateTabButton("AddRemoveButton", true);
                this.manager.UpdateTabButton("AddOnlyButton", true);
                this.manager.UpdateTabButton("RemoveOnlyButton", false);
                this.manager.UpdateTabButton("LockedButton", true);
                break;
            case eHopperPermissions.AddOnly:
                this.manager.UpdateTabButton("AddRemoveButton", true);
                this.manager.UpdateTabButton("AddOnlyButton", false);
                this.manager.UpdateTabButton("RemoveOnlyButton", true);
                this.manager.UpdateTabButton("LockedButton", true);
                break;
            case eHopperPermissions.Locked:
                this.manager.UpdateTabButton("AddRemoveButton", true);
                this.manager.UpdateTabButton("AddOnlyButton", true);
                this.manager.UpdateTabButton("RemoveOnlyButton", true);
                this.manager.UpdateTabButton("LockedButton", false);
                break;
        }
        if (hopper.mbHooverOn)
        {
            this.manager.UpdateLabel("HooverStatus", "Vacuum: On", Color.white);
        }
        else
        {
            this.manager.UpdateLabel("HooverStatus", "Vacuum: Off", Color.white);
        }
        if (hopper.ShareContent)
        {
            this.manager.UpdateLabel("ShareStatus", "Share Mode: On", Color.white);
        }
        else
        {
            this.manager.UpdateLabel("ShareStatus", "Share Mode: Off", Color.white);
        }
        this.manager.UpdateLabel("UsedStorage", string.Concat(new object[]
        {
            "Used ",
            hopper.UsedCapacity,
            "/",
            hopper.TotalCapacity
        }), Color.white);
        int num = 0;
        for (int i = 0; i < hopper.TotalCapacity; i++)
        {
            ItemBase itemBase = hopper.maItemInventory[i];
            if (itemBase != null)
            {
                string itemIcon = ItemManager.GetItemIcon(itemBase);
                int currentStackSize = ItemManager.GetCurrentStackSize(itemBase);
                string label = (currentStackSize != 100) ? ((currentStackSize >= 10) ? (" " + currentStackSize.ToString()) : ("   " + currentStackSize.ToString())) : currentStackSize.ToString();
                this.manager.UpdateIcon("ItemSlot" + num, itemIcon, Color.white);
                this.manager.UpdateLabel("StackSize" + num, label, Color.white);
                num++;
            }
        }
        ExtraStorageHopperWindowNew.dirty = false;
    }

    public override bool ButtonClicked(string name, SegmentEntity targetEntity)
    {
        ExtraStorageHoppers hopper = targetEntity as ExtraStorageHoppers;
        if (name == "AddRemoveButton")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            ExtraStorageHopperWindowNew.SetPermissions(WorldScript.mLocalPlayer, hopper, eHopperPermissions.AddAndRemove);
            return true;
        }
        if (name == "AddOnlyButton")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            ExtraStorageHopperWindowNew.SetPermissions(WorldScript.mLocalPlayer, hopper, eHopperPermissions.AddOnly);
            return true;
        }
        if (name == "RemoveOnlyButton")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            ExtraStorageHopperWindowNew.SetPermissions(WorldScript.mLocalPlayer, hopper, eHopperPermissions.RemoveOnly);
            return true;
        }
        if (name == "LockedButton")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            ExtraStorageHopperWindowNew.SetPermissions(WorldScript.mLocalPlayer, hopper, eHopperPermissions.Locked);
            return true;
        }
        if (name == "ToggleHoover")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            AudioHUDManager.instance.HUDIn();
            ExtraStorageHopperWindowNew.ToggleHoover(WorldScript.mLocalPlayer, hopper);
            return true;
        }
        if (name == "ToggleShare")
        {
            UIManager.ForceNGUIUpdate = 0.1f;
            AudioHUDManager.instance.HUDIn();
            ExtraStorageHopperWindowNew.ToggleShare(WorldScript.mLocalPlayer, hopper);
            return true;
        }
        if (name.Contains("ItemSlot"))
        {
            int num = -1;
            int.TryParse(name.Replace("ItemSlot", string.Empty), out num);
            if (num > -1)
            {
                int num2 = 100;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    num2 = 10;
                }
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    num2 = 1;
                }
                ItemBase item = ItemManager.CloneItem(this.GetNthItemSlot(hopper, num));
                if (num2 < ItemManager.GetCurrentStackSize(item))
                {
                    ItemManager.SetItemCount(item, num2);
                    if (hopper.IsFull())
                    {
                        ExtraStorageHopperWindowNew.networkRedraw = true;
                    }
                    else
                    {
                        ExtraStorageHopperWindowNew.dirty = true;
                    }
                }
                else
                {
                    ExtraStorageHopperWindowNew.networkRedraw = true;
                }
                ExtraStorageHopperWindowNew.TakeItems(WorldScript.mLocalPlayer, hopper, item);
                UIManager.ForceNGUIUpdate = 0.1f;
                AudioHUDManager.instance.OrePickup();
                if (WorldScript.meGameMode == eGameMode.eSurvival && SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.RemoveCoalFromHopper)
                {
                    SurvivalPlayerScript.TutorialSectionComplete();
                }
                typeof(ExtraStorageHoppers).GetField("mRetakeDebounce", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hopper, 0.5f);
                return true;
            }
        }
        return false;
    }

    public override void HandleItemDrag(string name, ItemBase draggedItem, DragAndDropManager.DragRemoveItem dragDelegate, SegmentEntity targetEntity)
    {


        ExtraStorageHoppers hopper = targetEntity as ExtraStorageHoppers;
        ItemBase itemForSlot = this.GetItemForSlot(hopper, name);
        bool flag = true;
        if (itemForSlot != null)
        {
            flag = (draggedItem.mnItemID == itemForSlot.mnItemID);
        }
        if (hopper.OT && hopper.IsEmpty() && !hopper.ExemplarSet)
        {
            hopper.SetExemplar(draggedItem);
        }
        if (name == "ItemSlot" + this.SlotCount && flag && hopper.IsNotFull() && hopper.CheckExemplar(draggedItem))
        {
            ItemBase itemBase = ItemManager.CloneItem(draggedItem);
            int currentStackSize = ItemManager.GetCurrentStackSize(itemBase);
            if (hopper.RemainingCapacity < currentStackSize)
            {
                ItemManager.SetItemCount(itemBase, hopper.RemainingCapacity);
            }
            ExtraStorageHopperWindowNew.StoreItems(WorldScript.mLocalPlayer, hopper, itemBase);
            InventoryPanelScript.mbDirty = true;
            SurvivalHotBarManager.MarkAsDirty();
            SurvivalHotBarManager.MarkContentDirty();
            ExtraStorageHopperWindowNew.networkRedraw = true;
        }
    }

    public override void ButtonEnter(string name, SegmentEntity targetEntity)
    {
        ExtraStorageHoppers hopper = targetEntity as ExtraStorageHoppers;
        string empty = string.Empty;
        ItemBase itemForSlot = this.GetItemForSlot(hopper, name);
        if (itemForSlot == null)
        {
            return;
        }
        if (HotBarManager.mbInited)
        {
            HotBarManager.SetCurrentBlockLabel(ItemManager.GetItemName(itemForSlot));
        }
        else
        {
            if (!SurvivalHotBarManager.mbInited)
            {
                return;
            }
            string text = WorldScript.mLocalPlayer.mResearch.IsKnown(itemForSlot) ? ItemManager.GetItemName(itemForSlot) : "Unknown Material";
            int currentStackSize = ItemManager.GetCurrentStackSize(itemForSlot);
            if (currentStackSize > 1)
            {
                SurvivalHotBarManager.SetCurrentBlockLabel(string.Format("{0} {1}", currentStackSize, text));
            }
            else
            {
                SurvivalHotBarManager.SetCurrentBlockLabel(text);
            }
        }
    }

    private ItemBase GetItemForSlot(ExtraStorageHoppers hopper, string name)
    {
        ItemBase result = null;
        int num = -1;
        int.TryParse(name.Replace("ItemSlot", string.Empty), out num);
        if (num > -1)
        {
            result = this.GetNthItemSlot(hopper, num);
        }
        return result;
    }

    private int CountUnique(ExtraStorageHoppers hopper)
    {
        int num = 0;
        int totalCapacity = hopper.TotalCapacity;
        for (int i = 0; i < totalCapacity; i++)
        {
            if (hopper.maItemInventory[i] != null)
            {
                num++;
            }
        }
        return num;
    }

    private ItemBase GetNthItemSlot(ExtraStorageHoppers hopper, int n)
    {
        int num = -1;
        for (int i = 0; i < hopper.TotalCapacity; i++)
        {
            if (hopper.maItemInventory[i] != null)
            {
                num++;
                if (num == n)
                {
                    return hopper.maItemInventory[i];
                }
            }
        }
        return null;
    }

    public static bool SetPermissions(Player player, ExtraStorageHoppers hopper, eHopperPermissions permissions)
    {
        if (hopper.mPermissions != permissions)
        {
            hopper.mPermissions = permissions;
        }
        if (!WorldScript.mbIsServer)
        {
            switch (permissions)
            {
                case eHopperPermissions.AddAndRemove:
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetAddRemove", null, null, hopper, 0f);
                    break;
                case eHopperPermissions.RemoveOnly:
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetRemoveOnly", null, null, hopper, 0f);
                    break;
                case eHopperPermissions.AddOnly:
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetAddOnly", null, null, hopper, 0f);
                    break;
                case eHopperPermissions.Locked:
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetLocked", null, null, hopper, 0f);
                    break;
                default:
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "SetAddRemove", null, null, hopper, 0f);
                    break;
            }
        }
        //AudioHUDManager.instance.HUDIn();
        //AudioSpeechManager.instance.UpdateStorageHopper(hopper.mPermissions);
        hopper.MarkDirtyDelayed();
        typeof(ExtraStorageHoppers).GetField("mbForceTextUpdate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hopper, true);
        typeof(ExtraStorageHoppers).GetField("mbForceHoloUpdate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hopper, true);
        hopper.RequestImmediateNetworkUpdate();
        FloatingCombatTextQueue floatingCombatTextQueue = FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, hopper.mPermissions.ToString(), Color.green, 1.5f, 64f);
        ExtraStorageHopperWindowNew.dirty = true;
        if (floatingCombatTextQueue != null)
        {
            floatingCombatTextQueue.mrStartRadiusRand = 0.25f;
        }
        return true;
    }

    public static bool ToggleHoover(Player player, ExtraStorageHoppers hopper)
    {
        hopper.ToggleHoover();
        ExtraStorageHopperWindowNew.dirty = true;
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "ToggleHoover", null, null, hopper, 0f);
        }
        return true;
    }

    public static bool ToggleShare(Player player, ExtraStorageHoppers hopper)
    {
        hopper.ToggleShareContent();
        ExtraStorageHopperWindowNew.dirty = true;
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "ToggleShare", null, null, hopper, 0f);
        }
        return true;
    }

    public static bool TakeItems(Player player, ExtraStorageHoppers hopper, ItemBase item)
    {
        //ENABLE/DISABLE FEEDING OF HIVEBIND - ONLY FOR VOID HOPPER
        if (hopper.GetCubeValue() == 0)
        {
            if (hopper.FeedHiveMind)
            {
                hopper.FeedHiveMind = false;
                FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, "Not Feeding Hivemind!", Color.green, 2f);
            }
            else
            {
                hopper.FeedHiveMind = true;
                FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, "Feeding Hivemind!", Color.red, 2f);
            }
            return true;
        }
        else if (hopper.mnStorageUsed > 0)
        {
            ItemBase itemBase;
            if (item == null)
            {
                itemBase = hopper.RemoveFirstInventoryItem();
            }
            else if (item.mType == ItemType.ItemCubeStack)
            {
                hopper.TryPartialExtractItemsOrCubes(null, item.mnItemID, (item as ItemCubeStack).mCubeType, (item as ItemCubeStack).mCubeValue, ItemManager.GetCurrentStackSize(item), out itemBase);
            }
            else
            {
                hopper.TryPartialExtractItems(null, item.mnItemID, ItemManager.GetCurrentStackSize(item), out itemBase);
            }
            if (itemBase != null)
            {
                Debug.Log("Removing Item from StorageHopper for " + player.mUserName);
                if (!player.mInventory.AddItem(itemBase))
                {
                    if (!hopper.AddItem(itemBase))
                    {
                        ItemManager.instance.DropItem(itemBase, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
                    }
                    return false;
                }
                if (player.mbIsLocalPlayer)
                {
                    Color lCol = Color.green;
                    if (itemBase.mType == ItemType.ItemCubeStack)
                    {
                        ItemCubeStack itemCubeStack = itemBase as ItemCubeStack;
                        if (CubeHelper.IsGarbage(itemCubeStack.mCubeType))
                        {
                            lCol = Color.red;
                        }
                        if (CubeHelper.IsSmeltableOre(itemCubeStack.mCubeType))
                        {
                            lCol = Color.green;
                        }
                    }
                    if (itemBase.mType == ItemType.ItemStack)
                    {
                        lCol = Color.cyan;
                    }
                    if (itemBase.mType == ItemType.ItemSingle)
                    {
                        lCol = Color.white;
                    }
                    if (itemBase.mType == ItemType.ItemCharge)
                    {
                        lCol = Color.magenta;
                    }
                    if (itemBase.mType == ItemType.ItemDurability)
                    {
                        lCol = Color.yellow;
                    }
                    if (itemBase.mType == ItemType.ItemLocation)
                    {
                        lCol = Color.gray;
                    }
                    FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, player.GetItemName(itemBase), lCol, 1.5f, 64f);
                }
                player.mInventory.VerifySuitUpgrades();
                if (!WorldScript.mbIsServer)
                {
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "TakeItems", null, itemBase, hopper, 0f);
                }
                return true;
            }
        }
        return false;
    }

    public static bool StoreItems(Player player, ExtraStorageHoppers hopper, ItemBase itemToStore)
    {
        if (player == WorldScript.mLocalPlayer && !WorldScript.mLocalPlayer.mInventory.RemoveItemByExample(itemToStore, true))
        {
            Debug.Log(string.Concat(new object[]
            {
                "Player ",
                player.mUserName,
                " doesnt have ",
                itemToStore
            }));
            return false;
        }
        if (hopper.AddItem(itemToStore) && hopper.CheckExemplar(itemToStore))
        {
            if (player.mbIsLocalPlayer)
            {
                Color lCol = Color.green;
                if (itemToStore.mType == ItemType.ItemCubeStack)
                {
                    ItemCubeStack itemCubeStack = itemToStore as ItemCubeStack;
                    if (CubeHelper.IsGarbage(itemCubeStack.mCubeType))
                    {
                        lCol = Color.red;
                    }
                    if (CubeHelper.IsSmeltableOre(itemCubeStack.mCubeType))
                    {
                        lCol = Color.green;
                    }
                }
                if (itemToStore.mType == ItemType.ItemStack)
                {
                    lCol = Color.cyan;
                }
                if (itemToStore.mType == ItemType.ItemSingle)
                {
                    lCol = Color.white;
                }
                if (itemToStore.mType == ItemType.ItemCharge)
                {
                    lCol = Color.magenta;
                }
                if (itemToStore.mType == ItemType.ItemDurability)
                {
                    lCol = Color.yellow;
                }
                if (itemToStore.mType == ItemType.ItemLocation)
                {
                    lCol = Color.gray;
                }
                FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "Stored " + player.GetItemName(itemToStore), lCol, 1.5f, 64f);
            }
            player.mInventory.VerifySuitUpgrades();
            if (!WorldScript.mbIsServer)
            {
                NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "StoreItems", null, itemToStore, hopper, 0f);
            }
            return true;
        }
        Debug.LogWarning("Bad thing that used to be unhandled! Thread interaccess probably caused this to screw up!");
        if (player == WorldScript.mLocalPlayer)
        {
            WorldScript.mLocalPlayer.mInventory.AddItem(itemToStore);
            return false;
        }
        player.mInventory.AddItem(itemToStore);
        return false;
    }
    public static void DebugMode(Player player, ExtraStorageHoppers hopper)
    {
        hopper.ToggleDebugMode();
        hopper.PrintAllHopperInfo();

        FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "SET DEBUG MODE TO: " + hopper.GetDebugMode(), new Color(0, 2, 0), 1.5f);
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindowNew", "DebugMode", null, null, hopper, 0f);
        }
    }

    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
    {
        ExtraStorageHoppers hopper = nic.target as ExtraStorageHoppers;
        string command = nic.command;
        if (command != null)
        {
            string text = command;
            switch (text)
            {
                case "SetAddRemove":
                    ExtraStorageHopperWindowNew.SetPermissions(player, hopper, eHopperPermissions.AddAndRemove);
                    break;
                case "SetAddOnly":
                    ExtraStorageHopperWindowNew.SetPermissions(player, hopper, eHopperPermissions.AddOnly);
                    break;
                case "SetRemoveOnly":
                    ExtraStorageHopperWindowNew.SetPermissions(player, hopper, eHopperPermissions.RemoveOnly);
                    break;
                case "SetLocked":
                    ExtraStorageHopperWindowNew.SetPermissions(player, hopper, eHopperPermissions.Locked);
                    break;
                case "ToggleHoover":
                    ExtraStorageHopperWindowNew.ToggleHoover(player, hopper);
                    break;
                case "ToggleShare":
                    ExtraStorageHopperWindowNew.ToggleShare(player, hopper);
                    break;
                case "TakeItems":
                    ExtraStorageHopperWindowNew.TakeItems(player, hopper, nic.itemContext);
                    break;
                case "StoreItems":
                    ExtraStorageHopperWindowNew.StoreItems(player, hopper, nic.itemContext);
                    break;
                case "DebugMode":
                    ExtraStorageHopperWindowNew.DebugMode(player, hopper);
                    break;
                case "SetExemplar":
                    ExtraStorageHopperWindowNew.SetNewExamplar(player, hopper, nic.itemContext);
                    break;
            }
        }
        return new NetworkInterfaceResponse
        {
            entity = hopper,
            inventory = player.mInventory
        };
    }
}
