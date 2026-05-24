public class GameManager_Training_5 : GameManager_Training
{
    // Randomized value so we need to use if else

    private int playerValidMines = 0;
    private int playerValidCollects = 0;

    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("measure", Measure);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
        Bind("mine", Mine);
        Bind("collect", Collect);
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);

        allowedSyntaxNodes.Remove("Match");
        allowedSyntaxNodes.Remove("match_case");
        allowedSyntaxNodes.Remove("MatchValue");
        allowedSyntaxNodes.Remove("MatchAs");
        allowedSyntaxNodes.Remove("MatchOr");

        string matchErrorMsg = "Match/Case is locked for now! You must use 'if' and 'else' to solve this level.";

        customLevelErrors["Match"] = matchErrorMsg;
        customLevelErrors["match_case"] = matchErrorMsg;
        customLevelErrors["MatchValue"] = matchErrorMsg;
        customLevelErrors["MatchAs"] = matchErrorMsg;
        customLevelErrors["MatchOr"] = matchErrorMsg;
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();
        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Check both ores in a single run! Mine ores if value > 5, then collect them if value > 10.",
            type = ObjectiveType.CustomEvent,
            customEventId = "LevelSolved"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 2;
        cargoSize = 2;
    }

    protected override void Start()
    {
        base.Start();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    public override void Mine()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile != null && targetTile.type == TileType.WhiteOre)
        {
            if (targetTile.tileInstance is ValueTile vTile)
            {
                if (vTile.value <= 5)
                {
                    PrintToDisplay($"<color=red>Error: You mined an ore with value {vTile.value}! Only mine if > 5.</color>");
                    if (PythonExecutor.instance != null) PythonExecutor.instance.StopRunningCode();
                    ResetPlayerToStart();
                    return;
                }

                CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
                if (ore != null && !ore.isMined) playerValidMines++;
            }
        }
        base.Mine();
    }

    public override void Collect()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile != null && targetTile.type == TileType.WhiteOre)
        {
            if (targetTile.tileInstance is ValueTile vTile)
            {
                if (vTile.value <= 10)
                {
                    PrintToDisplay($"<color=red>Error: You collected an ore with value {vTile.value}! Only collect if > 10.</color>");
                    if (PythonExecutor.instance != null) PythonExecutor.instance.StopRunningCode();
                    ResetPlayerToStart();
                    return;
                }

                CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
                if (ore != null && ore.isMined && !ore.isCollected) playerValidCollects++;
            }
        }
        base.Collect();
    }

    private void CheckWinCondition()
    {
        TileManager_Training_5 tm = tileManager as TileManager_Training_5;
        if (tm == null) return;

        if (playerValidMines == tm.expectedMines && playerValidCollects == tm.expectedCollects)
        {
            PrintToDisplay("<color=green>Perfect! You filtered both ores perfectly using logic!</color>");
            ObjectiveManager.instance.TriggerCustomEvent("LevelSolved");
        }
        else
        {
            PrintToDisplay($"<color=orange>Incomplete! You missed some valid ores. You needed to mine {tm.expectedMines} and collect {tm.expectedCollects}.</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        playerValidMines = 0;
        playerValidCollects = 0;

        if (tileManager != null) tileManager.GenerateMap();
        if (cargoComponent != null) cargoComponent.cargoSize = cargoSize;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}