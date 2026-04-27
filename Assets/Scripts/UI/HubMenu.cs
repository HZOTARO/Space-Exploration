using TMPro;
using UnityEngine;

public class HubMenu : MonoBehaviour, IResourceUpdatable
{
    [Header("Resources Text Reference")]
    public TextMeshProUGUI whiteOreText;
    public TextMeshProUGUI purpleLiquidText;
    public TextMeshProUGUI blackOreText;
    public TextMeshProUGUI partsAText;
    public TextMeshProUGUI partsBText;
    public TextMeshProUGUI partCText;

    void Start()
    {
        UpdateResource(SaveManager.saveData);
    }

    public void UpdateResource(SaveData saveData)
    {
        if (whiteOreText != null)
            whiteOreText.text = InventoryManager.instance.GetAmount("white_ore").ToString();

        if (purpleLiquidText != null)
            purpleLiquidText.text = InventoryManager.instance.GetAmount("purple_liquid").ToString();

        if (blackOreText != null)
            blackOreText.text = InventoryManager.instance.GetAmount("black_ore").ToString();

        if (partsAText != null)
            partsAText.text = InventoryManager.instance.GetAmount("parts_a").ToString();

        if (partsBText != null)
            partsBText.text = InventoryManager.instance.GetAmount("parts_b").ToString();

        if (partCText != null)
            partCText.text = InventoryManager.instance.GetAmount("parts_c").ToString();
    }
}