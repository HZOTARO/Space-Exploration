using UnityEngine;
using UnityEngine.UI;

public class UpgradeNode : MonoBehaviour
{
    public UpgradeSO upgradeData;

    [Header("UI References")]
    public Image nodeIcon;
    public Image nodeBackground;
    public Button nodeButton;

    [Header("Visual States")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
    public Color availableColor = Color.white;             
    public Color maxedColor = Color.green;                 

    private UpgradeUI treeUI;

    void Start()
    {
        treeUI = FindFirstObjectByType<UpgradeUI>();
        nodeButton.onClick.AddListener(OnClick);
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (upgradeData == null) return;

        int currentLevel = UpgradeManager.instance.GetUpgradeLevel(upgradeData.id);
        bool hasPrereq = UpgradeManager.instance.HasPrerequisite(upgradeData);
        bool isMaxed = currentLevel >= upgradeData.tiers.Length;

        if (isMaxed)
        {
            nodeBackground.color = maxedColor;
            nodeButton.interactable = true;
        }
        else if (hasPrereq)
        {
            nodeBackground.color = availableColor;
            nodeButton.interactable = true;
        }
        else
        {
            nodeBackground.color = lockedColor;
            nodeButton.interactable = false;
        }

        nodeIcon.sprite = upgradeData.icon;
    }

    private void OnClick()
    {
        if (treeUI != null)
        {
            treeUI.SelectNode(this);
        }
    }
}