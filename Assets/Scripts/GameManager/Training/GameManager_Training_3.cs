public class GameManager_Training_3 : GameManager_Training
{
    // Measure both ores and add value
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindWithArg<string>("turn", Turn);
        BindReturn("measure", Measure);
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckCalculatedTotal;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
        }
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();

        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use measure() on both ores and store their sum in a variable named 'total' in a single run.",
            type = ObjectiveType.CustomEvent,
            customEventId = "TotalCalculated"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 2;
        cargoSize = 0;
    }

    private void CheckCalculatedTotal()
    {
        TileManager_Training_3 tm = tileManager as TileManager_Training_3;
        if (tm == null) return;

        if (PythonExecutor.instance != null)
        {
            string totalStr = PythonExecutor.instance.GetVariableValue("total");

            if (totalStr == "Undefined")
            {
                PrintToDisplay("<color=red>Error: Variable 'total' was not found! Make sure you named it exactly 'total'.</color>");
                ResetPlayerToStart();
            }
            else if (int.TryParse(totalStr, out int playerTotal))
            {
                if (playerTotal == tm.expectedTotalValue)
                {
                    PrintToDisplay($"<color=green>Correct! You successfully calculated the total: {playerTotal}</color>");
                    ObjectiveManager.instance.TriggerCustomEvent("TotalCalculated");
                }
                else
                {
                    PrintToDisplay($"<color=red>Incorrect Math! The real total was {tm.expectedTotalValue}, but your 'total' variable was {playerTotal}.</color>");
                    ResetPlayerToStart();
                }
            }
            else
            {
                PrintToDisplay($"<color=red>Error: 'total' is not a valid number!</color>");
                ResetPlayerToStart();
            }
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        if (tileManager != null)
        {
            tileManager.GenerateMap();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckCalculatedTotal;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }
}