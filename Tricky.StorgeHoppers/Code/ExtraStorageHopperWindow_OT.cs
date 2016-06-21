using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class ExtraStorageHopperWindow_OT
{
    public const string InterfaceName = "ExtraStorageHopperWindow_OT";

    public const string InterfaceTogglePermissions = "TogglePermissions";

    public const string InterfaceToggleHoover = "ToggleHoover";

    public const string InterfaceTakeItems = "TakeItems";

    public const string InterfaceStoreItems = "StoreItems";
    public static bool StoreItems(Player player, ExtraStorageHoppers_OT hopper, ItemBase itemToStore)
    {
        if ((player == WorldScript.mLocalPlayer) && !WorldScript.mLocalPlayer.mInventory.RemoveItemByExample(itemToStore, true))
        {
            Debug.Log(string.Concat(new object[] { "Player ", player.mUserName, " doesnt have ", itemToStore.GetDisplayString() }));
            return false;
        }
        if (!hopper.AddItem(itemToStore))
        {
            Debug.LogWarning("Bad thing that used to be unhandled! Thread interaccess probably caused this to screw up!");
            if (player == WorldScript.mLocalPlayer)
            {
                WorldScript.mLocalPlayer.mInventory.AddItem(itemToStore);
                return false;
            }
            player.mInventory.AddItem(itemToStore);
            return false;
        }
        if (player.mbIsLocalPlayer)
        {
            Color green = Color.green;
            ItemBase lItem = itemToStore;
            if (lItem.mType == ItemType.ItemCubeStack)
            {
                ItemCubeStack stack = lItem as ItemCubeStack;
                if (CubeHelper.IsGarbage(stack.mCubeType))
                {
                    green = Color.red;
                }
                if (CubeHelper.IsSmeltableOre(stack.mCubeType))
                {
                    green = Color.green;
                }
            }
            if (lItem.mType == ItemType.ItemStack)
            {
                green = Color.cyan;
            }
            if (lItem.mType == ItemType.ItemSingle)
            {
                green = Color.white;
            }
            if (lItem.mType == ItemType.ItemCharge)
            {
                green = Color.magenta;
            }
            if (lItem.mType == ItemType.ItemDurability)
            {
                green = Color.yellow;
            }
            if (lItem.mType == ItemType.ItemLocation)
            {
                green = Color.gray;
            }
            FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, "Stored " + lItem.GetDisplayString(), green, 1.5f);
            char[] whitespace = new char[] { ' ', '\t' };
            
        }

        player.mInventory.VerifySuitUpgrades();
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindow_OT", "StoreItems", null, itemToStore, hopper, 0f);
        }
        return true;
    }


    public static void SetNewExamplar(Player player, ExtraStorageHoppers_OT hopper, ItemBase itemToSet)
    {
        FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "Set The Type to " + ItemManager.GetItemName(itemToSet), Color.blue, 1.5f);
    }

    public static void SetNewExamplar_Fail(Player player, ExtraStorageHoppers_OT hopper)
    {
        FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 0.75f, "Error: Hopper not empty!", Color.blue, 1.5f);
    }

    public static bool TakeItems(Player player, ExtraStorageHoppers_OT hopper)
    {
        if (hopper.mnStorageUsed > 0)
        {
            ItemBase lItemToAdd = hopper.RemoveFirstInventoryItem();
            if (lItemToAdd != null)
            {
                Debug.Log("RemovingFirstInventoryItem from for " + player.mUserName);
                if (!player.mInventory.AddItem(lItemToAdd))
                {
                    if (!hopper.AddItem(lItemToAdd))
                    {
                        ItemManager.instance.DropItem(lItemToAdd, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
                    }
                    return false;
                }
                if (player.mbIsLocalPlayer)
                {
                    Color green = Color.green;
                    if (lItemToAdd.mType == ItemType.ItemCubeStack)
                    {
                        ItemCubeStack stack = lItemToAdd as ItemCubeStack;
                        if (CubeHelper.IsGarbage(stack.mCubeType))
                        {
                            green = Color.red;
                        }
                        if (CubeHelper.IsSmeltableOre(stack.mCubeType))
                        {
                            green = Color.green;
                        }
                    }
                    if (lItemToAdd.mType == ItemType.ItemStack)
                    {
                        green = Color.cyan;
                    }
                    if (lItemToAdd.mType == ItemType.ItemSingle)
                    {
                        green = Color.white;
                    }
                    if (lItemToAdd.mType == ItemType.ItemCharge)
                    {
                        green = Color.magenta;
                    }
                    if (lItemToAdd.mType == ItemType.ItemDurability)
                    {
                        green = Color.yellow;
                    }
                    if (lItemToAdd.mType == ItemType.ItemLocation)
                    {
                        green = Color.gray;
                    }
                    FloatingCombatTextManager.instance.QueueText(hopper.mnX, hopper.mnY + 1L, hopper.mnZ, 1f, player.GetItemName(lItemToAdd), green, 1.5f);
                }
                player.mInventory.VerifySuitUpgrades();
                if (!WorldScript.mbIsServer)
                {
                    NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindow_OT", "TakeItems", null, lItemToAdd, hopper, 0f);
                }
                return true;
            }
        }
        return false;
    }

    public static bool ToggleHoover(Player player, ExtraStorageHoppers_OT hopper)
    {
        hopper.ToggleHoover();
        if (!WorldScript.mbIsServer)
        {
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindow_OT", "ToggleHoover", null, null, hopper, 0f);
        }
        return true;
    }

    public static bool TogglePermissions(Player player, ExtraStorageHoppers_OT hopper)
    {
        hopper.TogglePermissions();
        if (!WorldScript.mbIsServer)
        { 
            NetworkManager.instance.SendInterfaceCommand("ExtraStorageHopperWindow_OT", "TogglePermissions", null, null, hopper, 0f);
        }
        return true;
    }

    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
    {
        ExtraStorageHoppers_OT storageHopper = nic.target as ExtraStorageHoppers_OT;
        string command = nic.command;
        switch (command)
        {
            case "TogglePermissions":
                ExtraStorageHopperWindow_OT.TogglePermissions(player, storageHopper);
                break;
            case "ToggleHoover":
                ExtraStorageHopperWindow_OT.ToggleHoover(player, storageHopper);
                break;
            case "TakeItems":
                ExtraStorageHopperWindow_OT.TakeItems(player, storageHopper);
                break;
            case "StoreItems":
                ExtraStorageHopperWindow_OT.StoreItems(player, storageHopper, nic.itemContext);
                break;
        }
        return new NetworkInterfaceResponse
        {
            entity = storageHopper,
            inventory = player.mInventory
        };
    }
}
