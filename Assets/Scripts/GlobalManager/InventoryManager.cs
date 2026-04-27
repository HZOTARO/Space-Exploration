using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    // Runtime inventory for instant lookups
    private Dictionary<string, int> playerInventory = new Dictionary<string, int>();

    // Database of all items in the game
    private Dictionary<string, ItemSO> itemDatabase = new Dictionary<string, ItemSO>();

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // 1. Instantly load every item definition from your Resources folder
        ItemSO[] loadedItems = Resources.LoadAll<ItemSO>("Items");
        foreach (var item in loadedItems)
        {
            itemDatabase[item.itemId] = item;
        }
    }

    // --- SAVE / LOAD ---
    public void LoadInventory(List<ItemSaveState> savedInventory)
    {
        playerInventory.Clear();
        if (savedInventory != null)
        {
            foreach (var saveState in savedInventory)
            {
                playerInventory[saveState.itemId] = saveState.amount;
            }
        }
    }

    public List<ItemSaveState> GetInventoryForSave()
    {
        return playerInventory.Select(kvp => new ItemSaveState { itemId = kvp.Key, amount = kvp.Value }).ToList();
    }

    // --- GENERIC ITEM LOGIC ---

    public ItemSO GetItemData(string itemId)
    {
        itemDatabase.TryGetValue(itemId, out ItemSO item);
        return item;
    }

    public int GetAmount(string itemId)
    {
        if (playerInventory.TryGetValue(itemId, out int amount)) return amount;
        return 0;
    }

    public void AddItem(string itemId, int amount)
    {
        if (!playerInventory.ContainsKey(itemId)) playerInventory[itemId] = 0;
        playerInventory[itemId] += amount;
    }

    public void DeductItem(string itemId, int amount)
    {
        if (playerInventory.ContainsKey(itemId))
        {
            playerInventory[itemId] -= amount;
            if (playerInventory[itemId] < 0) playerInventory[itemId] = 0;
        }
    }

    // You can loop through this to populate your UI!
    public Dictionary<string, int> GetAllItems()
    {
        return playerInventory;
    }
}