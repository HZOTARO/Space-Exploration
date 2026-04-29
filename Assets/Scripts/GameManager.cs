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

    [Header("Tile")]
    int levelSize = 10;
    private TileManager tileManager;
    Vector2Int playerGridLoc = new();

    [Header("Code Editor")]
    public TextMeshProUGUI terminalText;
    private int maxLines = 10;

    [Header("Inventory")]
    [Range(6, 10)]
    public int inventorySize = 6;
    private int currentInventoryIndex = 0;
    public Transform inventoryUI;
    private List<(ItemSO item, int amount)> levelInventory = new List<(ItemSO item, int amount)>();
    private List<(Image slotImage, TextMeshProUGUI slotText)> inventorySlots = new List<(Image slotImage, TextMeshProUGUI slotText)>();

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

    void Start()
    {
        playerHealth = playerMaxHealth;
        UpdateHealth();

        if (!player) player = FindFirstObjectByType<Player>();
        if (!tileManager) tileManager = FindFirstObjectByType<TileManager>();
        if (tileManager)
        {
            tileManager.width = levelSize;
            tileManager.length = levelSize;
            tileManager.GenerateMap();
        }

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

    private void RegisterPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);
        void BindReturn<T>(string pyName, Func<T> func) => PythonExecutor.instance.RegisterPythonFunction(pyName, func);

        Bind("move_up", () => Move("N"));
        Bind("move_down", () => Move("S"));
        Bind("move_left", () => Move("W"));
        Bind("move_right", () => Move("E"));

        Bind("mine", Mine);
        Bind("collect", Collect);
        Bind("purify", Purify);
        Bind("drill", Drill);
        Bind("pump", Pump);
        Bind("measure", Measure);
        Bind("go_back", Return);

        BindReturn("scan", Scan);
    }

    //public void UnlockFeature(string featureName)
    //{
    //    if (bannedSyntaxNodes.Contains(featureName)) bannedSyntaxNodes.Remove(featureName);
    //    if (bannedFunctions.Contains(featureName)) bannedFunctions.Remove(featureName);

    //    PythonExecutor.instance.ClearPythonBan(featureName);
    //}

    IEnumerator SetupInventory()
    {
        Debug.Log("Setting up inventory...");
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

                Image slotImage = null;
                TextMeshProUGUI slotText = null;

                foreach (Transform slotElement in slotTransform.GetChild(0))
                {
                    if (slotElement.TryGetComponent<Image>(out Image image))
                    {
                        slotImage = image;
                    }
                    if (slotElement.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
                    {
                        slotText = text;
                    }
                }

                if (slotImage && slotText)
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

                    Image newImage = null;
                    TextMeshProUGUI newText = null;

                    foreach (Transform newSlotBackground in newSlot.GetChild(0))
                    {
                        if (newSlotBackground.TryGetComponent<Image>(out Image image))
                        {
                            newImage = image;
                        }
                        if (newSlotBackground.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
                        {
                            newText = text;
                        }
                    }

                    inventorySlots.Add((newImage, newText));
                    levelInventory.Add((null, 0));

                    inventorySlots[i].slotImage.sprite = null;
                    inventorySlots[i].slotImage.gameObject.SetActive(false);
                    inventorySlots[i].slotText.text = "";
                    inventorySlots[i].slotText.fontSize = 30 + 6 * ((10 - inventorySize) / 4);

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
        return tileManager.gridArray[playerGridLoc.y, playerGridLoc.x];
    }
    public void Move(string dir)
    {
        switch (dir)
        {
            case "N":
                if (playerGridLoc.y < tileManager.length - 1)
                {
                    player.Move(Direction.Forward);
                    playerGridLoc.y++;
                }
                break;
            case "S":
                if (playerGridLoc.y > 0)
                {
                    player.Move(Direction.Backward);
                    playerGridLoc.y--;
                }
                break;
            case "E":
                if (playerGridLoc.x < tileManager.width - 1)
                {
                    player.Move(Direction.Right);
                    playerGridLoc.x++;
                }
                break;
            case "W":
                if (playerGridLoc.x > 0)
                {
                    player.Move(Direction.Left);
                    playerGridLoc.x--;
                }
                break;
        }
    }
    public string Scan()
    {
        TileObject currentTile = GetCurrentTile();

        string tileTypeName = currentTile.type.ToString();

        Debug.Log($"Player scanned the tile: {tileTypeName}");

        return tileTypeName;
    }

    public void Mine()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            else Debug.Log("This White Ore has already been mined.");
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => { if (ore.Mine()) DamagePlayer(60); });
            else Debug.Log("This Black Ore has already been mined.");
        }
        else Debug.Log("No mineable resource at current location.");
    }

    public void Collect()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        AddToInventory(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=white>Collected {amountCollected} White Ore.</color>");
                    }
                });
            }
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        AddToInventory(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");
                    }
                });
            }
        }
    }
    public void Drill()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            if (!vein.isDrilled) player.PerformAction(PlayerAction.Drill, () => vein.Drill());
        }
    }

    public void Pump()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        AddToInventory(vein.itemOnTile, amountPumped);
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                    }
                });
            }
        }
    }

    public void Purify()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isPurified) player.PerformAction(PlayerAction.Purify, () => ore.Purify());
        }
    }

    public void Measure()
    {
        IMeasureable measureableTile = GetCurrentTile().tileInstance as IMeasureable;
        if (measureableTile != null) Debug.Log("Measurement result: " + measureableTile.Measured());
    }

    private void AddToInventory(ItemSO item, int amount)
    {
        if (currentInventoryIndex >= inventorySize)
        {
            Debug.Log("Inventory is full! Cannot add more items.");
            return;
        }

        if (item == null) return;

        inventorySlots[currentInventoryIndex].slotImage.sprite = item.icon;
        inventorySlots[currentInventoryIndex].slotImage.gameObject.SetActive(true);
        inventorySlots[currentInventoryIndex].slotText.text = amount.ToString();

        levelInventory[currentInventoryIndex] = (item, amount);
        currentInventoryIndex++;
    }

    private void DamagePlayer(int damage)
    {
        Debug.Log($"<color=red>Player took {damage} damage!</color>");
        playerHealth = Mathf.Max(playerHealth - damage, 0);
        UpdateHealth();
        if (playerHealth <= 0)
        {
            LevelGameOver();
        }
    }
    private void UpdateHealth()
    {
        if (playerHealthBar) playerHealthBar.fillAmount = (float)playerHealth / playerMaxHealth;
        if (playerHealthText) playerHealthText.text = $"{playerHealth} / {playerMaxHealth}";
    }
    private void LevelGameOver()
    {
        PythonExecutor.instance.continuous = false;
        PythonExecutor.instance.currentCode = null;
        Debug.Log("<color=red>Lose Level!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }

    private void Return()
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

    private void LevelComplete()
    {
        foreach (var collected in levelInventory)
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