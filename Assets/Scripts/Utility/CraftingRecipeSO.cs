using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public string recipeId;
    public string recipeName;
    [TextArea] public string description;

    public List<ItemCost> materialsRequired = new List<ItemCost>();

    public ItemCost baseOutput;

    public string puzzleSceneName;
}