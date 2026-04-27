using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Side Panel UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private SkillTreeNode currentlySelectedNode;
    private SkillTreeNode[] allNodes;

    void Start()
    {
        allNodes = GetComponentsInChildren<SkillTreeNode>();
        ClearPanel();
    }

    public void SelectNode(SkillTreeNode node)
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
            foreach (ItemCost cost in nextTier.costs)
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
        foreach (SkillTreeNode node in allNodes)
        {
            node.RefreshVisuals();
        }
    }

    private void ClearPanel()
    {
        titleText.text = "Select an Upgrade";
        descriptionText.text = "";
        costText.text = "";
        actionButton.interactable = false;
        actionButtonText.text = "---";
    }
}