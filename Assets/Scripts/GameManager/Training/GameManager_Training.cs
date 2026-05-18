using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Training : GameManager
{
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
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionAborted -= ResetPlayerToStart;
            PythonExecutor.instance.StopRunningCode();
            PythonExecutor.instance.OnExecutionAborted += ResetPlayerToStart;
        }

        playerGridLoc = Vector2Int.zero;
        playerFacing = 0;

        if (player != null)
        {
            player.transform.position = startingPhysicalPos;
            player.transform.rotation = startingPhysicalRot;
            player.ResetPlayerState();
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

    public void Mine()
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

    public void Collect()
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
}