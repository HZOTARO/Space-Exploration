using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeTreeUI : MonoBehaviour
{
    [Header("Side Panel UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private UpgradeNode currentlySelectedNode;
    private UpgradeNode[] allNodes;

    void Start()
    {
        allNodes = GetComponentsInChildren<UpgradeNode>();
        ClearPanel();
    }

    public void SelectNode(UpgradeNode node)
    {
        currentlySelectedNode = node;
        UpgradeSO data = node.upgradeData;

        titleText.text = data.upgradeName;

        int level = UpgradeManager.instance.GetUpgradeLevel(data.id);
        bool isMaxed = level >= data.tiers.Length;

        if (isMaxed)
        {
            descriptionText.text = data.tiers[data.tiers.Length - 1].description;
            costText.text = "Max Level Reached";

            actionButton.interactable = false;
            actionButtonText.text = "UNLOCKED";
        }
        else
        {
            UpgradeTier nextTier = data.tiers[level];
            descriptionText.text = nextTier.description;

            string costString = "Requires:\n";
            if (nextTier.costs.Length == 0) costString += "- Free\n";
            foreach (ItemAmount cost in nextTier.costs)
            {
                costString += $"- {cost.amount} {cost.item.displayName}\n";
            }
            costText.text = costString;

            bool canAfford = UpgradeManager.instance.CanAffordAndUnlock(data);
            actionButton.interactable = canAfford;

            if (canAfford)
            {
                actionButtonText.text = nextTier.requiresPuzzleToUnlock ? "START PUZZLE" : "BUY UPGRADE";
            }
            else
            {
                actionButtonText.text = "NOT ENOUGH MATERIALS";
            }

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }
    }

    private void OnActionClicked()
    {
        if (currentlySelectedNode != null)
        {
            UpgradeManager.instance.AttemptPurchase(currentlySelectedNode.upgradeData);

            RefreshAllNodes();
            SelectNode(currentlySelectedNode);
        }
    }

    public void RefreshAllNodes()
    {
        if (allNodes == null) return;
        foreach (UpgradeNode node in allNodes)
        {
            node.RefreshVisuals();
        }
    }

    public void ClearPanel()
    {
        titleText.text = "Select an Upgrade";
        descriptionText.text = "";
        costText.text = "";
        actionButton.interactable = false;
        actionButtonText.text = "---";
    }
}