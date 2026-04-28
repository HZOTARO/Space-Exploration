using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeNode : MonoBehaviour
{
    [HideInInspector] public CraftingRecipeSO recipeData;

    [Header("UI References")]
    public Image outputIcon;
    public TextMeshProUGUI recipeNameText;
    public Button nodeButton;

    private CraftingUI craftingUI;

    public void Setup(CraftingRecipeSO recipe, CraftingUI uiManager)
    {
        recipeData = recipe;
        craftingUI = uiManager;

        // Automatically grab the output item's name and icon!
        if (recipe.baseOutput.item != null)
        {
            outputIcon.sprite = recipe.baseOutput.item.icon;
            recipeNameText.text = recipe.recipeName;
        }

        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (craftingUI != null)
        {
            craftingUI.SelectRecipe(this);
        }
    }
}