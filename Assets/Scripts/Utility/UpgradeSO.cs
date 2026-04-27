using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Progression/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string id;
    public string upgradeName;

    [Header("Requirements")]
    public UpgradeSO prerequisiteUpgrade;
    public int prerequisiteLevelRequired = 1;

    [Header("Levels / Tiers")]
    public UpgradeTier[] tiers;
}

[System.Serializable]
public class UpgradeTier
{
    [TextArea] public string description;

    public bool requiresPuzzleToUnlock;

    [Tooltip("Syntax or Function Unlocked")]
    public string unlockFeatureString;

    public ItemCost[] costs;
}