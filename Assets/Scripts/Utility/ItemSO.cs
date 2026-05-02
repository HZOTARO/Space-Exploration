using UnityEngine;

public enum ItemCategory { Material, Consumable, KeyItem }

[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;
    public ItemCategory category;

    [TextArea] public string description;
}