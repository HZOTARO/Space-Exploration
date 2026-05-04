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
    int playerMaxHealth = 100;
    int playerHealth;
    // 0 = North(N), 1 = East(E), 2 = South(S), 3 = West(W)
    public int playerFacing = 0;

    [Header("Tile")]
    protected int levelSize = 10;
    private TileManager tileManager;
    Vector2Int playerGridLoc = new();

    [Header("Code Editor")]
    public TextMeshProUGUI terminalText;
    private int maxLines = 10;

    [Header("Inventory")]
    [Range(1, 15)]
    public int inventorySize = 6;
    private int currentInventoryIndex = 0;
    public Transform inventoryUI;
    private List<ItemAmount> levelInventory = new List<ItemAmount>();
    private List<ItemSlotUI> inventorySlots = new List<ItemSlotUI>();

    [Header("Level Restrictions (Python)")]
    public List<string> levelBannedSyntaxNodes = new List<string>();
    public List<string> levelBannedFunctions = new List<string>();

    [HideInInspector] public List<string> bannedSyntaxNodes = new List<string>();
    [HideInInspector] public List<string> bannedFunctions = new List<string>();

    void Awake()
    {
        List<string> permanentlyBannedSyntax = new List<string>
        {
            "ClassDef", "Yield", "YieldFrom", "Lambda", "Import", "ImportFrom",
            "Try", "ExceptHandler", "Raise", "With", "Global", "Nonlocal",
            "AsyncFunctionDef", "Await", "Delete", "Assert"
        };

        List<string> permanentlyBannedFunctions = new List<string>
        {
            "eval", "exec", "open", "compile", "__import__", "globals", "locals",
            "getattr", "setattr", "delattr", "hasattr", "input", "super", "dir", "help", "memoryview"
        };

        bannedSyntaxNodes = new List<string>(permanentlyBannedSyntax);
        bannedSyntaxNodes.AddRange(levelBannedSyntaxNodes);

        bannedFunctions = new List<string>(permanentlyBannedFunctions);
        bannedFunctions.AddRange(levelBannedFunctions);

        PythonExecutor.instance.InitializePythonBans(bannedSyntaxNodes.ToArray(), bannedFunctions.ToArray());
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
        RegisterPythonCommands();

        PythonExecutor.instance.CanStepCode = () => !InAction();
        PythonExecutor.instance.OnPythonPrint += PrintToDisplay;
    }

    protected virtual void SetupMap()
    {
        if (tileManager)
        {
            tileManager.width = levelSize;
            tileManager.length = levelSize;
            tileManager.GenerateMap();
        }
    }

    private void RegisterPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);
        void BindReturn<T>(string pyName, Func<T> func) => PythonExecutor.instance.RegisterPythonFunction(pyName, func);

        Bind("move_forward", MoveForward);
        Bind("move_backward", MoveBackward);
        //Bind("move_left", () => Move("W"));
        //Bind("move_right", () => Move("E"));
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
        Bind("go_back", Return);
        BindReturn("scan", Scan);

        RegisterLevelSpecificPythonCommands();
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
        if (terminalText == null) return;

        terminalText.text += "> " + message + "\n";

        string[] lines = terminalText.text.Split('\n');
        if (lines.Length > maxLines)
        {
            terminalText.text = string.Join("\n", lines, 1, Mathf.Min(lines.Length - 1, 10));
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
        TileObject currentTile = GetCurrentTile();

        string tileTypeName = currentTile.type.ToString();

        Debug.Log($"Player scanned the tile: {tileTypeName}");

        return tileTypeName;
    }

    protected void AddToInventory(ItemSO item, int amount)
    {
        if (currentInventoryIndex >= inventorySize)
        {
            Debug.Log("Inventory is full! Cannot add more items.");
            return;
        }

        if (item == null) return;

        inventorySlots[currentInventoryIndex].Setup(item, amount);
        inventorySlots[currentInventoryIndex].itemIcon.gameObject.SetActive(true);
        
        levelInventory[currentInventoryIndex] = new ItemAmount { item = item, amount = amount };
        currentInventoryIndex++;
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

    protected void LevelComplete()
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

    public void UseItem(string requestedItemId)
    {
        string itemId = requestedItemId.ToLower().Trim();

        ItemSO itemData = InventoryManager.instance.GetItemData(itemId);
        if (itemData == null)
        {
            Debug.Log($"<color=red>Error: The item '{itemId}' does not exist in the game.</color>");
            return;
        }

        if (itemData.category != ItemCategory.Consumable)
        {
            Debug.Log($"<color=red>Cannot use '{itemData.displayName}': That is a material, not a consumable!</color>");
            return;
        }

        if (InventoryManager.instance.GetAmount(itemId) <= 0)
        {
            Debug.Log($"<color=red>Cannot use: You don't have any {itemData.displayName}!</color>");
            return;
        }

        InventoryManager.instance.DeductItem(itemId, 1);

        Debug.Log($"<color=green>Used {itemData.displayName}!</color>");
    }
}