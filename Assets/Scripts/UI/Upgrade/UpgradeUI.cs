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

    [Header("Main Reference")]
    public GameObject infoPanel;
    public GameObject otherText;

    [Header("Side Panel UI")]
    public TextMeshProUGUI titleText;
    public Image iconImage;
    public TextMeshProUGUI tier;
    public TextMeshProUGUI currentDescriptionText;
    public TextMeshProUGUI upgradeDescriptionText;
    public Button upgradeButton;
    public Image upgradeButtonImage;
    public TextMeshProUGUI upgradeButtonText;

    [Header("Cost Display")]
    public Transform costContainer;
    public GameObject costSlotPrefab;

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
        UpgradeSO upgradeData = node.upgradeData;

        if (infoPanel) infoPanel.SetActive(true);
        if (otherText) otherText.SetActive(false);

        bool unlocked = UpgradeManager.instance.IsUpgradeUnlocked(upgradeData.id);
        int level = UpgradeManager.instance.GetUpgradeLevel(upgradeData.id);
        bool isMaxed = level >= upgradeData.tiers.Length;

        if (titleText) titleText.text = upgradeData.upgradeName;
        if (iconImage) iconImage.sprite = upgradeData.icon;
        if (tier)
        {
            if (isMaxed) tier.text = "<color=green>MAX LEVEL</color>";
            else tier.text = $"Level {level} -> {level + 1}";
        }

        if (!unlocked)
        {
            currentDescriptionText.gameObject.SetActive(false);

            upgradeDescriptionText.gameObject.SetActive(true);
            upgradeDescriptionText.text = "Next Upgrade:\n" + upgradeData.tiers[0].description;
        }
        else if (isMaxed)
        {
            currentDescriptionText.gameObject.SetActive(true);
            currentDescriptionText.text = "Current Upgrade:\n" + upgradeData.tiers[level - 1].description;

            upgradeDescriptionText.gameObject.SetActive(false);
        }
        else
        {
            currentDescriptionText.gameObject.SetActive(true);
            currentDescriptionText.text = "Current Upgrade:\n" + upgradeData.tiers[level - 1].description;
            upgradeDescriptionText.gameObject.SetActive(true);
            upgradeDescriptionText.text = "Next Upgrade:\n" + upgradeData.tiers[level].description;
        }

        bool canAfford = true;
        foreach (Transform child in costContainer) Destroy(child.gameObject);

        if (!isMaxed) 
        { 
            costContainer.gameObject.SetActive(true);
            
            UpgradeTier nextTier = upgradeData.tiers[level];

            foreach (ItemAmount cost in nextTier.costs)
            {
                GameObject newSlot = Instantiate(costSlotPrefab, costContainer);
                ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(cost.item, cost.amount);

                    int playerAmount = InventoryManager.instance.GetAmount(cost.item.itemId);
                    bool hasEnough = playerAmount >= cost.amount;
                    if (!hasEnough) canAfford = false;

                    slotUI.amountText.color = !hasEnough ? Color.red : Color.white;
                }
            }
        }
        else
        {
            costContainer.gameObject.SetActive(false);
        }

        upgradeButton.onClick.RemoveAllListeners();

        if (isMaxed)
        {
            upgradeButtonText.text = "MAXED";
            upgradeButton.interactable = false;
            upgradeButtonImage.color = new Color32(25, 25, 25, 255);
        }
        else if (canAfford)
        {
            upgradeButtonText.text = unlocked ? "UPGRADE" : "UNLOCK";
            upgradeButton.interactable = true;
            upgradeButtonImage.color = Color.white;
            upgradeButton.onClick.AddListener(OnActionClicked);
        }
        else
        {
            upgradeButtonText.text = "INSUFFICIENT MATERIALS";
            upgradeButton.interactable = false;
            upgradeButtonImage.color = new Color32(25, 25, 25, 255);
        }
    }

    private void OnActionClicked()
    {
        if (currentlySelectedNode != null)
        {
            UpgradeManager.instance.AttemptPurchase(currentlySelectedNode.upgradeData);

            RefreshAllNodes();
            SelectNode(currentlySelectedNode);

            if (SaveManager.instance != null)
            {
                SaveManager.instance.UpdateAllUI();
            }
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
        if (infoPanel != null) infoPanel.SetActive(false);
        if (otherText != null) otherText.gameObject.SetActive(true);

        foreach (Transform child in costContainer) Destroy(child.gameObject);

        upgradeButton.interactable = false;
    }
}