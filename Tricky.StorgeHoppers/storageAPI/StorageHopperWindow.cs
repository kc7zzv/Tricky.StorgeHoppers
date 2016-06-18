using UnityEngine;


public abstract class StorageHopperWindow
{
	public const string InterfaceName = "StorageHopperWindow";
	
	public const string InterfaceTogglePermissions = "TogglePermissions";
	public const string InterfaceToggleHoover = "ToggleHoover";
	public const string InterfaceTakeItems  = "TakeItems";
	public const string InterfaceStoreItems = "StoreItems";
	
	
	public static bool TogglePermissions(Player player, StorageHopper hopper)
	{
		hopper.TogglePermissions();
		
		if (!WorldScript.mbIsServer)
		{
			NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceTogglePermissions, null, null, hopper, 0);
		}
		return true; // not necessary, but makes things more readable
	}
	
	public static bool ToggleHoover(Player player, StorageHopper hopper)
	{
		hopper.ToggleHoover();
		
		if (!WorldScript.mbIsServer)
		{
			NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceToggleHoover, null, null, hopper, 0);
		}
		return true; // not necessary, but makes things more readable
	}	

	public static bool TakeItems (Player player, StorageHopper hopper)
	{
		if (hopper.mnStorageUsed > 0)
		{
//			ushort lCube, lValue;
//			lSelectedHopper.GetSpecificCube(eHopperRequestType.eAny, out lCube, out lValue);//this also reduces CubeStacks
//			if (lCube != eCubeTypes.NULL)
//			{
//				Debug.Log("Transferring "+ TerrainData.mEntries[lCube].Name +" from StorageHopper to player inventory");
//				WorldScript.mLocalPlayer.mInventory.CollectValue(lCube, lValue, 1);
//				WorldScript.mLocalPlayer.mInventory.VerifySuitUpgrades();//we may have just taken a suit upgrade OUT.
//				return;
//			}
			//only remove Items now
            //This is really ficking stupid, we should duplicate the item and ASK if it'll fit and THEN remove it. Please god someone change this.
			ItemBase lItem = hopper.RemoveFirstInventoryItem();
			if (lItem != null)
			{
				Debug.Log("RemovingFirstInventoryItem from StorageHopper for " + player.mUserName);


				//This has a potential issue where the server and client diverge 

				//TODO: ensure it fits?
				if (!player.mInventory.AddItem(lItem))
				{

                    //Do we know why it didn't fit?

					//Doesn't fit, put it back in
					if (!hopper.AddItem(lItem))//oh dear god threading
					{
						//THROW IT TO THE GROUND
						ItemManager.instance.DropItem(lItem, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
					}
					return false;
				}
				else
				{
					if (player.mbIsLocalPlayer == true)
					{
						//if we know what sort of item it was, colour the text!

						Color lCol = Color.green;
						if (lItem.mType == ItemType.ItemCubeStack)
						{
							ItemCubeStack lCubeStack = lItem as ItemCubeStack;
							if (CubeHelper.IsGarbage(lCubeStack.mCubeType)) lCol = Color.red;
							if (CubeHelper.IsSmeltableOre(lCubeStack.mCubeType)) lCol = Color.green;
						}
						if (lItem.mType == ItemType.ItemStack)
						{
							//probably a crafted item of some sort; we could introspect further into bar types
							lCol = Color.cyan;
						}
						if (lItem.mType == ItemType.ItemSingle)
						{
							//Minecart
							lCol = Color.white;
						}
						if (lItem.mType == ItemType.ItemCharge)
						{
							//Drill head/Minecart
							lCol = Color.magenta;
						}
						if (lItem.mType == ItemType.ItemDurability)
						{
							//Drill head
							lCol = Color.yellow;
						}
						if (lItem.mType == ItemType.ItemLocation)
						{
							lCol = Color.gray;
						}

						FloatingCombatTextManager.instance.QueueText(hopper.mnX,hopper.mnY+1,hopper.mnZ,1.0f,player.GetItemName(lItem),lCol,1.5f);
					}
				}
				player.mInventory.VerifySuitUpgrades();//we may have just taken a suit upgrade OUT.
				
				if (!WorldScript.mbIsServer)//Tell the server we took the items
				{
					NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceTakeItems, null, lItem, hopper, 0);
				}

				return true;
			}
			
		}
		return false;
	}
	
	public static bool StoreItems(Player player, StorageHopper hopper, ItemBase itemToStore)
	{
		if (player == WorldScript.mLocalPlayer)
		{
			if (!WorldScript.mLocalPlayer.mInventory.RemoveItemByExample(itemToStore, true))
			{
				// player didn't have this item
				Debug.Log ("Player " + player.mUserName + " doesnt have " + itemToStore);
				return false;
			}
		}

		//stored first to reduce the gap - the VSU below could take several hundred MS due to logging.
		if (!hopper.AddItem(itemToStore))
		{
			Debug.LogWarning("Bad thing that used to be unhandled! Thread interaccess probably caused this to screw up!");

			if (player == WorldScript.mLocalPlayer)
			{
				WorldScript.mLocalPlayer.mInventory.AddItem(itemToStore);
				return false;
			}
			else
			{
				//Give the NETWORK player their items back! 
				player.mInventory.AddItem(itemToStore);//This needs to be communicated to the player, NOT just added to the local version of the inventory
				return false;
			}
		}

		if (player.mbIsLocalPlayer)
		{
			Color lCol = Color.green;
			ItemBase lItem = itemToStore;
			if (lItem.mType == ItemType.ItemCubeStack)
			{
				ItemCubeStack lCubeStack = lItem as ItemCubeStack;
				if (CubeHelper.IsGarbage(lCubeStack.mCubeType)) lCol = Color.red;
				if (CubeHelper.IsSmeltableOre(lCubeStack.mCubeType)) lCol = Color.green;
			}
			if (lItem.mType == ItemType.ItemStack)
			{
				//probably a crafted item of some sort; we could introspect further into bar types
				lCol = Color.cyan;
			}
			if (lItem.mType == ItemType.ItemSingle)
			{
				//Minecart
				lCol = Color.white;
			}
			if (lItem.mType == ItemType.ItemCharge)
			{
				//Drill head/Minecart
				lCol = Color.magenta;
			}
			if (lItem.mType == ItemType.ItemDurability)
			{
				//Drill head
				lCol = Color.yellow;
			}
			if (lItem.mType == ItemType.ItemLocation)
			{
				lCol = Color.gray;
			}
			
			
			FloatingCombatTextManager.instance.QueueText(hopper.mnX,hopper.mnY+1,hopper.mnZ,0.75f,"Stored " + player.GetItemName(lItem),lCol,1.5f);
		}

		player.mInventory.VerifySuitUpgrades();//we may have just taken a suit upgrade OUT.
		
		if (!WorldScript.mbIsServer)
		{
			NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceStoreItems, null, itemToStore, hopper, 0);
		}		
		
		return true;
	}
	
	
	//******************** Network Interface ****************************
	
	public static NetworkInterfaceResponse HandleNetworkCommand (Player player, NetworkInterfaceCommand nic)
	{
		StorageHopper sh = nic.target as StorageHopper;
		switch(nic.command)
		{
		case InterfaceTogglePermissions:
			TogglePermissions(player, sh);
			break;
			
		case InterfaceToggleHoover:
			ToggleHoover(player, sh);
			break;			
			
		case InterfaceTakeItems:
			TakeItems(player, sh);
			break;
			
		case InterfaceStoreItems:
			StoreItems(player, sh, nic.itemContext);
			break;			
			
		}
		
		NetworkInterfaceResponse response = new NetworkInterfaceResponse();
		response.entity = sh;
		response.inventory = player.mInventory;
		
		return response;
	}
	
}


