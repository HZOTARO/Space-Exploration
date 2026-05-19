using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameManager_Training_6 : GameManager_Training
{
    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
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
            description = "Traverse every tile on the map. You may only write 'move_forward()' a maximum of 2 times!",
            type = ObjectiveType.CustomEvent,
            customEventId = "TraversedGrid"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckGridCompletion;

            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
        }

        ClearVisitedTiles();
    }

    private void ClearVisitedTiles()
    {
        visitedTiles.Clear();
        visitedTiles.Add(Vector2Int.zero);
    }

    public override bool MoveForward()
    {
        if (!ValidateFunctionCallCount("move_forward", 2, false)) return false;

        bool result = base.MoveForward();
        visitedTiles.Add(playerGridLoc);
        return result;
    }

    public override bool MoveBackward()
    {
        if (!ValidateFunctionCallCount("move_backward", 2, false)) return false;

        bool result = base.MoveBackward();
        visitedTiles.Add(playerGridLoc);
        return result;
    }

    private void CheckGridCompletion()
    {
        int totalTiles = levelLength * levelWidth;
        if (visitedTiles.Count < totalTiles)
        {
            PrintToDisplay($"<color=red>Incomplete! You only traversed {visitedTiles.Count} out of {totalTiles} tiles.</color>");
            ResetPlayerToStart();
            ClearVisitedTiles();
            return;
        }

        ObjectiveManager.instance.TriggerCustomEvent("TraversedGrid");
        PrintToDisplay("<color=green>Grid successfully fully traversed efficiently!</color>");
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();
        ClearVisitedTiles();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckGridCompletion;

            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }

    protected override void StartValuesSetup()
    {
        levelLength = 10;
        levelWidth = 10;
        cargoSize = 0;
    }
}
