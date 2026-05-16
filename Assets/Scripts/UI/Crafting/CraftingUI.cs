using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour, IResourceUpdatable
{
    [Header("Recipe List Panel")]
    public Transform recipeListContainer;
    public GameObject recipeNodePrefab;

    [Header("Main Reference")]
    public Transform infoPanel;
    public TextMeshProUGUI otherText;

    [Header("Details Panel Content")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image outputItemIcon;

    [Header("Amount Selection")]
    public Button plusButton;
    public Button minusButton;
    public TMP_InputField amountInputField;
    public TextMeshProUGUI outputAmountText;

    [Header("Cost Display")]
    public Transform costContainer;
    public GameObject costSlotPrefab;

    [Header("Action")]
    public Button craftButton;
    public Image craftButtonImage;
    public TextMeshProUGUI craftButtonText;

    [HideInInspector]
    public CraftingRecipeNode currentlySelectedNode;
    private List<CraftingRecipeNode> recipeNodes = new List<CraftingRecipeNode>();

    private int currentCraftAmount = 1;
    private const int MIN_AMOUNT = 1;
    private const int MAX_AMOUNT = 99;

    void Start()
    {
        if (plusButton != null) plusButton.onClick.AddListener(IncreaseAmount);
        if (minusButton != null) minusButton.onClick.AddListener(DecreaseAmount);

        if (amountInputField != null)
        {
            amountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            amountInputField.onEndEdit.AddListener(OnAmountInputEndEdit);
        }

        PopulateRecipeList();
        ClearPanel();
    }

    private void PopulateRecipeList()
    {
        foreach (Transform child in recipeListContainer) Destroy(child.gameObject);

        recipeNodes.Clear();

        foreach (CraftingRecipeSO recipe in CraftingManager.instance.GetAllRecipes())
        {
            GameObject newNode = Instantiate(recipeNodePrefab, recipeListContainer);
            CraftingRecipeNode nodeScript = newNode.GetComponent<CraftingRecipeNode>();

            if (nodeScript != null)
            {
                nodeScript.Setup(recipe, this);
                recipeNodes.Add(nodeScript);
            }
        }
    }

    public void SelectRecipe(CraftingRecipeNode node)
    {
        currentlySelectedNode = node;
        currentCraftAmount = 1;

        CraftingRecipeSO recipeData = node.recipeData;

        otherText.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(true);

        titleText.text = recipeData.output.item.displayName;
        descriptionText.text = recipeData.output.item.description;
        outputItemIcon.sprite = recipeData.output.item.icon;

        RefreshDetailsPanel();
    }

    private void RefreshDetailsPanel()
    {
        if (currentlySelectedNode == null) return;
        CraftingRecipeSO recipe = currentlySelectedNode.recipeData;

        if (amountInputField != null)
        {
            amountInputField.SetTextWithoutNotify(currentCraftAmount.ToString());
        }

        int totalOutputAmount = recipe.output.amount * currentCraftAmount;
        outputAmountText.text = "x" + totalOutputAmount.ToString();

        foreach (Transform child in costContainer) Destroy(child.gameObject);

        foreach (ItemAmount cost in recipe.materialsRequired)
        {
            GameObject newSlot = Instantiate(costSlotPrefab, costContainer);
            ItemSlotUI slotUI = newSlot.GetComponent<ItemSlotUI>();
            if (slotUI != null)
            {
                int totalCost = cost.amount * currentCraftAmount;
                slotUI.Setup(cost.item, totalCost);

                int playerAmount = InventoryManager.instance.GetAmount(cost.item.itemId);
                bool hasEnough = playerAmount >= totalCost;

                slotUI.amountText.color = !hasEnough ? Color.red : Color.white;
            }
        }

        bool canAfford = CraftingManager.instance.CanAffordRecipe(recipe, currentCraftAmount);

        if (canAfford)
        {
            craftButtonText.text = "Craft";
            craftButton.interactable = true;
            craftButtonImage.color = Color.white;
        }
        else
        {
            craftButtonText.text = "Insufficient Materials";
            craftButton.interactable = false;
            craftButtonImage.color = new Color32(25, 25, 25, 255);
        }
        
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(OnCraftButtonClicked);
    }

    public void IncreaseAmount()
    {
        if (currentCraftAmount < MAX_AMOUNT)
        {
            currentCraftAmount++;
            RefreshDetailsPanel();
        }
    }

    public void DecreaseAmount()
    {
        if (currentCraftAmount > MIN_AMOUNT)
        {
            currentCraftAmount--;
            RefreshDetailsPanel();
        }
    }

    private void OnAmountInputEndEdit(string input)
    {
        if (int.TryParse(input, out int parsedAmount))
        {
            currentCraftAmount = Mathf.Clamp(parsedAmount, MIN_AMOUNT, MAX_AMOUNT);
        }
        else
        {
            currentCraftAmount = MIN_AMOUNT;
        }

        RefreshDetailsPanel();
    }

    private void OnCraftButtonClicked()
    {
        if (currentlySelectedNode != null)
        {
            bool success = CraftingManager.instance.AttemptCraft(currentlySelectedNode.recipeData, currentCraftAmount);

            if (success)
            {
                foreach (CraftingRecipeNode node in recipeNodes)
                {
                    node.RefreshOwnedCount();
                }
            }

            RefreshDetailsPanel();
        }
    }

    public void ClearPanel()
    {
        currentlySelectedNode = null;

        otherText.text = "Please select a Recipe";

        otherText.gameObject.SetActive(true);
        infoPanel.gameObject.SetActive(false);

        foreach (Transform child in costContainer) Destroy(child.gameObject);
    }

    public void UpdateResource(SaveData saveData)
    {
        foreach (CraftingRecipeNode node in recipeNodes)
        {
            if (node != null)
            {
                node.RefreshOwnedCount();
            }
        }

        if (currentlySelectedNode != null)
        {
            RefreshDetailsPanel();
        }
    }

    private void OnEnable()
    {
        if (recipeNodes != null)
        {
            foreach (CraftingRecipeNode node in recipeNodes)
            {
                if (node != null)
                {
                    node.RefreshOwnedCount();
                    node.RefreshLockState();
                }
            }
        }

        if (currentlySelectedNode != null)
        {
            RefreshDetailsPanel();
        }
    }
}