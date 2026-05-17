using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Scriptable Objects/UpgradeSO")]
public class UpgradeSO : ScriptableObject
{
    public string id;
    public string upgradeName;
    public Sprite icon;

    public HintSO unlockedHint;

    [Header("Requirements")]
    public UpgradeSO[] prerequisiteUpgrades;
    public string prerequisitePuzzle;

    [Header("Levels / Tiers")]
    public UpgradeTier[] tiers;
}

[System.Serializable]
public class UpgradeTier
{
    [TextArea] public string description;

    public ItemAmount[] costs;
}