using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Component")]
    [HideInInspector] public PlayerHealthComponent healthComponent;
    [HideInInspector] public PlayerCargoComponent cargoComponent;
    [HideInInspector] public CameraController cameraController;
    [HideInInspector] public Player player;
    protected TileManager tileManager;

    [Header("Upgrades")]
    public UpgradeSO mapSizeUpgrade;
    public UpgradeSO cargoSizeUpgrade;
    public UpgradeSO healthUpgrade;

    [Header("Player")]
    Vector2Int playerGridLoc = new();
    [HideInInspector]
    // 0 = North(N), 1 = East(E), 2 = South(S), 3 = West(W)
    public int playerFacing = 0;

    [Header("Component Setup Value")]
    protected int levelSize = 10;
    protected int cargoSize = 5;
    protected int maxHealth = 100;

    [Header("Consumables (Shuffled)")]
    public List<ItemSO> availableConsumables = new List<ItemSO>();
    protected List<ItemSO> shuffledConsumables = new List<ItemSO>();

    [HideInInspector] public List<string> allowedSyntaxNodes = new List<string>();
    [HideInInspector] public List<string> allowedFunctions = new List<string>();

    [Header("Hint")]
    bool useHintCollection = true;
    public HintCollectionSO hintCollection;
    public List<HintSO> hintList;

    #region ---UNITY LIFECYCLE---

    protected virtual void Awake()
    {
        allowedSyntaxNodes = new List<string>(SyntaxDictionary.Core);
        allowedFunctions = new List<string>(FunctionDictionary.Core);
    }

    protected virtual void Start()
    {
        if (!healthComponent) healthComponent = FindFirstObjectByType<PlayerHealthComponent>();
        if (!cargoComponent) cargoComponent = FindFirstObjectByType<PlayerCargoComponent>();
        if (!cameraController) cameraController = FindFirstObjectByType<CameraController>();
        if (!player) player = FindFirstObjectByType<Player>();
        if (!tileManager) tileManager = FindFirstObjectByType<TileManager>();

        SetLevelAllowedSyntax();
        StartValuesSetup();

        if (healthComponent != null)
        {
            healthComponent.maxHealth = maxHealth;

            healthComponent.Initialize();
            healthComponent.OnPlayerDeath += LevelGameOver;
        }

        if (tileManager) 
        { 
            tileManager.width = levelSize;
            tileManager.length = levelSize;

            tileManager.GenerateMap();
        }

        if (cameraController)
        {
            cameraController.gridHeight = levelSize;
            cameraController.gridWidth = levelSize;

            cameraController.Initialize();
        }

        if (cargoComponent)
        {
            cargoComponent.cargoSize = cargoSize;

            StartCoroutine(cargoComponent.SetupCargoCoroutine());
        }

        shuffledConsumables = new List<ItemSO>(availableConsumables);
        for (int i = 0; i < shuffledConsumables.Count; i++)
        {
            ItemSO temp = shuffledConsumables[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffledConsumables.Count);
            shuffledConsumables[i] = shuffledConsumables[randomIndex];
            shuffledConsumables[randomIndex] = temp;
        }

        PythonExecutor.instance.InitializePythonAllowed(allowedSyntaxNodes.ToArray(), allowedFunctions.ToArray());

        RegisterPythonCommands();

        PythonExecutor.instance.CanStepCode = () => !InAction();
        PythonExecutor.instance.OnPythonPrint += PrintToDisplay;

        if (HintManager.instance)
        {
            if (useHintCollection && hintCollection)
            {
                HintManager.instance.RequestDisplayHints(hintCollection, onlyShowNewOne: true);
            }
            else
            {
                HintManager.instance.RequestDisplayHints(hintList, onlyShowNewOne: true);
            }
        }
    }

    protected virtual void StartValuesSetup()
    {
        if (!UpgradeManager.instance) return;

        if (healthUpgrade && healthComponent)
        {
            int healthLevel = UpgradeManager.instance.GetUpgradeLevel(healthUpgrade.id);
            maxHealth += 50 + 25 * (healthLevel - 1);
        }
    }

    protected virtual void OnDestroy()
    {
        if (PythonExecutor.instance != null) PythonExecutor.instance.OnPythonPrint -= PrintToDisplay;
        if (healthComponent != null) healthComponent.OnPlayerDeath -= LevelGameOver;
    }

    #endregion

    #region ---SYNTAX & FUNCTION SETUP---
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

        //BindWithArg<int, object>("use_item", UseItem);
        //BindWithArg<int, string>("inspect_item", InspectItem);
        //BindReturn<int>("item_type_count", GetConsumablesSize);
        //BindWithArg<int, int>("item_count", GetItemCount);
        //BindWithArg<int, bool>("discard_inventory", DiscardInventory);

        RegisterLevelSpecificPythonCommands();

        CodeEditor editor = FindFirstObjectByType<CodeEditor>();
        if (editor != null) editor.InitializeSyntaxGroups();
    }

    protected virtual void RegisterLevelSpecificPythonCommands() { }
    #endregion

    #region ---UTILITY---
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
    public bool InAction() { return player != null && player.inAction; }
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

        if (targetTile.type == TileType.None)
        {
            return false;
        } 
        else if (targetTile.type == TileType.Floor)
        {
            return true;
        }
        else if (targetTile.tileInstance != null)
        {
            return targetTile.tileInstance.isWalkable;
        }

        return false;
    }
    #endregion

    #region ---PLAYER FUNCTIONS---

    public virtual void MoveForward()
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

    public virtual void MoveBackward()
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
    #endregion

    #region ---LEVEL COMPLETE & GAME OVER---
    public virtual void LevelComplete()
    {
        if (cargoComponent != null)
        {
            foreach (ItemAmount collected in cargoComponent.levelCargo)
            {
                if (collected.item != null) InventoryManager.instance.AddItem(collected.item.itemId, collected.amount);
            }
        }

        SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);

        Debug.Log("<color=green>Level Completed!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
    protected void LevelGameOver()
    {
        PythonExecutor.instance.continuous = false;
        PythonExecutor.instance.currentCode = null;
        Debug.Log("<color=red>Lose Level!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
    #endregion

    #region ---ITEM---
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
    #endregion
}