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
            whiteOreText.text = SaveManager.saveData.whiteOre.ToString();
        if (purpleLiquidText != null)
            purpleLiquidText.text = SaveManager.saveData.purpleLiquid.ToString();
        if (blackOreText != null)
            blackOreText.text = SaveManager.saveData.blackOre.ToString();
        if (partsAText != null)
            partsAText.text = SaveManager.saveData.partsA.ToString();
        if (partsBText != null)
            partsBText.text = SaveManager.saveData.partsB.ToString();
        if (partCText != null)
            partCText.text = SaveManager.saveData.partsC.ToString();
    }
}
