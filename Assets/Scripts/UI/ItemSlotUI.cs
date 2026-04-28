using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public void Setup(ItemSO itemData, int amount)
    {
        itemIcon.sprite = itemData.icon;
        amountText.text = amount.ToString();
    }
}