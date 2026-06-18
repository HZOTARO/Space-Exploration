using UnityEngine;

public class GameManager_Training_2 : GameManager_Training
{
    // Variable, scan and print
    [Header("Level Specifics")]
    public string expectedScanResult;

    protected override void Start()
    {
        base.Start();

        if (tileManager != null && tileManager.objectsArray != null)
        {
            TileObject targetTile = tileManager.objectsArray[1, 0];
            if (targetTile != null)
            {
                expectedScanResult = targetTile.type.ToString();
            }
        }

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnPythonPrint += CheckPlayerPrint;
        }
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();
        BindReturn("scan", Scan);
    }

    private void CheckPlayerPrint(string printedMessage)
    {
        if (printedMessage == expectedScanResult)
        {
            bool usedVariableCorrectly = PythonExecutor.instance != null && PythonExecutor.instance.CheckASTPattern(1, 999, "ScanAndPrintVar", "");

            if (usedVariableCorrectly)
            {
                ObjectiveManager.instance.TriggerCustomEvent("PrintedVariable");
            }
            else
            {
                PythonExecutor.instance.TriggerRuntimeError("Save scan() to a variable first, then print it! Example: \ntile = scan() \nprint(tile)", true);
            }
        }
    }

    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);

        //allowedSyntaxNodes.Remove("Constant");
        //allowedSyntaxNodes.Remove("Num");
        //allowedSyntaxNodes.Remove("Str");

        //customLevelErrors["Constant"] = "You cannot type numbers or strings directly! Use scan() instead.";
        //customLevelErrors["Num"] = "You cannot type numbers in this level!";
        //customLevelErrors["Str"] = "You cannot type strings directly! Use scan() instead.";
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use <color=green>scan</color>() to check the tile in front of you.",
            type = ObjectiveType.FunctionCall,
            targetFunctionName = "scan"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Store the <color=green>scan</color>() result of White Ore in a variable and display it using <color=#F5F5AB>print</color>().",
            type = ObjectiveType.CustomEvent,
            customEventId = "PrintedVariable"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 1;
        cargoSize = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnPythonPrint -= CheckPlayerPrint;
        }
    }
}