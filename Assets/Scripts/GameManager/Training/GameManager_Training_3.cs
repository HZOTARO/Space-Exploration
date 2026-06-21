public class GameManager_Training_3 : GameManager_Training
{
    // Measure both ores and add value
    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckCalculatedTotal;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
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
            description = "Use measure() to find the value of the tile in front of you.",
            type = ObjectiveType.FunctionCall,
            targetFunctionName = "measure"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Calculate the sum of the measured ores and store the result in a variable named total.\nDo it in a single run.",
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
                PrintToDisplay("<color=yellow>Variable 'total' was not found! Make sure you named it exactly 'total'.</color>");
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
                    PrintToDisplay($"<color=yellow>Incorrect value! The real total was {tm.expectedTotalValue}, but your 'total' variable was {playerTotal}.</color>");
                    ResetPlayerToStart();
                }
            }
            else
            {
                PrintToDisplay($"<color=yellow>'total' is not a valid number!</color>");
                ResetPlayerToStart();
            }
        }
    }

    protected override void ResetPlayerToStart()
    {
        if (completed) return;

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
        }
    }
}