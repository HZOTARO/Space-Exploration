using UnityEngine;
using System.Collections.Generic;

public class GameManager_Training_9 : GameManager_Training
{
    private List<Vector2Int> visitedTiles = new List<Vector2Int>();

    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);
        Bind("mine", Mine);
        Bind("collect", Collect);
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
            description = "Traverse every tile on the map in a single run.\nYou may only write 'move()' a maximum of 2 times!\nDo it in a single run.",
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
        }

        OnSuccessfulMove += VisitTile;

        ClearVisitedTiles();
    }

    private void ClearVisitedTiles()
    {
        visitedTiles.Clear();
        VisitTile();
    }

    public override void Move(string directionString, int distance)
    {
        if (PythonExecutor.instance == null) return;
        
        string currentCode = PythonExecutor.instance.currentCode;

        if (!ValidateFunctionCallCount("move", 2, false)) return;
        base.Move(directionString, distance);
    }

    private void VisitTile()
    {
        if (!visitedTiles.Contains(playerGridLoc))
        {
            visitedTiles.Add(playerGridLoc);
            if (player.markPrefab && tileManager)
            {
                tileManager.InstantiateTileVisual(playerGridLoc.x, playerGridLoc.y, player.markPrefab);
            }
        }
    }

    private void CheckGridCompletion()
    {
        if (completed) return;

        int totalTiles = levelLength * levelWidth;
        if (visitedTiles.Count < totalTiles)
        {
            PrintToDisplay($"<color=red>Incomplete! You only traversed {visitedTiles.Count} out of {totalTiles} tiles.</color>");
            ResetPlayerToStart();
            return;
        }

        ObjectiveManager.instance.TriggerCustomEvent("TraversedGrid");
        PrintToDisplay("<color=green>Grids successfully fully traversed!</color>");
    }

    protected override void ResetPlayerToStart()
    {
        if (completed) return;

        base.ResetPlayerToStart();

        if (tileManager != null) tileManager.GenerateMap();

        ClearVisitedTiles();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckGridCompletion;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }

        OnSuccessfulMove -= VisitTile;
    }

    protected override void StartValuesSetup()
    {
        levelLength = 10;
        levelWidth = 10;
        cargoSize = 0;
    }
}
