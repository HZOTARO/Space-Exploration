public class GameManager_Training_5 : GameManager_Training
{
    // Get higher valued ore
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
            description = "Measure both ores and collect the one with the higher value in a single run!",
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
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckWinCondition;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
        }
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
                PrintToDisplay($"<color=green>Success! You collected the highest value ore: {collectedValue}!</color>");
                ObjectiveManager.instance.TriggerCustomEvent("LevelSolved");
            }
            else
            {
                PrintToDisplay($"<color=orange>You collected value {collectedValue}, but the better ore was {tm.highestValue}!</color>");
                ResetPlayerToStart();
            }
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

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