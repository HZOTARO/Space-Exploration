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
            LoadRecipes();
        }
        else Destroy(gameObject);
    }

    private void LoadRecipes()
    {
        // Automatically loads all recipes from a "Resources/Crafting" folder!
        CraftingRecipeSO[] loaded = Resources.LoadAll<CraftingRecipeSO>("Crafting");
        allRecipes.AddRange(loaded);
    }

    public CraftingRecipeSO GetRecipe(string id)
    {
        return allRecipes.FirstOrDefault(r => r.recipeId == id);
    }

    // 1. Check if they have the materials
    public bool CanAffordRecipe(CraftingRecipeSO recipe)
    {
        // FIXED: Uses ItemCost and asks the InventoryManager!
        foreach (ItemCost cost in recipe.materialsRequired)
        {
            if (InventoryManager.instance.GetAmount(cost.item.itemId) < cost.amount)
                return false;
        }
        return true;
    }

    // 2. Start the Crafting Process
    public void AttemptCraft(CraftingRecipeSO recipe)
    {
        if (!CanAffordRecipe(recipe)) return;

        Debug.Log($"Starting crafting puzzle for {recipe.recipeName}");

        // Save what we are trying to craft
        PlayerPrefs.SetString("PendingCraftingRecipe", recipe.recipeId);
        PlayerPrefs.Save();

        // TODO: Load the puzzle scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene(recipe.puzzleSceneName);
    }

    // 3. Complete the Crafting (Called from your Puzzle Scene when they win)
    // The "scoreMultiplier" is passed in by your puzzle logic (e.g., 1.5x for completing in 3 lines of code)
    public void CompleteCraftingPuzzle(float scoreMultiplier)
    {
        string recipeId = PlayerPrefs.GetString("PendingCraftingRecipe", "");
        if (string.IsNullOrEmpty(recipeId)) return;

        CraftingRecipeSO recipe = GetRecipe(recipeId);
        if (recipe == null || !CanAffordRecipe(recipe)) return;

        // A. Deduct the materials
        foreach (ItemCost cost in recipe.materialsRequired)
        {
            // FIXED: Tell InventoryManager to deduct the items
            InventoryManager.instance.DeductItem(cost.item.itemId, cost.amount);
        }

        // B. Calculate the multiplied output! (Use Mathf.FloorToInt or CeilToInt to handle decimals)
        int finalYield = Mathf.FloorToInt(recipe.baseOutput.amount * scoreMultiplier);

        // Ensure they get at least 1, even if they had a terrible score multiplier like 0.1x
        if (finalYield < 1) finalYield = 1;

        // C. Give the player the items
        // FIXED: Add to InventoryManager!
        InventoryManager.instance.AddItem(recipe.baseOutput.item.itemId, finalYield);

        // D. Clear memory
        PlayerPrefs.DeleteKey("PendingCraftingRecipe");

        // FIXED: Prints the dynamic Item name
        Debug.Log($"<color=green>Crafted successfully! Score Multiplier: {scoreMultiplier}x. Received {finalYield} {recipe.baseOutput.item.displayName}!</color>");

        // Optional: Save the game automatically after crafting
        // SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
        // SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
    }
}