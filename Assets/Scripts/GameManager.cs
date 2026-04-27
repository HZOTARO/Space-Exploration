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
    public List<ResourceImage> resourceSprites = new List<ResourceImage>();
    private List<(ResourceType resourceType, int amount)> inventory = new List<(ResourceType resourceType, int amount)>();
    private List<(Image slotImage, TextMeshProUGUI slotText)> inventorySlots = new List<(Image slotImage, TextMeshProUGUI slotText)>();

    [Header("Level Restrictions (Python)")]
    public List<string> bannedSyntaxNodes = new List<string>();
    public List<string> bannedFunctions = new List<string>();

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

        bannedSyntaxNodes = new List<string> { "For", "While", "FunctionDef" };
        bannedFunctions = new List<string> { "print", "range" };
        PythonExecutor.instance.InitializePythonBans(bannedSyntaxNodes.ToArray(), bannedFunctions.ToArray());

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
                    inventory.Add((ResourceType.None, 0));

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

            if (!ore.isMined)
            {
                player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            }
            else
            {
                Debug.Log("This White Ore has already been mined.");
            }
        }
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined)
            {
                player.PerformAction(PlayerAction.Mine, () =>
                {
                    if (ore.Mine())
                    {
                        DamagePlayer(60);
                    }
                });
            }
            else
            {
                Debug.Log("This Black Ore has already been mined.");
            }
        }
        else
        {
            Debug.Log("No mineable resource at current location.");
        }
    }
    public void Purify()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;

            if (!ore.isPurified)
            {
                player.PerformAction(PlayerAction.Purify, () => ore.Purify());
            }
            else
            {
                Debug.Log("This Black Ore has already been purified.");
            }
        }
        else
        {
            Debug.Log("No purifable resource at current location.");
        }
    }

    public void Drill()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;

            if (!vein.isDrilled)
            {
                player.PerformAction(PlayerAction.Drill, () => vein.Drill());
            }
            else
            {
                Debug.Log("This Purple Vein has already been drilled.");
            }
        }
        else
        {
            Debug.Log("No drillable resource at current location.");
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
                        AddToInventory(ResourceType.PurpleLiquid, amountPumped);
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot pump! It is either not drilled yet, or already empty.");
            }
        }
        else
        {
            Debug.Log("No pumpable resource at current location.");
        }
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
                        AddToInventory(ResourceType.WhiteOre, amountCollected);
                        Debug.Log($"<color=white>Collected {amountCollected} White Ore.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot collect! It is either not mined, or already collected.");
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
                        AddToInventory(ResourceType.BlackOre, amountCollected);
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");
                    }
                });
            }
            else
            {
                Debug.Log("Cannot collect! It is either not mined, or already collected.");
            }
        }
        else
        {
            Debug.Log("No collectable resource at current location.");
        }
    }
    public void Measure()
    {
        IMeasureable measureableTile = GetCurrentTile().tileInstance as IMeasureable;
        if (measureableTile != null)
        {
            int measurement = measureableTile.Measured();
            Debug.Log("Measurement result: " + measurement);
        }
        else
        {
            Debug.Log("Current tile is not measurable.");
        }
    }

    private void AddToInventory(ResourceType tileType, int amount)
    {
        if (currentInventoryIndex >= inventorySize)
        {
            Debug.Log("Inventory is full! Cannot add more items.");
            return;
        }

        foreach (ResourceImage tileImage in resourceSprites)
        {
            if (tileImage.type == tileType)
            {
                inventorySlots[currentInventoryIndex].slotImage.sprite = tileImage.image;
                inventorySlots[currentInventoryIndex].slotImage.gameObject.SetActive(true);
                break;
            }
        }
        inventorySlots[currentInventoryIndex].slotText.text = amount.ToString();
        inventory[currentInventoryIndex] = (tileType, amount);
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
        int whiteOreCount = 0;
        int purpleLiquidCount = 0;
        int blackOreCount = 0;
        foreach ((ResourceType resourceType, int amount) in inventory)
        {
            switch (resourceType)
            {
                case ResourceType.WhiteOre:
                    whiteOreCount += amount;
                    break;
                case ResourceType.PurpleLiquid:
                    purpleLiquidCount += amount;
                    break;
                case ResourceType.BlackOre:
                    blackOreCount += amount;
                    break;
            }
        }
        SaveManager.saveData.whiteOre += whiteOreCount;
        SaveManager.saveData.purpleLiquid += purpleLiquidCount;
        SaveManager.saveData.blackOre += blackOreCount;
        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        Debug.Log("<color=green>Level Completed!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
}