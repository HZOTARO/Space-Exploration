using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Training : GameManager
{
    protected string lastCheckedRawCode = "";
    protected string cachedCleanCode = "";

    [Header("UI References")]
    public Transform levelCompletePopup;
    public Button finishLevelButton;

    private Vector3 startingPhysicalPos;
    private Quaternion startingPhysicalRot;

    protected bool completed = false;

    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindWithArgs<string, int>("move", Move);
        BindWithArg<string>("turn", Turn);
        Bind("wait", Wait);
    }

    protected override void Start()
    {
        SetLevelObjectives();
        base.Start();
        ObjectiveManager.instance.InitiateAllTask();
        ObjectiveManager.instance.OnAllObjectiveComplete += OnLevelComplete;

        if (player != null)
        {
            startingPhysicalPos = player.transform.position;
            startingPhysicalRot = player.transform.rotation;
        }

        if (levelCompletePopup != null)
        {
            levelCompletePopup.gameObject.SetActive(false);
        }

        if (finishLevelButton != null)
        {
            finishLevelButton.gameObject.SetActive(false);
        }
    }

    protected override void SetLevelAllowedSyntax()
    {
        customLevelErrors = new Dictionary<string, string>(ErrorDictionary.ErrorTranslations);
    }

    protected virtual void SetLevelObjectives()
    {
        if (ObjectiveManager.instance == null) return;
    }
    protected virtual void OnLevelComplete()
    {
        if (completed) return;

        if (!SaveManager.saveData.levelCompleted.Contains(PlayerPrefs.GetString("CurrentLevelId")))
        {
            SaveManager.saveData.levelCompleted.Add(PlayerPrefs.GetString("CurrentLevelId"));
            SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        }
        Debug.Log("<color=green>Training Completed!</color>");

        completed = true;

        PythonExecutor.instance.StopRunningCode();

        StartCoroutine(ShowPopupTimer());
    }
    private IEnumerator ShowPopupTimer()
    {
        yield return new WaitForSeconds(1.5f);

        if (levelCompletePopup != null)
        {
            levelCompletePopup.gameObject.SetActive(true);
        }

        if (finishLevelButton != null)
        {
            finishLevelButton.gameObject.SetActive(true);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (ObjectiveManager.instance != null) ObjectiveManager.instance.OnAllObjectiveComplete -= OnLevelComplete;
    }

    protected virtual void ResetPlayerToStart()
    {
        if (completed) return;

        playerGridLoc = Vector2Int.zero;
        playerFacing = 0;

        if (player != null)
        {
            player.transform.position = startingPhysicalPos;
            player.transform.rotation = startingPhysicalRot;
            player.ResetPlayerState();
        }

        if (cargoComponent != null)
        {
            for (int i = cargoComponent.levelCargo.Count - 1; i >= 0; i--)
            {
                cargoComponent.DiscardCargo(i);
            }
        }

        PrintToDisplay("<color=orange>Failed to complete in one run, resetting...</color>");
    }

    protected virtual void HandleAbort()
    {
        ResetPlayerToStart();
    }

    public virtual void Mine()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("Nothing to mine. You are facing the edge of the map!");
            return;
        }

        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            else Debug.Log("This White Ore has already been mined.");
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => { if (ore.Mine()) healthComponent.DamagePlayer(100); });
            else Debug.Log("This Black Ore has already been mined.");
        }
        else
        {
            Debug.Log("No mineable resource in front of you.");
        }
    }

    public virtual void Collect()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }

        if (cargoComponent && cargoComponent.IsFull())
        {
            Debug.Log("Cargo is full. Cannot collect more resources.");
            return;
        }

        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=white>Collected {amountCollected} White Ore.</color>");

                        targetTile.type = TileType.Floor;

                        if (targetTile.tileInstance != null)
                        {
                            Destroy(targetTile.tileInstance.gameObject);
                            targetTile.tileInstance = null;
                        }
                    }
                });
            }
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");

                        targetTile.type = TileType.Floor;

                        if (targetTile.tileInstance != null)
                        {
                            Destroy(targetTile.tileInstance.gameObject);
                            targetTile.tileInstance = null;
                        }
                    }
                });
            }
        }
    }

    public virtual void Drill()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }
        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (!vein.isDrilled) player.PerformAction(PlayerAction.Drill, () => vein.Drill());
        }
    }

    public virtual void Pump()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }

        if (cargoComponent && cargoComponent.IsFull())
        {
            Debug.Log("Cargo is full. Cannot collect more resources.");
            return;
        }

        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        cargoComponent.AddToCargo(vein.itemOnTile, amountPumped);
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");

                        targetTile.type = TileType.Floor;

                        if (targetTile.tileInstance != null)
                        {
                            targetTile.tileInstance = null;
                        }
                    }
                });
            }
        }
    }

    public virtual void Purify()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }

        if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isPurified) player.PerformAction(PlayerAction.Purify, () => ore.Purify());
        }
    }

    protected bool ValidateFunctionCallCount(string functionName, int targetCount, bool exactMatch = false)
    {
        if (PythonExecutor.instance == null) return false;

        string currentCode = PythonExecutor.instance.currentCode;
        if (string.IsNullOrEmpty(currentCode)) return false;

        if (lastCheckedRawCode != currentCode)
        {
            cachedCleanCode = Regex.Replace(currentCode, @"#.*", "");
            cachedCleanCode = Regex.Replace(cachedCleanCode, "<.*?>", "");
            cachedCleanCode = cachedCleanCode.Replace("\u200B", "");

            cachedCleanCode = Regex.Replace(cachedCleanCode, "\".*?\"", "");
            cachedCleanCode = Regex.Replace(cachedCleanCode, "'.*?'", "");

            lastCheckedRawCode = currentCode;
        }

        string pattern = @"\b" + functionName + @"\s*\(";
        int actualCount = Regex.Matches(cachedCleanCode, pattern).Count;

        bool isValid = exactMatch ? (actualCount == targetCount) : (actualCount <= targetCount);

        if (!isValid)
        {
            string conditionText = exactMatch ? $"exactly {targetCount}" : $"a maximum of {targetCount}";
            PythonExecutor.instance.TriggerRuntimeError($"Constraint Failed! You are only allowed to write {functionName}() {conditionText} time(s)! You typed {actualCount} times", true);
            return false;
        }

        return true;
    }

    protected override void LevelGameOver()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.StopRunningCode();
        }

        if (player)
        {
            player.Die(() =>
            {
                ResetPlayerToStart();
                if (healthComponent) healthComponent.Initialize();
            });
        }
        else
        {
            ResetPlayerToStart();
            if (healthComponent) healthComponent.Initialize();
        }
    }
}