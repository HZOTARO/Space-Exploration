using UnityEngine;

public class GameManager_Training_8 : GameManager_Training
{
    public int randomizedOreCount;
    public int targetTotalValue;
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);

        Bind("mine", Mine);
        Bind("collect", Collect);
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();

        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Lists);
    }

    protected override void StartValuesSetup()
    {
        levelLength = 1;
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        levelWidth = Random.Range(30, 51);
        randomizedOreCount = Random.Range(6, 11);

        cargoSize = randomizedOreCount;

        TileManager_Training_8 tm = FindFirstObjectByType<TileManager_Training_8>();
        if (tm != null)
        {
            tm.numberOfOres = randomizedOreCount;
        }

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = $"There are {randomizedOreCount} ores. Collect them all and append their values to your 'inventory' list!",
            type = ObjectiveType.CustomEvent,
            customEventId = "TotalCalculated"
        });
    }

    protected override void Start()
    {
        base.Start();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished += CheckInventoryTotal;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
        }
    }

    private void CheckInventoryTotal()
    {
        bool createdList = PythonExecutor.instance.CheckASTPattern(1, 999, "AssignList", "inventory");
        if (!createdList)
        {
            PrintToDisplay("<color=red>Error: You must create a list named 'inventory'!</color>");
            ResetPlayerToStart();
            return;
        }

        bool usedAppend = PythonExecutor.instance.CheckASTPattern(1, 999, "HasListAppend", "");
        if (!usedAppend)
        {
            PrintToDisplay("<color=red>Error: You must use the .append() function to add ores to your list!</color>");
            ResetPlayerToStart();
            return;
        }

        TileManager_Training_8 tm = tileManager as TileManager_Training_8;
        if (tm == null) return;

        string inventoryResult = PythonExecutor.instance.GetVariableValue("inventory");
        int playerTotal = 0;

        if (!string.IsNullOrEmpty(inventoryResult) && inventoryResult != "[]")
        {
            string cleanString = inventoryResult.Replace("[", "").Replace("]", "").Replace(" ", "");
            string[] stringValues = cleanString.Split(',');

            foreach (string val in stringValues)
            {
                if (int.TryParse(val, out int parsedVal))
                {
                    playerTotal += parsedVal;
                }
            }
        }

        if (playerTotal == tm.expectedTotalValue && cargoComponent.IsFull())
        {
            PrintToDisplay($"<color=green>Success! Your inventory total is exactly {playerTotal}, matching the map perfectly!</color>");
            ObjectiveManager.instance.TriggerCustomEvent("TotalCalculated");
            base.OnLevelComplete();
        }
        else
        {
            PrintToDisplay($"<color=red>Total Mismatch! The map had a total value of {tm.expectedTotalValue}, but your inventory sum is {playerTotal}. Did you miss an ore?</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        if (tileManager != null) tileManager.GenerateMap();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= CheckInventoryTotal;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }
}
