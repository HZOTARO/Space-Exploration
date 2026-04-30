using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [Header("Recipe List Panel")]
    public Transform recipeListContainer;
    public GameObject recipeNodePrefab;

    [Header("Details Panel")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Output Display")]
    public Image outputItemIcon;
    public TextMeshProUGUI outputAmountText;

    [Header("Cost Display")]
    public Transform costContainer;
    public GameObject costSlotPrefab;

    [Header("Action")]
    public Button craftButton;
    public TextMeshProUGUI craftButtonText;

    private CraftingRecipeNode currentlySelectedNode;

    void Start()
    {
        PopulateRecipeList();
        ClearPanel();
    }

    private void PopulateRecipeList()
    {
        foreach (Transform child in recipeListContainer) Destroy(child.gameObject);

        foreach (CraftingRecipeSO recipe in CraftingManager.instance.GetAllRecipes())
        {
            GameObject newNode = Instantiate(recipeNodePrefab, recipeListContainer);
            CraftingRecipeNode nodeScript = newNode.GetComponent<CraftingRecipeNode>();

            if (nodeScript != null)
            {
                nodeScript.Setup(recipe, this);
            }
        }
    }

    public void SelectRecipe(CraftingRecipeNode node)
    {
        currentlySelectedNode = node;
        CraftingRecipeSO data = node.recipeData;

        titleText.text = data.recipeName;
        descriptionText.text = data.description;

        outputItemIcon.sprite = data.baseOutput.item.icon;
        outputItemIcon.gameObject.SetActive(true);
        outputAmountText.text = "x" + data.baseOutput.amount.ToString();

        foreach (Transform child in costContainer) Destroy(child.gameObject);

        foreach (ItemAmount cost in data.materialsRequired)
        {
            GameObject newSlot = Instantiate(costSlotPrefab, costContainer);
            ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(cost.item, cost.amount);
            }
        }

        bool canAfford = CraftingManager.instance.CanAffordRecipe(data);
        craftButton.interactable = canAfford;

        if (canAfford)
        {
            craftButtonText.text = "START CRAFTING";
        }
        else
        {
            craftButtonText.text = "NOT ENOUGH MATERIALS";
        }

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftButtonClicked);
    }

    private void OnCraftButtonClicked()
    {
        if (currentlySelectedNode != null)
        {
            CraftingManager.instance.AttemptCraft(currentlySelectedNode.recipeData);

            SelectRecipe(currentlySelectedNode);
        }
    }

    private void ClearPanel()
    {
        titleText.text = "Select a Recipe";
        descriptionText.text = "";
        outputItemIcon.gameObject.SetActive(false);
        outputAmountText.text = "";

        foreach (Transform child in costContainer) Destroy(child.gameObject);

        craftButton.interactable = false;
        craftButtonText.text = "---";
    }
}