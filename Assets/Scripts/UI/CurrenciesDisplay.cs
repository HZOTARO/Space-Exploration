using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CurrencyDisplay
{
    public ItemSO itemData;
    public ItemSlotUI slotUI;
}

public class CurrenciesDisplay : MonoBehaviour, IResourceUpdatable
{
    [Header("Resources Reference")]
    public List<CurrencyDisplay> currenciesDisplays;

    void Start()
    {
        foreach (CurrencyDisplay currencyDisplay in currenciesDisplays)
        {
            if (currencyDisplay.slotUI != null && currencyDisplay.itemData != null)
            {
                currencyDisplay.slotUI.itemIcon.sprite = currencyDisplay.itemData.icon;
            }
        }
        UpdateResource(SaveManager.saveData);
    }

    public void UpdateResource(SaveData saveData)
    {
        foreach (CurrencyDisplay currencyDisplay in currenciesDisplays)
        {
            if (currencyDisplay.slotUI != null && currencyDisplay.itemData != null)
            {
                currencyDisplay.slotUI.amountText.text = InventoryManager.instance.GetAmount(currencyDisplay.itemData.itemId).ToString();
            }
        }
    }
}
