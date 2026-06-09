public class GameManager_Training_6 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("scan", Scan);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);

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

        allowedSyntaxNodes.Remove("If");
        allowedSyntaxNodes.Remove("IfExp");

        customLevelErrors["If"] = "If statements are banned in this unstable environment! Use match/case instead.";
        customLevelErrors["IfExp"] = "If statements are banned in this unstable environment! Use match/case instead.";
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Unstable Environment! Use match/case to handle the random resources and collect them all in a single run.",
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
        if (tileManager.objectsArray[1, 0].type == TileType.Floor &&
            tileManager.objectsArray[0, 1].type == TileType.Floor)
        {
            PrintToDisplay("<color=green>Success! You successfully matched and handled both resources.</color>");
            ObjectiveManager.instance.TriggerCustomEvent("LevelSolved");
        }
        else
        {
            PrintToDisplay("<color=orange>Level failed! Make sure you collect both resources. Watch out for the Black Ore!</color>");
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