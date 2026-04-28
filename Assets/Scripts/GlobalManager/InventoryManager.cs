using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    private Dictionary<string, ItemSO> itemDatabase = new Dictionary<string, ItemSO>();
    private Dictionary<string, int> playerInventory = new Dictionary<string, int>();

    void Awake()
    {
        if (instance == null) 
        { 
            instance = this; 
            DontDestroyOnLoad(gameObject);

            ItemSO[] loadedItems = Resources.LoadAll<ItemSO>("Items");
            foreach (ItemSO item in loadedItems)
            {
                itemDatabase[item.itemId] = item;
            }
        }
        else 
        {
            Destroy(gameObject); 
        }
    }

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
        return playerInventory.Select(itemAmountPair => new ItemSaveState { itemId = itemAmountPair.Key, amount = itemAmountPair.Value }).ToList();
    }

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
        if (SaveManager.instance != null)
        {
            SaveManager.instance.UpdateAllUI();
        }
    }

    public void DeductItem(string itemId, int amount)
    {
        if (playerInventory.ContainsKey(itemId))
        {
            playerInventory[itemId] -= amount;
            if (playerInventory[itemId] < 0) playerInventory[itemId] = 0;

            if (SaveManager.instance != null)
            {
                SaveManager.instance.UpdateAllUI();
            }
        }
    }

    public Dictionary<string, int> GetAllItems()
    {
        return playerInventory;
    }
}