using UnityEngine;

public class GameManager_Training_11 : GameManager_Training
{
    // Get highest value ores. It doesnt force list but very hard to do without
    public int randomizedOreCount;
    public int targetTotalValue;

    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();
        BindReturn("scan", Scan);
        BindReturn("measure", Measure);
        Bind("mine", Mine);
        Bind("collect", Collect);
        BindWithArg<int>("discard", Discard);
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

        TileManager_Training_11 tm = tileManager as TileManager_Training_11;
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
            description = "Measure the ores and collect the most valuable ones along the path\n. Do it in a single run!",
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
        if (completed) return;

        TileManager_Training_11 tm = tileManager as TileManager_Training_11;
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
        if (completed) return;

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
        }
    }
}