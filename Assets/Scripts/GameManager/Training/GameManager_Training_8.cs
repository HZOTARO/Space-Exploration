using System.Collections.Generic;
using UnityEngine;

public class GameManager_Training_8 : GameManager_Training
{
    public int randomizedOreCount;
    public int targetTotalValue;

    private List<int> cargoValues = new List<int>();

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
        BindWithArg<int, bool>("discard", Discard);
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
        RandomizeMapData();
    }

    private void RandomizeMapData()
    {
        levelWidth = Random.Range(30, 51);
        
        randomizedOreCount = Random.Range(6, 11) * 2;
        cargoSize = randomizedOreCount / 2;

        TileManager_Training_8 tm = tileManager as TileManager_Training_8;
        if (tm != null)
        {
            tm.numberOfOres = randomizedOreCount;
        }
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = $"Measure the ores and collect the most valuable ones on the path!",
            type = ObjectiveType.CustomEvent,
            customEventId = "TotalCalculated"
        });
    }

    protected override void Start()
    {
        base.Start(); 

        SetupCargoUI();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore += CheckInventoryTotal;
            PythonExecutor.instance.OnExecutionAborted += HandleAbort;
            PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;
        }
    }

    private void SetupCargoUI()
    {
        if (cargoComponent != null)
        {
            cargoComponent.cargoSize = cargoSize;
            StartCoroutine(cargoComponent.SetupCargoCoroutine());
        }
    }

    private void CheckInventoryTotal()
    {
        TileManager_Training_8 tm = tileManager as TileManager_Training_8;
        if (tm == null || cargoComponent == null) return;

        int physicalCargoTotal = 0;
        string cargoContents = "Cargo Contents:\n";
        foreach (ItemAmount collected in cargoComponent.levelCargo)
        {
            physicalCargoTotal += collected.amount;
            cargoContents += $" {collected.amount},";
        }
        Debug.Log(cargoContents);

        if (cargoComponent.IsFull() && physicalCargoTotal == tm.expectedTotalValue)
        {
            PrintToDisplay($"<color=green>Success! You collected the most valuable ores with a total value of {physicalCargoTotal}!</color>");
            ObjectiveManager.instance.TriggerCustomEvent("TotalCalculated");
        }
        else
        {
            if (!cargoComponent.IsFull())
            {
                PrintToDisplay($"<color=red>Incomplete! Your cargo hold is not full. You must fill your inventory with the best ores!</color>");
            }
            else
            {
                PrintToDisplay($"<color=red>Total Mismatch! You collected a total value of {physicalCargoTotal}, but the top ores had a value of {tm.expectedTotalValue}. You picked up a lower value item!</color>");
            }
            ResetPlayerToStart();
        }
    }

    protected override void ResetPlayerToStart()
    {
        base.ResetPlayerToStart();

        if (tileManager != null)
        {
            RandomizeMapData();
            tileManager.GenerateMap();
        }

        SetupCargoUI();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinishedBefore -= CheckInventoryTotal;
            PythonExecutor.instance.OnExecutionAborted -= HandleAbort;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }
}