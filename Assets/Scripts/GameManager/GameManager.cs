using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Camera")]
    public CameraController cameraController;

    [Header("Player")]
    public Player player;
    public Image playerHealthBar;
    public TextMeshProUGUI playerHealthText;
    protected int playerMaxHealth = 100;
    protected int playerHealth;
    // 0 = North(N), 1 = East(E), 2 = South(S), 3 = West(W)
    public int playerFacing = 0;

    [Header("Tile")]
    protected int levelSize;
    protected TileManager tileManager;
    Vector2Int playerGridLoc = new();

    [Header("Inventory")]
    [Range(1, 15)]
    public int inventorySize = 6;
    public Transform inventoryUI;
    protected List<ItemAmount> levelInventory = new List<ItemAmount>();
    protected List<ItemSlotUI> inventorySlots = new List<ItemSlotUI>();

    [Header("Consumables (Shuffled)")]
    public List<ItemSO> availableConsumables = new List<ItemSO>();
    protected List<ItemSO> shuffledConsumables = new List<ItemSO>();

    [HideInInspector] public List<string> allowedSyntaxNodes = new List<string>();
    [HideInInspector] public List<string> allowedFunctions = new List<string>();

    void Awake()
    {
        allowedSyntaxNodes = new List<string>();
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Core);

        allowedFunctions = new List<string>();
        allowedFunctions.AddRange(FunctionDictionary.Core);

        SetLevelAllowedSyntax();
    }

    protected virtual void SetLevelAllowedSyntax()
    {
        if (UpgradeManager.instance != null)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("variable"))
            {
                allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
            }

            if (UpgradeManager.instance.IsUpgradeUnlocked("ifelse"))
            {
                allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
            }

            if (UpgradeManager.instance.IsUpgradeUnlocked("loop"))
            {
                allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
            }

            if (UpgradeManager.instance.IsUpgradeUnlocked("list"))
            {
                allowedSyntaxNodes.AddRange(SyntaxDictionary.Lists);
            }
        }
    }

    protected virtual void Start()
    {
        playerHealth = playerMaxHealth;
        UpdateHealth();

        if (!player) player = FindFirstObjectByType<Player>();
        if (!tileManager) tileManager = FindFirstObjectByType<TileManager>();

        SetupMap();

        if (!cameraController) cameraController = FindFirstObjectByType<CameraController>();
        if (cameraController)
        {
            cameraController.gridHeight = levelSize;
            cameraController.gridWidth = levelSize;
            cameraController.Initialize();
        }

        StartCoroutine(SetupInventory());
        PythonExecutor.instance.InitializePythonAllowed(allowedSyntaxNodes.ToArray(), allowedFunctions.ToArray());
        RegisterPythonCommands();

        shuffledConsumables = new List<ItemSO>(availableConsumables);

        for (int i = 0; i < shuffledConsumables.Count; i++)
        {
            ItemSO temp = shuffledConsumables[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffledConsumables.Count);

            shuffledConsumables[i] = shuffledConsumables[randomIndex];
            shuffledConsumables[randomIndex] = temp;
        }

        string debugResult = "Shuffled Order: ";
        for (int i = 0; i < shuffledConsumables.Count; i++)
        {
            debugResult += shuffledConsumables[i].itemId + " | ";
        }
        Debug.Log($"<color=cyan>{debugResult}</color>");

        PythonExecutor.instance.CanStepCode = () => !InAction();
        PythonExecutor.instance.OnPythonPrint += PrintToDisplay;
    }

    protected virtual void SetupMap()
    {
        if (!tileManager) return;
        
        tileManager.width = levelSize;
        tileManager.length = levelSize;
        tileManager.GenerateMap();
    }

    private void RegisterPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);
        void BindReturn<TResult>(string pyName, Func<TResult> func) => PythonExecutor.instance.RegisterPythonFunction(pyName, func);
        void BindWithArg<TArg, TResult>(string pyName, Func<TArg, TResult> func) => PythonExecutor.instance.RegisterPythonFunction(pyName, func);

        Bind("move_forward", MoveForward);
        Bind("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
        Bind("go_back", Return);

        BindReturn("scan", Scan);
        BindWithArg<int, object>("use_item", UseItem);
        BindWithArg<int, string>("inspect_item", InspectItem);
        BindReturn<int>("item_type_count", GetConsumablesSize);
        BindWithArg<int, int>("item_count", GetItemCount);
        BindWithArg<int, bool>("discard_inventory", DiscardInventory);

        RegisterLevelSpecificPythonCommands();

        CodeEditor editor = FindFirstObjectByType<CodeEditor>();
        if (editor != null)
        {
            editor.InitializeSyntaxGroups();
        }
    }

    protected virtual void RegisterLevelSpecificPythonCommands() { }

    IEnumerator SetupInventory()
    {
        if (inventoryUI)
        {
            Transform template = null;
            foreach (Transform slotTransform in inventoryUI.transform)
            {
                if (template)
                {
                    Destroy(slotTransform);
                    continue;
                }

                ItemSlotUI itemSlot = slotTransform.GetComponentInChildren<ItemSlotUI>();

                if (itemSlot)
                {
                    template = slotTransform;
                    template.gameObject.SetActive(false);
                }
            }

            if (template)
            {
                RectTransform slotRectTransform = null;
                for (int i = 0; i < inventorySize; i++)
                {
                    Transform newSlot = Instantiate(template, inventoryUI, false);
                    newSlot.gameObject.SetActive(true);
                    newSlot.name = $"Slot ({i + 1})";

                    ItemSlotUI newItemSlot = newSlot.GetComponentInChildren<ItemSlotUI>();

                    inventorySlots.Add(newItemSlot);
                    levelInventory.Add(new ItemAmount { item = null, amount = 0 });

                    inventorySlots[i].itemIcon.sprite = null;
                    inventorySlots[i].itemIcon.gameObject.SetActive(false);
                    inventorySlots[i].amountText.text = "";
                    inventorySlots[i].amountText.fontSize = 30 + 6 * ((10 - Mathf.Min(Mathf.Max(inventorySize, 6), 10)) / (10 - 6));

                    if (i == inventorySize - 1)
                    {
                        Transform child = newSlot.GetChild(0);
                        slotRectTransform = child.GetComponent<RectTransform>();
                    }
                }
                yield return new WaitForEndOfFrame();
                if (slotRectTransform)
                {
                    float offset = slotRectTransform.offsetMax.y;
                    RectTransform inventoryRectTransform = inventoryUI.GetComponent<RectTransform>();
                    inventoryRectTransform.anchoredPosition += new Vector2(0f, offset);
                    inventoryRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(inventorySize * 150, 910));
                }
            }
            else
            {
                inventoryUI = null;
                Debug.LogWarning("Inventory is not valid because it was missing an Image or Text!");
            }
        }
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnPythonPrint -= PrintToDisplay;
        }
    }

    public void PrintToDisplay(string message)
    {
        Debug.Log(message);

        if (player != null)
        {
            PlayerFloatingText pft = player.GetComponent<PlayerFloatingText>();
            if (pft != null) pft.ShowText(message);
        }
    }

    /// <summary>
    /// In Animation or Moving
    /// </summary>
    public bool InAction()
    {
        return player != null && player.inAction;
    }
    public TileObject GetCurrentTile()
    {
        return tileManager.objectsArray[playerGridLoc.y, playerGridLoc.x];
    }

    public Vector2Int GetForwardGridLoc()
    {
        int targetX = playerGridLoc.x;
        int targetY = playerGridLoc.y;

        if (playerFacing == 0) targetY++;
        else if (playerFacing == 1) targetX++;
        else if (playerFacing == 2) targetY--;
        else if (playerFacing == 3) targetX--;

        return new Vector2Int(targetX, targetY);
    }

    public TileObject GetTileInFront()
    {
        Vector2Int forwardLoc = GetForwardGridLoc();

        if (forwardLoc.x >= 0 && forwardLoc.x < tileManager.width && forwardLoc.y >= 0 && forwardLoc.y < tileManager.length)
        {
            return tileManager.objectsArray[forwardLoc.y, forwardLoc.x];
        }

        return null;
    }

    private bool IsTileWalkable(int x, int y)
    {
        TileObject targetTile = tileManager.objectsArray[y, x];

        if (targetTile.type == TileType.Floor)
        {
            return true;
        }

        if (targetTile.tileInstance != null)
        {
            return targetTile.tileInstance.isWalkable;
        }

        return false;
    }

    public void MoveForward()
    {
        int targetX = playerGridLoc.x;
        int targetY = playerGridLoc.y;

        if (playerFacing == 0) targetY++;
        else if (playerFacing == 1) targetX++;
        else if (playerFacing == 2) targetY--;
        else if (playerFacing == 3) targetX--;

        if (targetX < 0 || targetX >= tileManager.width || targetY < 0 || targetY >= tileManager.length) return;
        if (!IsTileWalkable(targetX, targetY)) return;

        playerGridLoc.x = targetX;
        playerGridLoc.y = targetY;

        player.Move(Direction.Forward);
    }

    public void MoveBackward()
    {
        int targetX = playerGridLoc.x;
        int targetY = playerGridLoc.y;

        if (playerFacing == 0) targetY--;
        else if (playerFacing == 1) targetX--;
        else if (playerFacing == 2) targetY++;
        else if (playerFacing == 3) targetX++;

        if (targetX < 0 || targetX >= tileManager.width || targetY < 0 || targetY >= tileManager.length) return;
        if (!IsTileWalkable(targetX, targetY)) return;

        playerGridLoc.x = targetX;
        playerGridLoc.y = targetY;

        player.Move(Direction.Backward);
    }

    public void TurnRight()
    {
        playerFacing = (playerFacing + 1) % 4;
        player.Turn(90f);
    }

    public void TurnLeft()
    {
        playerFacing = (playerFacing + 3) % 4;
        player.Turn(-90f);
    }

    public string Scan()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            return "Empty";
        }

        string tileTypeName = targetTile.type.ToString();

        Debug.Log($"Player scanned the tile: {tileTypeName}");

        return tileTypeName;
    }

    public void Measure()
    {
        IMeasureable measureableTile = GetTileInFront().tileInstance as IMeasureable;
        if (measureableTile != null) Debug.Log("Measurement result: " + measureableTile.Measured());
    }

    public int GetItemCount(int index)
    {
        if (index < 0 || index >= inventorySize) return 0;
        if (levelInventory[index].item == null) return 0;

        return levelInventory[index].amount;
    }

    public bool DiscardInventory(int index)
    {
        if (index < 0 || index >= inventorySize) return false;
        if (levelInventory[index].item == null) return false;

        Debug.Log($"Discarded {levelInventory[index].item.displayName} from slot {index}.");

        levelInventory[index] = new ItemAmount { item = null, amount = 0 };

        inventorySlots[index].itemIcon.sprite = null;
        inventorySlots[index].itemIcon.gameObject.SetActive(false);
        inventorySlots[index].amountText.text = "";

        return true;
    }

    protected void AddToInventory(ItemSO item, int amount)
    {
        if (item == null) return;

        int emptyIndex = -1;

        for (int i = 0; i < inventorySize; i++)
        {
            if (levelInventory[i].item == null)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex == -1)
        {
            Debug.Log("<color=red>Inventory is full! Cannot add more items.</color>");
            return;
        }

        inventorySlots[emptyIndex].Setup(item, amount);
        inventorySlots[emptyIndex].itemIcon.gameObject.SetActive(true);

        levelInventory[emptyIndex] = new ItemAmount { item = item, amount = amount };

        Debug.Log($"<color=green>Added {item.displayName} to Inventory Slot {emptyIndex}.</color>");
    }

    protected void DamagePlayer(int damage)
    {
        Debug.Log($"<color=red>Player took {damage} damage!</color>");
        playerHealth = Mathf.Max(playerHealth - damage, 0);
        UpdateHealth();
        if (playerHealth <= 0)
        {
            LevelGameOver();
        }
    }
    protected void UpdateHealth()
    {
        if (playerHealthBar) playerHealthBar.fillAmount = (float)playerHealth / playerMaxHealth;
        if (playerHealthText) playerHealthText.text = $"{playerHealth} / {playerMaxHealth}";
    }
    protected void LevelGameOver()
    {
        PythonExecutor.instance.continuous = false;
        PythonExecutor.instance.currentCode = null;
        Debug.Log("<color=red>Lose Level!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }

    protected void Return()
    {
        if (playerGridLoc.x == 0 && playerGridLoc.y == 0)
        {
            LevelComplete();
        }
        else
        {
            Debug.Log("You must be at the starting location to return!");
        }
    }

    protected virtual void LevelComplete()
    {
        foreach (ItemAmount collected in levelInventory)
        {
            if (collected.item != null)
            {
                InventoryManager.instance.AddItem(collected.item.itemId, collected.amount);
            }
        }

        SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);

        Debug.Log("<color=green>Level Completed!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }

    public object UseItem(int index)
    {
        if (index < 0 || index >= shuffledConsumables.Count) return false;

        ItemSO itemData = shuffledConsumables[index];
        if (itemData == null || itemData.category != ItemCategory.Consumable) return false;
        if (InventoryManager.instance.GetAmount(itemData.itemId) <= 0) return false;

        //InventoryManager.instance.DeductItem(itemData.itemId, 1);
        Debug.Log($"Used {itemData.displayName}!");

        string id = itemData.itemId.ToLower();

        return itemData.itemId;
    }

    public string InspectItem(int index)
    {
        if (index < 0 || index >= shuffledConsumables.Count)
        {
            return "Invalid";
        }

        ItemSO itemData = shuffledConsumables[index];
        if (itemData == null)
        {
            return "None";
        }

        return itemData.itemId;
    }
    public int GetConsumablesSize()
    {
        if (shuffledConsumables == null) return 0;

        return shuffledConsumables.Count;
    }
}