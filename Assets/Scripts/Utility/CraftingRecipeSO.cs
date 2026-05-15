using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Scriptable Objects/RecipeSO")]
public class CraftingRecipeSO : ScriptableObject
{
    public string recipeId;
    public UpgradeSO requiredUpgrade;

    public List<ItemAmount> materialsRequired = new List<ItemAmount>();
    public ItemAmount output = new ItemAmount();
}