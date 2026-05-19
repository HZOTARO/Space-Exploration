using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    public List<UpgradeSO> allUpgrades = new List<UpgradeSO>();
    public Dictionary<string, UpgradeSaveState> playerUpgrades = new Dictionary<string, UpgradeSaveState>();

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

    public bool IsUpgradeUnlocked(string upgradeId)
    {
        return GetUpgradeLevel(upgradeId) > 0;
    }

    public bool HasPrerequisite(UpgradeSO upgrade)
    {
        if (!string.IsNullOrEmpty(upgrade.prerequisitePuzzle))
        {
            if (!SaveManager.saveData.levelCompleted.Contains(upgrade.prerequisitePuzzle))
            {
                return false;
            }
        }

        if (upgrade.prerequisiteUpgrades != null && upgrade.prerequisiteUpgrades.Length > 0)
        {
            foreach (UpgradeSO prereq in upgrade.prerequisiteUpgrades)
            {
                if (!IsUpgradeUnlocked(prereq.id))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool CanAffordAndUnlock(UpgradeSO upgrade)
    {
        int nextLevel = GetUpgradeLevel(upgrade.id);

        if (nextLevel >= upgrade.tiers.Length) return false;
        if (!HasPrerequisite(upgrade)) return false;

        UpgradeTier nextTier = upgrade.tiers[nextLevel];
        foreach (ItemAmount cost in nextTier.costs)
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

        TierUpgradeConsumeResource(tierToBuy);
        ApplyUnlock(upgrade.id);

        if (HintManager.instance != null && upgrade.unlockedHint != null)
        {
            HintManager.instance.UnlockHint(upgrade.unlockedHint, showHint: true, setHasAppeared: false);
        }
    }

    private void TierUpgradeConsumeResource(UpgradeTier tier)
    {
        foreach (ItemAmount cost in tier.costs)
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
        SaveManager.instance.UpdateAllUI();
    }
}