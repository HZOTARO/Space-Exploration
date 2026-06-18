public class GameManager_Training_5 : GameManager_Training
{
    // Get higher valued mineral
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);

        BindWithArg<string>("turn", Turn);

        Bind("mine", Mine);
        Bind("collect", Collect);
        Bind("drill", Drill);
        Bind("pump", Pump);
        Bind("purify", Purify);
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
            description = "Unstable Environment! Measure both minerals and collect the one with the higher value in a single run!",
            type = ObjectiveType.CustomEvent,
            customEventId = "LevelSolved"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 2;
        cargoSize = 1;
    }

    protected override void Start()
    {
        base.Start();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionStarted += RandomizeUnstableEnvironment;
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
    }

    public void RandomizeUnstableEnvironment()
    {
        PrintToDisplay("<color=yellow>Warning: Unstable Environment shifting...</color>");
        if (tileManager != null) tileManager.GenerateMap();
    }

    private void CheckWinCondition()
    {
        TileManager_Training_5 tm = tileManager as TileManager_Training_5;
        if (tm == null || cargoComponent == null) return;

        if (cargoComponent.levelCargo.Count > 0 && cargoComponent.levelCargo[0].item != null)
        {
            int collectedValue = cargoComponent.levelCargo[0].amount;

            if (collectedValue == tm.highestValue)
            {
                PrintToDisplay($"<color=green>Success! You safely extracted the highest value resource: {collectedValue}!</color>");
                ObjectiveManager.instance.TriggerCustomEvent("LevelSolved");
            }
            else
            {
                PrintToDisplay($"<color=orange>Your collected value {collectedValue}, but the better resource value was {tm.highestValue}!</color>");
                ResetPlayerToStart();
            }
        }
        else
        {
            PrintToDisplay("<color=orange>You didn't collect any resources!</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        RandomizeUnstableEnvironment();

        if (cargoComponent != null) cargoComponent.cargoSize = cargoSize;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionStarted -= RandomizeUnstableEnvironment;
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
        }
    }
}