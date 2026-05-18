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
        bool result = base.MoveForward();
        visitedTiles.Add(playerGridLoc);
        return result;
    }

    public override bool MoveBackward()
    {
        bool result = base.MoveBackward();
        visitedTiles.Add(playerGridLoc);
        return result;
    }

    private void CheckGridCompletion()
    {
        string code = PythonExecutor.instance.currentCode;

        if (!string.IsNullOrEmpty(code))
        {
            int moveCount = Regex.Matches(code, "move_forward\\(\\)").Count;

            if (moveCount > 2)
            {
                PrintToDisplay($"<color=red>Constraint Failed! You called move_forward() {moveCount} times. You are only allowed to write it 2 times!</color>");
                ResetPlayerToStart();
                ClearVisitedTiles();
                return;
            }
        }

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
        base.OnLevelComplete();
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
        levelLength = 20;
        levelWidth = 20;
        cargoSize = 0;
    }
}
