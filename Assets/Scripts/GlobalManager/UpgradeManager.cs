using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    private List<UpgradeSO> allUpgrades = new List<UpgradeSO>();
    private Dictionary<string, UpgradeSaveState> playerUpgrades = new Dictionary<string, UpgradeSaveState>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            UpgradeSO[] loaded = Resources.LoadAll<UpgradeSO>("Upgrades");
            allUpgrades.AddRange(loaded);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadUpgradesFromSave();
    }

    public void LoadUpgradesFromSave()
    {
        playerUpgrades.Clear();

        if (SaveManager.saveData != null && SaveManager.saveData.unlockedUpgrades != null)
        {
            foreach (UpgradeSaveState state in SaveManager.saveData.unlockedUpgrades)
            {
                playerUpgrades[state.id] = state;
            }
        }
    }

    public void SyncToSaveData()
    {
        if (SaveManager.saveData != null)
        {
            SaveManager.saveData.unlockedUpgrades = playerUpgrades.Values.ToList();
        }
    }

    public UpgradeSO GetUpgradeData(string id)
    {
        return allUpgrades.FirstOrDefault(upgrade => upgrade.id == id);
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        if (playerUpgrades.TryGetValue(upgradeId, out var saveState)) return saveState.currentLevel;
        return 0;
    }

    public bool HasFeatureUnlocked(string featureKey)
    {
        foreach (var saveState in playerUpgrades.Values)
        {
            UpgradeSO data = GetUpgradeData(saveState.id);
            if (data != null && saveState.currentLevel > 0)
            {
                for (int i = 0; i < saveState.currentLevel; i++)
                {
                    if (data.tiers[i].unlockFeatureString == featureKey) return true;
                }
            }
        }
        return false;
    }

    public bool HasPrerequisite(UpgradeSO upgrade)
    {
        if (upgrade.prerequisiteUpgrade == null) return true;
        return GetUpgradeLevel(upgrade.prerequisiteUpgrade.id) >= upgrade.prerequisiteLevelRequired;
    }

    public bool CanAffordAndUnlock(UpgradeSO upgrade)
    {
        int nextLevel = GetUpgradeLevel(upgrade.id);

        if (nextLevel >= upgrade.tiers.Length) return false;
        if (!HasPrerequisite(upgrade)) return false;

        UpgradeTier nextTier = upgrade.tiers[nextLevel];
        foreach (ItemCost cost in nextTier.costs)
        {
            if (InventoryManager.instance.GetAmount(cost.item.itemId) < cost.amount) return false;
        }

        return true;
    }

    public void AttemptPurchase(UpgradeSO upgrade)
    {
        if (!CanAffordAndUnlock(upgrade)) return;

        int currentLvl = GetUpgradeLevel(upgrade.id);
        UpgradeTier tierToBuy = upgrade.tiers[currentLvl];

        if (tierToBuy.requiresPuzzleToUnlock)
        {
            Debug.Log($"<color=yellow>Starting puzzle for {upgrade.upgradeName}!</color>");
            PlayerPrefs.SetString("PendingPuzzleUpgrade", upgrade.id);
            PlayerPrefs.Save();

            // UnityEngine.SceneManagement.SceneManager.LoadScene("PuzzleScene");
        }
        else
        {
            TierUpgradeConsumeResource(tierToBuy);
            ApplyUnlock(upgrade.id);
        }
    }

    public void CompletePuzzle()
    {
        string upgradeId = PlayerPrefs.GetString("PendingPuzzleUpgrade", "");
        if (string.IsNullOrEmpty(upgradeId)) return;

        UpgradeSO upgradeData = GetUpgradeData(upgradeId);
        if (upgradeData == null || !CanAffordAndUnlock(upgradeData)) return;

        int currentLvl = GetUpgradeLevel(upgradeId);

        TierUpgradeConsumeResource(upgradeData.tiers[currentLvl]);
        ApplyUnlock(upgradeId);

        PlayerPrefs.DeleteKey("PendingPuzzleUpgrade");
        Debug.Log($"<color=green>Puzzle solved! {upgradeData.upgradeName} unlocked!</color>");
    }

    private void TierUpgradeConsumeResource(UpgradeTier tier)
    {
        foreach (ItemCost cost in tier.costs)
        {
            InventoryManager.instance.DeductItem(cost.item.itemId, cost.amount);
        }
    }

    private void ApplyUnlock(string upgradeId)
    {
        if (!playerUpgrades.ContainsKey(upgradeId))
        {
            playerUpgrades[upgradeId] = new UpgradeSaveState { id = upgradeId, currentLevel = 0 };
        }
        playerUpgrades[upgradeId].currentLevel++;
        SyncToSaveData();
    }
}