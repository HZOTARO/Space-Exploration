using UnityEngine;

public class GameManager_Training_5 : GameManager_Training
{
    private Vector3 startingPhysicalPos;
    private Quaternion startingPhysicalRot;

    protected override void RegisterLevelSpecificPythonCommands()
    {
        Bind("move_forward", MoveForward);
        Bind("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
        BindReturn("scan", Scan);
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();

        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "The goal is at an unknown distance! Use a 'while' loop and scan() to reach it.",
            type = ObjectiveType.CustomEvent,
            customEventId = "ReachedGoal"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (player != null)
        {
            startingPhysicalPos = player.transform.position;
            startingPhysicalRot = player.transform.rotation;
        }

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckPrecisionGoal;
        }
    }

    private void CheckPrecisionGoal()
    {
        TileObject finalTile = GetCurrentTile();

        if (finalTile != null && finalTile.type == TileType.Goal)
        {
            ObjectiveManager.instance.TriggerCustomEvent("ReachedGoal");
            PrintToDisplay("<color=green>Excellent! You successfully used logic to find the unknown goal distance.</color>");
            base.OnLevelComplete();
        }
        else
        {
            PrintToDisplay("<color=red>Missed! You did not stop on the Goal tile. Resetting position...</color>");
            ResetPlayerToStart();
        }
    }

    private void ResetPlayerToStart()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.StopRunningCode();
        }

        playerGridLoc = Vector2Int.zero;
        playerFacing = 0;

        if (player != null)
        {
            player.transform.position = startingPhysicalPos;
            player.transform.rotation = startingPhysicalRot;
            player.inAction = false;
        }
    }

    protected override void StartValuesSetup()
    {
        levelLength = 1;
        levelWidth = Random.Range(100, 201);
        cargoSize = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckPrecisionGoal;
        }
    }
}
