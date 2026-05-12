using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCargoComponent : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Range(1, 15)]
    public int cargoSize = 6;
    public Transform cargoUI;

    [HideInInspector]
    public List<ItemAmount> levelCargo = new List<ItemAmount>();
    private List<ItemSlotUI> cargoSlots = new List<ItemSlotUI>();

    public IEnumerator SetupCargoCoroutine()
    {
        if (cargoUI == null) yield break;

        Transform template = null;
        foreach (Transform slotTransform in cargoUI.transform)
        {
            if (template) { Destroy(slotTransform.gameObject); continue; }
            ItemSlotUI itemSlot = slotTransform.GetComponentInChildren<ItemSlotUI>();
            if (itemSlot)
            {
                template = slotTransform;
                template.gameObject.SetActive(false);
            }
        }

        if (template)
        {
            RectTransform slotRectTransform = null;
            for (int i = 0; i < cargoSize; i++)
            {
                Transform newSlot = Instantiate(template, cargoUI, false);
                newSlot.gameObject.SetActive(true);
                newSlot.name = $"Slot ({i + 1})";

                ItemSlotUI newItemSlot = newSlot.GetComponentInChildren<ItemSlotUI>();
                cargoSlots.Add(newItemSlot);
                levelCargo.Add(new ItemAmount { item = null, amount = 0 });

                cargoSlots[i].itemIcon.sprite = null;
                cargoSlots[i].itemIcon.gameObject.SetActive(false);
                cargoSlots[i].amountText.text = "";
                cargoSlots[i].amountText.fontSize = 30 + 6 * ((10 - Mathf.Min(Mathf.Max(cargoSize, 6), 10)) / (10 - 6));

                if (i == cargoSize - 1)
                {
                    slotRectTransform = newSlot.GetChild(0).GetComponent<RectTransform>();
                }
            }

            yield return new WaitForEndOfFrame();

            if (slotRectTransform)
            {
                float offset = slotRectTransform.offsetMax.y;
                RectTransform cargoRectTransform = cargoUI.GetComponent<RectTransform>();
                cargoRectTransform.anchoredPosition += new Vector2(0f, offset);
                cargoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(cargoSize * 150, 910));
            }
        }
    }

    public void AddToCargo(ItemSO item, int amount)
    {
        if (item == null) return;
        int emptyIndex = levelCargo.FindIndex(slot => slot.item == null);

        if (emptyIndex == -1)
        {
            Debug.Log("<color=red>Cargo is full! Cannot add more items.</color>");
            return;
        }

        cargoSlots[emptyIndex].Setup(item, amount);
        cargoSlots[emptyIndex].itemIcon.gameObject.SetActive(true);
        levelCargo[emptyIndex] = new ItemAmount { item = item, amount = amount };
        Debug.Log($"<color=green>Added {item.displayName} to Cargo Slot {emptyIndex}.</color>");
    }

    public int GetItemCount(int index)
    {
        if (index < 0 || index >= cargoSize || levelCargo[index].item == null) return 0;
        return levelCargo[index].amount;
    }

    public bool DiscardCargo(int index)
    {
        if (index < 0 || index >= cargoSize || levelCargo[index].item == null) return false;

        Debug.Log($"Discarded {levelCargo[index].item.displayName} from slot {index}.");
        levelCargo[index] = new ItemAmount { item = null, amount = 0 };

        cargoSlots[index].itemIcon.sprite = null;
        cargoSlots[index].itemIcon.gameObject.SetActive(false);
        cargoSlots[index].amountText.text = "";
        return true;
    }
}