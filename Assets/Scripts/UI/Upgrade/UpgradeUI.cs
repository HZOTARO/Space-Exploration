using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct TabPage
{
    public Button tabButton;
    public GameObject pagePanel;
    public ScrollRect scrollView;
}

public class UpgradeUI : MonoBehaviour
{
    [Header("Tabs")]
    public TabPage[] tabs;
    public Sprite defaultTabSprite;
    public Sprite selectedTabSprite;

    [Header("Side Panel UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    public Button upgradeButton;
    public TextMeshProUGUI upgradeButtonText;

    [Header("Nodes")]
    private UpgradeNode currentlySelectedNode;
    private UpgradeNode[] allNodes;

    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(index));
            tabs[i].tabButton.image.sprite = defaultTabSprite;
        }

        if (tabs.Length > 0)
        {
            SwitchToTab(0);
        }

        allNodes = GetComponentsInChildren<UpgradeNode>(true);
        ClearPanel();
    }

    public void OnOpened()
    {
        RefreshAllNodes();

        if (tabs.Length > 0)
        {
            SwitchToTab(0);
        }
        else
        {
            ClearPanel();
        }
    }

    public void SwitchToTab(int tabIndex)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == tabIndex);
            tabs[i].pagePanel.SetActive(isActive);
            tabs[i].tabButton.interactable = !isActive;
            tabs[i].tabButton.image.sprite = isActive ? selectedTabSprite : defaultTabSprite;

            if (isActive && tabs[i].scrollView != null)
            {
                tabs[i].scrollView.normalizedPosition = new Vector2(0f, 1f);
            }
        }

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

            upgradeButton.interactable = false;
            upgradeButtonText.text = "UNLOCKED";
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
            upgradeButton.interactable = canAfford;

            if (!canAfford)
            {
                upgradeButtonText.text = "NOT ENOUGH MATERIALS";
            }

            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnActionClicked);
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
            if (node != null) node.RefreshVisuals();
        }
    }

    public void ClearPanel()
    {
        currentlySelectedNode = null;
        titleText.text = "Select an Upgrade";
        descriptionText.text = "";
        costText.text = "";
        upgradeButton.interactable = false;
        upgradeButtonText.text = "---";
    }
}