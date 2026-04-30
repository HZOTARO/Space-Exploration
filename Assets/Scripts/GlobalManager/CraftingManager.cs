using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager instance;

    private List<CraftingRecipeSO> allRecipes = new List<CraftingRecipeSO>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            CraftingRecipeSO[] loaded = Resources.LoadAll<CraftingRecipeSO>("Crafting");
            allRecipes.AddRange(loaded);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CraftingRecipeSO GetRecipe(string id)
    {
        return allRecipes.FirstOrDefault(recipe => recipe.recipeId == id);
    }

    public List<CraftingRecipeSO> GetAllRecipes()
    {
        return allRecipes;
    }

    public bool CanAffordRecipe(CraftingRecipeSO recipe)
    {
        foreach (ItemAmount cost in recipe.materialsRequired)
        {
            if (InventoryManager.instance.GetAmount(cost.item.itemId) < cost.amount)
                return false;
        }
        return true;
    }

    public void AttemptCraft(CraftingRecipeSO recipe)
    {
        if (!CanAffordRecipe(recipe)) return;

        Debug.Log($"Starting crafting level for {recipe.recipeName}");

        PlayerPrefs.SetString("PendingCraftingRecipe", recipe.recipeId);
        PlayerPrefs.Save();

        // UnityEngine.SceneManagement.SceneManager.LoadScene(recipe.craftingScene);
    }

    public void CompleteCraftingPuzzle(float scoreMultiplier)
    {
        string recipeId = PlayerPrefs.GetString("PendingCraftingRecipe", "");
        if (string.IsNullOrEmpty(recipeId)) return;

        CraftingRecipeSO recipe = GetRecipe(recipeId);
        if (recipe == null || !CanAffordRecipe(recipe)) return;

        foreach (ItemAmount cost in recipe.materialsRequired)
        {
            InventoryManager.instance.DeductItem(cost.item.itemId, cost.amount);
        }

        int finalYield = Mathf.FloorToInt(recipe.baseOutput.amount * scoreMultiplier);

        if (finalYield < 1) finalYield = 1;

        InventoryManager.instance.AddItem(recipe.baseOutput.item.itemId, finalYield);

        PlayerPrefs.DeleteKey("PendingCraftingRecipe");

        Debug.Log($"<color=green>Crafted successfully! Score Multiplier: {scoreMultiplier}x. Received {finalYield} {recipe.baseOutput.item.displayName}!</color>");

        // SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
        // SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
    }
}