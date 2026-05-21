using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    protected List<Enemy> activeEnemies;

    [Header("Component")]
    [HideInInspector] public PlayerHealthComponent healthComponent;
    [HideInInspector] public PlayerCargoComponent cargoComponent;
    [HideInInspector] public CameraController cameraController;
    [HideInInspector] public Player player;
    protected TileManager tileManager;
    [HideInInspector] public CodeEditor codeEditor;

    [Header("Upgrades")]
    public UpgradeSO mapSizeUpgrade;
    public UpgradeSO cargoSizeUpgrade;
    public UpgradeSO healthUpgrade;

    [Header("Player")]
    [HideInInspector]
    public Vector2Int playerGridLoc = new();
    [HideInInspector]
    // 0 = North(N), 1 = East(E), 2 = South(S), 3 = West(W)
    public int playerFacing = 0;

    [Header("Component Setup Value")]
    protected int levelWidth = 10;
    protected int levelLength = 10;
    protected int cargoSize = 6;
    protected int maxHealth = 100;

    [Header("Consumables (Shuffled)")]
    public List<ItemSO> availableConsumables = new List<ItemSO>();
    protected List<ItemSO> shuffledConsumables = new List<ItemSO>();

    [HideInInspector] public List<string> allowedSyntaxNodes = new List<string>();
    [HideInInspector] public List<string> allowedFunctions = new List<string>();

    protected Dictionary<string, string> customLevelErrors = new Dictionary<string, string>();

    [Header("Hint")]
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
        if (!codeEditor) codeEditor = FindFirstObjectByType<CodeEditor>();

        StartValuesSetup();
        SetLevelAllowedSyntax();

        if (healthComponent != null)
        {
            healthComponent.maxHealth = maxHealth;
            healthComponent.Initialize();
            healthComponent.OnPlayerDeath += LevelGameOver;
        }

        if (tileManager)
        {
            tileManager.width = levelWidth;
            tileManager.length = levelLength;
            tileManager.GenerateMap();
        }

        if (cameraController)
        {
            cameraController.gridHeight = levelLength;
            cameraController.gridWidth = levelWidth;
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
        PythonExecutor.instance.OnRuntimeError += HandlePythonError;
        PythonExecutor.instance.OnPythonPrint += HandlePythonPrint;

        if (HintManager.instance)
        {
            List<HintSO> list = new List<HintSO>();
            if (hintCollection != null) list.AddRange(hintCollection.hints);
            if (hintList != null) list.AddRange(hintList);

            HintManager.instance.RequestDisplayHints(list, null, true, true);
        }
    }

    protected virtual void StartValuesSetup()
    {
        if (!UpgradeManager.instance) return;
        maxHealth = 100;
    }

    protected virtual void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnRuntimeError -= HandlePythonError;
            PythonExecutor.instance.OnPythonPrint -= HandlePythonPrint;
        }
        if (healthComponent != null) healthComponent.OnPlayerDeath -= LevelGameOver;
    }

    #endregion

    #region ---SYNTAX & FUNCTION SETUP---
    protected virtual void SetLevelAllowedSyntax()
    {
        customLevelErrors = new Dictionary<string, string>(ErrorDictionary.ErrorTranslations);

        if (UpgradeManager.instance != null)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("variable")) allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
            if (UpgradeManager.instance.IsUpgradeUnlocked("ifelse")) allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
            if (UpgradeManager.instance.IsUpgradeUnlocked("loop")) allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
            if (UpgradeManager.instance.IsUpgradeUnlocked("list")) allowedSyntaxNodes.AddRange(SyntaxDictionary.Lists);
        }
    }

    private void RegisterPythonCommands()
    {
        RegisterLevelSpecificPythonCommands();
        if (codeEditor != null) codeEditor.InitializeSyntaxGroups();
    }

    protected virtual void RegisterLevelSpecificPythonCommands() { }

    protected void Bind(string pyName, Action action)
    {
        if (PythonExecutor.instance != null) PythonExecutor.instance.RegisterPythonFunction(pyName, action);
    }

    protected void BindReturn<TResult>(string pyName, Func<TResult> func)
    {
        if (PythonExecutor.instance != null) PythonExecutor.instance.RegisterPythonFunction(pyName, func);
    }

    protected void BindWithArg<TArg, TResult>(string pyName, Func<TArg, TResult> func)
    {
        if (PythonExecutor.instance != null) PythonExecutor.instance.RegisterPythonFunction(pyName, func);
    }
    #endregion

    #region ---UTILITY---
    private void HandlePythonError(int line, string errorMsg)
    {
        if (codeEditor != null)
        {
            codeEditor.ShowError(line, errorMsg);
        }
        else
        {
            string translatedMsg = TranslatePythonError(errorMsg);
            PrintToDisplay($"<color=red>Error on line {line}: {translatedMsg}</color>");
        }
    }

    private void HandlePythonPrint(string msg)
    {
        PrintToDisplay(msg);
    }

    public void PrintToDisplay(string message)
    {
        Debug.Log(message);
        if (codeEditor != null) codeEditor.PrintToTerminal(message);
    }

    public string TranslatePythonError(string rawError)
    {
        if (!rawError.Contains("Syntax '") || !rawError.Contains("locked")) return rawError;

        int startIndex = rawError.IndexOf("Syntax '") + 8;
        int endIndex = rawError.IndexOf("'", startIndex);

        if (startIndex >= 8 && endIndex > startIndex)
        {
            string nodeName = rawError.Substring(startIndex, endIndex - startIndex);

            if (customLevelErrors.ContainsKey(nodeName)) return customLevelErrors[nodeName];
            if (ErrorDictionary.ErrorTranslations.ContainsKey(nodeName)) return ErrorDictionary.ErrorTranslations[nodeName];

            return $"The '{nodeName}' syntax is not allowed!";
        }

        return rawError;
    }

    public virtual bool InAction() { return player != null && player.inAction; }

    public TileObject GetCurrentTile()
    {
        return tileManager.objectsArray[playerGridLoc.x, playerGridLoc.y];
    }

    public Vector2Int GetForwardGridLoc()
    {
        int targetZ = playerGridLoc.x;
        int targetX = playerGridLoc.y;

        if (playerFacing == 0) targetZ++;
        else if (playerFacing == 1) targetX++;
        else if (playerFacing == 2) targetZ--;
        else if (playerFacing == 3) targetX--;

        return new Vector2Int(targetZ, targetX);
    }

    public virtual TileObject GetTileInFront()
    {
        Vector2Int forwardLoc = GetForwardGridLoc();

        if (forwardLoc.y >= 0 && forwardLoc.y < tileManager.width && forwardLoc.x >= 0 && forwardLoc.x < tileManager.length)
        {
            return tileManager.objectsArray[forwardLoc.x, forwardLoc.y];
        }

        return null;
    }

    private bool IsTileWalkable(int z, int x)
    {
        TileObject targetTile = tileManager.objectsArray[z, x];

        if (targetTile.type == TileType.None) return false;
        else if (targetTile.type == TileType.Floor) return true;
        else if (targetTile.tileInstance != null) return targetTile.tileInstance.isWalkable;

        return false;
    }
    #endregion

    #region ---PLAYER FUNCTIONS---

    public virtual bool MoveForward()
    {
        int targetZ = playerGridLoc.x;
        int targetX = playerGridLoc.y;

        if (playerFacing == 0) targetZ++;
        else if (playerFacing == 1) targetX++;
        else if (playerFacing == 2) targetZ--;
        else if (playerFacing == 3) targetX--;

        if (targetX < 0 || targetX >= tileManager.width || targetZ < 0 || targetZ >= tileManager.length) return false;
        if (!IsTileWalkable(targetZ, targetX)) return false;

        playerGridLoc.x = targetZ;
        playerGridLoc.y = targetX;

        player.Move(Direction.Forward);
        return true;
    }

    public virtual bool MoveBackward()
    {
        int targetZ = playerGridLoc.x;
        int targetX = playerGridLoc.y;

        if (playerFacing == 0) targetZ--;
        else if (playerFacing == 1) targetX--;
        else if (playerFacing == 2) targetZ++;
        else if (playerFacing == 3) targetX++;

        if (targetX < 0 || targetX >= tileManager.width || targetZ < 0 || targetZ >= tileManager.length) return false;
        if (!IsTileWalkable(targetZ, targetX)) return false;

        playerGridLoc.x = targetZ;
        playerGridLoc.y = targetX;

        player.Move(Direction.Backward);
        return true;
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

    public virtual string Scan()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null) return "Wall";

        string tileTypeName = targetTile.type.ToString();
        Debug.Log($"Player scanned the tile: {tileTypeName}");
        return tileTypeName;
    }

    public virtual string FarScan()
    {
        Vector2Int currentCheckLoc = GetForwardGridLoc();
        int distance = 1;

        while (true)
        {
            if (currentCheckLoc.y < 0 || currentCheckLoc.y >= tileManager.width || currentCheckLoc.x < 0 || currentCheckLoc.x >= tileManager.length)
            {
                Debug.Log($"Player far scanned the tile at distance {distance}: Wall");
                return "Wall";
            }

            if (activeEnemies != null)
            {
                foreach (Enemy enemy in activeEnemies)
                {
                    if (!enemy.isDead && enemy.gridLoc == currentCheckLoc)
                    {
                        Debug.Log($"Player far scanned the tile at distance {distance}: Enemy");
                        return "Enemy";
                    }
                }
            }

            TileObject targetTile = tileManager.objectsArray[currentCheckLoc.x, currentCheckLoc.y];

            if (targetTile.type != TileType.None && targetTile.type != TileType.Floor && targetTile.type != TileType.EnemyPath)
            {
                string tileTypeName = targetTile.type.ToString();
                Debug.Log($"Player far scanned the tile at distance {distance}: {tileTypeName}");
                return tileTypeName;
            }

            if (playerFacing == 0) currentCheckLoc.x++;
            else if (playerFacing == 1) currentCheckLoc.y++;
            else if (playerFacing == 2) currentCheckLoc.x--;
            else if (playerFacing == 3) currentCheckLoc.y--;

            distance++;
        }
    }

    public int Measure()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null || targetTile.tileInstance == null) return -1;

        IMeasureable measureableTile = targetTile.tileInstance as IMeasureable;
        if (measureableTile != null)
        {
            int result = measureableTile.Measured();
            Debug.Log("Measurement result: " + result);
            return result;
        }
        return -1;
    }

    public void Shoot()
    {
        Vector2Int currentCheckLoc = GetForwardGridLoc();
        bool hitSomething = false;

        // Play the shooting animation/sound
        // player.PerformAction(PlayerAction.Shoot); 

        while (true)
        {
            if (currentCheckLoc.y < 0 || currentCheckLoc.y >= tileManager.width || currentCheckLoc.x < 0 || currentCheckLoc.x >= tileManager.length)
            {
                Debug.Log("Your shot hit a Wall.");
                hitSomething = true;
                break;
            }

            TileObject staticTile = tileManager.objectsArray[currentCheckLoc.x, currentCheckLoc.y];
            if (staticTile.type == TileType.Wall)
            {
                Debug.Log("Your shot hit a Wall.");
                hitSomething = true;
                break;
            }

            Enemy hitEnemy = null;
            if (activeEnemies != null)
            {
                foreach (Enemy enemy in activeEnemies)
                {
                    if (!enemy.isDead && enemy.gridLoc == currentCheckLoc)
                    {
                        hitEnemy = enemy;
                        break;
                    }
                }
            }

            if (hitEnemy != null)
            {
                Debug.Log("<color=orange>Enemy destroyed!</color>");
                activeEnemies.Remove(hitEnemy);
                hitEnemy.PerformAction(EnemyAction.Death, null);
                hitSomething = true;
                break;
            }

            if (playerFacing == 0) currentCheckLoc.x++;
            else if (playerFacing == 1) currentCheckLoc.y++;
            else if (playerFacing == 2) currentCheckLoc.x--;
            else if (playerFacing == 3) currentCheckLoc.y--;
        }

        if (!hitSomething)
        {
            Debug.Log("You shot into empty space.");
        }
    }

    protected void Return()
    {
        if (playerGridLoc.x == 0 && playerGridLoc.y == 0) LevelComplete();
        else PrintToDisplay("You must be at the starting location to return!");
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

        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
    protected void LevelGameOver()
    {
        PythonExecutor.instance.StopRunningCode();
        Debug.Log("<color=red>Lose Level!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
    #endregion
}