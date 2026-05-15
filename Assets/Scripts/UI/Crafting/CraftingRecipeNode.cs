using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeNode : MonoBehaviour
{
    [HideInInspector] public CraftingRecipeSO recipeData;

    [Header("UI References")]
    public Image outputIcon;
    public TextMeshProUGUI recipeNameText;
    public TextMeshProUGUI ownedCountText;
    public Button nodeButton;

    private CraftingUI craftingUI;

    public void Setup(CraftingRecipeSO recipe, CraftingUI uiManager)
    {
        recipeData = recipe;
        craftingUI = uiManager;

        if (recipe.output.item != null)
        {
            outputIcon.sprite = recipe.output.item.icon;
            recipeNameText.text = recipe.output.item.displayName;
        }
        RefreshOwnedCount();

        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(OnClick);
    }

    public void RefreshOwnedCount()
    {
        if (ownedCountText != null && recipeData != null && recipeData.output.item != null)
        {
            int ownedAmount = InventoryManager.instance.GetAmount(recipeData.output.item.itemId);
            ownedCountText.text = "Owned: " + ownedAmount;
        }
    }

    private void OnClick()
    {
        if (craftingUI != null && craftingUI.currentlySelectedNode != this)
        {
            craftingUI.SelectRecipe(this);
        }
    }
}