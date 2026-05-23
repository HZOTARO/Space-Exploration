using UnityEngine;

public class GameManager_Training_10 : GameManager_Training
{
    // Append List
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
        levelWidth = Random.Range(20, 31);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();
        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Create a list named 'inventory'. Measure all the White Ores and append their values to the list in a single run.",
            type = ObjectiveType.CustomEvent,
            customEventId = "ListCompleted"
        });
    }

    protected override void Start()
    {
        base.Start();

        TileManager_Training_10 tm = tileManager as TileManager_Training_10;
        if (tm != null)
        {
            cargoSize = tm.numberOfOres;

            if (cargoComponent != null)
            {
                cargoComponent.cargoSize = cargoSize;
                StartCoroutine(cargoComponent.SetupCargoCoroutine());
            }
        }

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckListCompletion;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
        }
    }

    private void CheckListCompletion()
    {
        bool createdList = PythonExecutor.instance.CheckASTPattern(1, 999, "AssignList", "inventory");
        if (!createdList)
        {
            PrintToDisplay("<color=red>Error: You must create a list named 'inventory' (e.g., inventory = [])</color>");
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

        TileManager_Training_10 tm = tileManager as TileManager_Training_10;
        if (tm == null) return;

        string inventoryResult = PythonExecutor.instance.GetVariableValue("inventory");
        string expectedListString = "[" + string.Join(", ", tm.expectedOreValues) + "]";

        if (inventoryResult == expectedListString)
        {
            PrintToDisplay($"<color=green>Perfect! Your list matched exactly: {expectedListString}</color>");
            ObjectiveManager.instance.TriggerCustomEvent("ListCompleted");
        }
        else
        {
            PrintToDisplay($"<color=red>Mismatch! Expected {expectedListString} but got {inventoryResult}. Resetting everything...</color>");
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        if (tileManager != null)
        {
            tileManager.GenerateMap();
        }

        TileManager_Training_10 tm = tileManager as TileManager_Training_10;
        if (tm != null)
        {
            cargoSize = tm.numberOfOres;

            if (cargoComponent != null)
            {
                cargoComponent.cargoSize = cargoSize;
                StartCoroutine(cargoComponent.SetupCargoCoroutine());
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckListCompletion;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }
}