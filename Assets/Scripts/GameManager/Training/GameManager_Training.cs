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
    public Button levelCompletePopup;

    private bool hasFinished = false;

    private Vector3 startingPhysicalPos;
    private Quaternion startingPhysicalRot;

    protected override void RegisterLevelSpecificPythonCommands()
    {
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
            levelCompletePopup.onClick.AddListener(FinishAndLeaveLevel);
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
        if (levelCompletePopup != null)
        {
            levelCompletePopup.gameObject.SetActive(true);
        }

        StartCoroutine(AutoFinishTimer());
    }
    private IEnumerator AutoFinishTimer()
    {
        yield return new WaitForSeconds(5f);
        FinishAndLeaveLevel();
    }

    private void FinishAndLeaveLevel()
    {
        if (hasFinished) return;
        hasFinished = true;

        LevelComplete();
    }

    public override void LevelComplete()
    {
        if (!SaveManager.saveData.levelCompleted.Contains(PlayerPrefs.GetString("CurrentLevelId")))
        {
            SaveManager.saveData.levelCompleted.Add(PlayerPrefs.GetString("CurrentLevelId"));
            SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        }

        Debug.Log("<color=green>Training Completed!</color>");
        LevelManager.instance.OpenScene(LevelType.Hub);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (ObjectiveManager.instance != null) ObjectiveManager.instance.OnAllObjectiveComplete -= OnLevelComplete;
        if (levelCompletePopup != null) levelCompletePopup.onClick.RemoveListener(FinishAndLeaveLevel);
    }

    protected virtual void ResetPlayerToStart()
    {
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
    }

    protected virtual void HandleAbort()
    {
        PrintToDisplay("<color=orange>Program Aborted. Resetting position...</color>");
        ResetPlayerToStart();
    }

    protected virtual void HandleRuntimeError(int line, string message)
    {
        PrintToDisplay($"<color=red>Code Error: {message}</color> Resetting...");
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
        else
        {
            Debug.Log("No mineable resource in front of you.");
        }
    }

    public virtual void Collect()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile != null && targetTile.type == TileType.WhiteOre)
        {
            if (cargoComponent.IsFull())
            {
                return;
            }
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        PrintToDisplay($"<color=white>Collected {amountCollected} White Ore.</color>");
                    }
                });
            }
        }
        else
        {
            PrintToDisplay("Nothing to collect here.");
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

        string pattern = functionName + @"\s*\(\s*\)";
        int actualCount = Regex.Matches(cachedCleanCode, pattern).Count;

        bool isValid = exactMatch ? (actualCount == targetCount) : (actualCount <= targetCount);

        if (!isValid)
        {
            string conditionText = exactMatch ? $"exactly {targetCount}" : $"a maximum of {targetCount}";
            PrintToDisplay($"<color=red>Constraint Failed! You are only allowed to write {functionName}() {conditionText} time(s)!</color>");
            PythonExecutor.instance.StopRunningCode();
            return false;
        }

        return true;
    }
}