using UnityEngine;

public class GameManager_Training_8 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
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
            description = "The goal is at an unknown distance! Use a while loop and scan() to reach it.",
            type = ObjectiveType.CustomEvent,
            customEventId = "ReachedGoal"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckPrecisionGoal;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    private void CheckPrecisionGoal()
    {
        TileObject finalTile = GetCurrentTile();

        if (finalTile != null && finalTile.type == TileType.Goal)
        {
            ObjectiveManager.instance.TriggerCustomEvent("ReachedGoal");
            PrintToDisplay("<color=green>Excellent! You successfully used logic to find the unknown goal distance.</color>");
        }
        else
        {
            PrintToDisplay("<color=red>Missed! You did not stop on the Goal tile. Resetting position...</color>");
            ResetPlayerToStart();
        }
    }

    protected override void StartValuesSetup()
    {
        levelLength = 1;
        levelWidth = Random.Range(30, 51);
        cargoSize = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckPrecisionGoal;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}
