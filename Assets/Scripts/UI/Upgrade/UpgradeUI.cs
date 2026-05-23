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
    public Transform iconContainer;
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
    private PuzzleNode currentlySelectedPuzzleNode;
    private UpgradeNode[] allNodes;
    private PuzzleNode[] allPuzzleNodes;

    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(index));
            tabs[i].tabButton.image.sprite = defaultTabSprite;
        }

        allNodes = GetComponentsInChildren<UpgradeNode>(true);
        allPuzzleNodes = GetComponentsInChildren<PuzzleNode>(true);

        if (tabs.Length > 0)
        {
            SwitchToTab(0);
        }
        else
        {
            ClearPanel();
        }
    }

    public void OnOpened()
    {
        RefreshAllNodes();

        if (tabs.Length > 0) SwitchToTab(0);
        else ClearPanel();
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
        currentlySelectedPuzzleNode = null;
        currentlySelectedNode = node;
        UpgradeSO upgradeData = node.upgradeData;

        if (infoPanel) infoPanel.SetActive(true);
        if (otherText) otherText.SetActive(false);
        if (iconContainer) iconContainer.gameObject.SetActive(true);
        if (iconImage) iconImage.sprite = upgradeData.icon;

        if (tier) tier.gameObject.SetActive(true);
        if (upgradeDescriptionText) upgradeDescriptionText.gameObject.SetActive(true);
        if (currentDescriptionText) currentDescriptionText.gameObject.SetActive(true);

        bool unlocked = UpgradeManager.instance.IsUpgradeUnlocked(upgradeData.id);
        int level = UpgradeManager.instance.GetUpgradeLevel(upgradeData.id);
        bool isMaxed = level >= upgradeData.tiers.Length;
        bool hasPrereqs = UpgradeManager.instance.HasPrerequisite(upgradeData);

        if (titleText) titleText.text = upgradeData.upgradeName;

        if (tier)
        {
            if (!hasPrereqs) tier.text = "<color=#5D5D5D>LOCKED</color>";
            else if (isMaxed) tier.text = "<color=green>MAX LEVEL</color>";
            else tier.text = $"Level {level} -> {level + 1}";
        }

        if (!unlocked)
        {
            currentDescriptionText.text = "";

            if (!hasPrereqs)
            {
                currentDescriptionText.gameObject.SetActive(true);
                string reqString = "";

                if (upgradeData.prerequisitePuzzles != null && upgradeData.prerequisitePuzzles.Length > 0)
                {
                    foreach (PuzzleSO prereqPuzzle in upgradeData.prerequisitePuzzles)
                    {
                        if (prereqPuzzle == null) continue;
                        bool hasPuzzle = SaveManager.saveData.levelCompleted.Contains(prereqPuzzle.id);
                        string color = hasPuzzle ? "green" : "red";
                        reqString += $"<color={color}>- Require Level '{prereqPuzzle.puzzleName}' completed</color>\n";
                    }
                }

                if (upgradeData.prerequisiteUpgrades != null && upgradeData.prerequisiteUpgrades.Length > 0)
                {
                    foreach (UpgradeSO prereq in upgradeData.prerequisiteUpgrades)
                    {
                        if (prereq == null) continue;
                        bool hasUp = UpgradeManager.instance.IsUpgradeUnlocked(prereq.id);
                        string color = hasUp ? "green" : "red";
                        reqString += $"<color={color}>- Require '{prereq.upgradeName}' unlocked</color>\n";
                    }
                }
                currentDescriptionText.text = "Requirements:\n" + reqString;
            }
            upgradeDescriptionText.text = "Next Upgrade:\n" + upgradeData.tiers[0].description;
        }
        else if (isMaxed)
        {
            currentDescriptionText.text = "Current Upgrade:\n" + upgradeData.tiers[level - 1].description;
            upgradeDescriptionText.text = "";
        }
        else
        {
            currentDescriptionText.text = "Current Upgrade:\n" + upgradeData.tiers[level - 1].description;
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
        else if (!hasPrereqs)
        {
            upgradeButtonText.text = "REQUIREMENTS NOT MET";
            upgradeButton.interactable = false;
            upgradeButtonImage.color = new Color32(25, 25, 25, 255);
        }
        else if (canAfford)
        {
            upgradeButtonText.text = unlocked ? "UPGRADE" : "UNLOCK";
            upgradeButton.interactable = true;
            upgradeButtonImage.color = Color.white;
            upgradeButton.onClick.AddListener(OnUpgradeActionClicked);
        }
        else
        {
            upgradeButtonText.text = "INSUFFICIENT MATERIALS";
            upgradeButton.interactable = false;
            upgradeButtonImage.color = new Color32(25, 25, 25, 255);
        }
    }

    private void OnUpgradeActionClicked()
    {
        if (currentlySelectedNode != null)
        {
            UpgradeManager.instance.AttemptPurchase(currentlySelectedNode.upgradeData);
            RefreshAllNodes();
            SelectNode(currentlySelectedNode);

            if (SaveManager.instance != null) SaveManager.instance.UpdateAllUI();
        }
    }

    public void SelectNode(PuzzleNode node)
    {
        currentlySelectedNode = null;
        currentlySelectedPuzzleNode = node;
        PuzzleSO puzzleData = node.puzzleData;

        if (infoPanel) infoPanel.SetActive(true);
        if (otherText) otherText.SetActive(false);
        if (iconContainer) iconContainer.gameObject.SetActive(false);

        if (upgradeDescriptionText) upgradeDescriptionText.gameObject.SetActive(false);
        if (costContainer) costContainer.gameObject.SetActive(false);

        bool isCompleted = SaveManager.saveData.levelCompleted.Contains(puzzleData.id);
        bool hasPrereqs = UpgradeManager.instance.HasPrerequisite(puzzleData);

        if (titleText) titleText.text = puzzleData.puzzleName;

        if (tier)
        {
            tier.text = isCompleted ? "<color=green>COMPLETE</color>" : (hasPrereqs ? "AVAILABLE" : "<color=#5D5D5D>LOCKED</color>");
        }

        if (!hasPrereqs)
        {
            if (currentDescriptionText) currentDescriptionText.gameObject.SetActive(true);
            string reqString = "";

            if (puzzleData.prerequisitePuzzles != null && puzzleData.prerequisitePuzzles.Length > 0)
            {
                foreach (PuzzleSO prereqPuzzle in puzzleData.prerequisitePuzzles)
                {
                    if (prereqPuzzle == null) continue;
                    bool hasPuzzle = SaveManager.saveData.levelCompleted.Contains(prereqPuzzle.id);
                    string color = hasPuzzle ? "green" : "red";
                    reqString += $"<color={color}>- Require Level '{prereqPuzzle.puzzleName}' completed</color>\n";
                }
            }

            if (puzzleData.prerequisiteUpgrades != null && puzzleData.prerequisiteUpgrades.Length > 0)
            {
                foreach (UpgradeSO prereq in puzzleData.prerequisiteUpgrades)
                {
                    if (prereq == null) continue;
                    bool hasUp = UpgradeManager.instance.IsUpgradeUnlocked(prereq.id);
                    string color = hasUp ? "green" : "red";
                    reqString += $"<color={color}>- Require '{prereq.upgradeName}' unlocked</color>\n";
                }
            }
            if (currentDescriptionText) currentDescriptionText.text = "Requirements:\n" + reqString;
        }
        else
        {
            if (currentDescriptionText) currentDescriptionText.gameObject.SetActive(false);
        }

        upgradeButton.onClick.RemoveAllListeners();

        if (!hasPrereqs)
        {
            upgradeButtonText.text = "LOCKED";
            upgradeButton.interactable = false;
            upgradeButtonImage.color = new Color32(25, 25, 25, 255);
        }
        else
        {
            upgradeButtonText.text = isCompleted ? "REATTEMPT" : "ATTEMPT";
            upgradeButton.interactable = true;
            upgradeButtonImage.color = Color.white;
            upgradeButton.onClick.AddListener(OnPuzzleActionClicked);
        }
    }

    private void OnPuzzleActionClicked()
    {
        if (currentlySelectedPuzzleNode != null)
        {
            PuzzleSO puzzle = currentlySelectedPuzzleNode.puzzleData;
            Debug.Log($"Attempting Puzzle: {puzzle.id}");

            PlayerPrefs.SetString("PuzzleID", puzzle.id);
            PlayerPrefs.SetInt("PuzzleSize", puzzle.levelSize);

            PlayerPrefs.Save();

            if (LevelManager.instance != null)
            {
                LevelManager.instance.OpenScene(LevelType.UpgradeLevel);
            }
        }
    }

    public void RefreshAllNodes()
    {
        if (allNodes != null)
        {
            foreach (UpgradeNode node in allNodes)
                if (node != null) node.RefreshVisuals();
        }

        if (allPuzzleNodes != null)
        {
            foreach (PuzzleNode pNode in allPuzzleNodes)
                if (pNode != null) pNode.RefreshVisuals();
        }
    }

    public void ClearPanel()
    {
        currentlySelectedNode = null;
        currentlySelectedPuzzleNode = null;

        if (infoPanel != null) infoPanel.SetActive(false);
        if (otherText != null) otherText.gameObject.SetActive(true);

        foreach (Transform child in costContainer) Destroy(child.gameObject);

        upgradeButton.interactable = false;
    }
}