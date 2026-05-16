using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNode : MonoBehaviour
{
    public UpgradeSO upgradeData;

    [Header("UI References")]
    public Button nodeButton;
    public Image nodeBackground;
    public Image nodeIcon;
    public TextMeshProUGUI nodeLabel;
    public TextMeshProUGUI nodeLevelText;

    [Header("Visual States")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
    public Color availableColor = Color.white;
    public Color maxedColor = Color.green;

    private UpgradeUI upgradeUI;

    void Start()
    {
        upgradeUI = FindFirstObjectByType<UpgradeUI>();
        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(OnClick);
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (upgradeData == null) return;

        nodeIcon.sprite = upgradeData.icon;
        nodeLabel.text = upgradeData.upgradeName;

        int currentLevel = UpgradeManager.instance.GetUpgradeLevel(upgradeData.id);
        bool hasPrereq = UpgradeManager.instance.HasPrerequisite(upgradeData);

        int maxLevel = upgradeData.tiers.Length;
        bool isMaxed = currentLevel >= maxLevel;

        if (isMaxed)
        {
            nodeLevelText.text = "MAX";

            SetColor(maxedColor);
        }
        else if (hasPrereq)
        {
            nodeLevelText.text = currentLevel + " / " + maxLevel;

            SetColor(availableColor);
        }
        else
        {
            nodeLevelText.text = "LOCKED";
            SetColor(lockedColor);
        }
    }

    private void OnClick()
    {
        if (upgradeUI != null)
        {
            upgradeUI.SelectNode(this);
        }
    }

    private void SetColor(Color color)
    {
        nodeBackground.color = color;
        nodeLevelText.color = color;
        nodeIcon.color = color;
        nodeLabel.color = color;
    }
}