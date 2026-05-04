using UnityEngine;

public class ValueTile : BaseTile, IMeasureable
{
    [Header("Item Setting")]
    public ItemSO itemOnTile;

    [Header("Value Settings")]
    public bool notRandomized;
    public int value;

    [Header("Upgrade Setting")]
    public UpgradeSO upgrade;
    public Vector2Int[] upgradeValues;

    protected virtual void Start()
    {
        DetermineValue();
    }
    protected virtual void DetermineValue()
    {
        if (notRandomized) return;

        int upgradeTier = 1;
        if (upgrade)
        {
            upgradeTier = UpgradeManager.instance.GetUpgradeLevel(upgrade.id);
            upgradeTier = CalculateUpgradeIndex(upgradeTier);
        }

        if (upgradeValues != null && upgradeValues.Length > 0)
        {
            int arrayIndex = Mathf.Clamp(upgradeTier - 1, 0, upgradeValues.Length - 1);

            Vector2Int range = upgradeValues[arrayIndex];

            value = Random.Range(range.x, range.y + 1);
        }
    }
    protected virtual int CalculateUpgradeIndex(int upgradeTier)
    {
        return upgradeTier;
    }
    int IMeasureable.Measured()
    {
        return value;
    }

    public virtual int Collect()
    {
        return value;
    }
}
