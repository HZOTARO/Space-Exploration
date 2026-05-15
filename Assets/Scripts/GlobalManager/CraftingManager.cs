using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    public static event Action<CraftingRecipeSO, int> OnCraftingSucceeded;
    public static event Action<CraftingRecipeSO> OnCraftingFailed;

    private Dictionary<string, CraftingRecipeSO> recipeDatabase = new Dictionary<string, CraftingRecipeSO>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRecipes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadRecipes()
    {
        CraftingRecipeSO[] loaded = Resources.LoadAll<CraftingRecipeSO>("Crafting");
        foreach (CraftingRecipeSO recipe in loaded)
        {
            if (!recipeDatabase.ContainsKey(recipe.recipeId))
            {
                recipeDatabase.Add(recipe.recipeId, recipe);
            }
        }
    }

    public CraftingRecipeSO GetRecipe(string id)
    {
        if (recipeDatabase.TryGetValue(id, out CraftingRecipeSO recipe))
        {
            return recipe;
        }
        return null;
    }

    public List<CraftingRecipeSO> GetAllRecipes()
    {
        return recipeDatabase.Values.ToList();
    }

    public bool CanAffordRecipe(CraftingRecipeSO recipe, int multiplier = 1)
    {
        if (recipe == null) return false;

        foreach (ItemAmount cost in recipe.materialsRequired)
        {
            if (InventoryManager.instance.GetAmount(cost.item.itemId) < cost.amount * multiplier)
                return false;
        }
        return true;
    }

    public bool AttemptCraft(CraftingRecipeSO recipe, int multiplier = 1)
    {
        if (recipe == null) return false;

        if (!CanAffordRecipe(recipe, multiplier))
        {
            OnCraftingFailed?.Invoke(recipe);
            return false;
        }

        foreach (ItemAmount cost in recipe.materialsRequired)
        {
            InventoryManager.instance.DeductItem(cost.item.itemId, cost.amount * multiplier);
        }

        InventoryManager.instance.AddItem(recipe.output.item.itemId, recipe.output.amount * multiplier);

        SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);

        OnCraftingSucceeded?.Invoke(recipe, multiplier);

        return true;
    }
}