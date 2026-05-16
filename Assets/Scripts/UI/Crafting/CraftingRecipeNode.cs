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

    public GameObject lockedOverlay;
    public TextMeshProUGUI lockedText;

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
        RefreshLockState();

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

    public void RefreshLockState()
    {
        if (recipeData == null) return;

        if (recipeData.requiredUpgrade != null && UpgradeManager.instance != null)
        {
            bool isUnlocked = UpgradeManager.instance.IsUpgradeUnlocked(recipeData.requiredUpgrade.id);

            if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);
            if (nodeButton != null) nodeButton.interactable = isUnlocked;

            if (!isUnlocked && lockedText != null)
            {
                lockedText.text = $"Requires {recipeData.requiredUpgrade.upgradeName} Upgrade to Craft";
            }
        }
        else
        {
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (nodeButton != null) nodeButton.interactable = true;
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