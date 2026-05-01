using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public string recipeId;
    //public string recipeName;
    //[TextArea] public string description;

    public List<ItemAmount> materialsRequired = new List<ItemAmount>();

    public ItemAmount baseOutput;

    public string puzzleSceneName;
}