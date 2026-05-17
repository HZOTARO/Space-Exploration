using UnityEngine;

public class GameManager_Training_3 : GameManager_Training
{
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("scan", Scan);
        Bind("mine", Mine);
        Bind("collect", Collect);
    }
    protected override void SetLevelAllowedSyntax()
    {
        base.SetLevelAllowedSyntax();

        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use mine() function inside an 'if == \"WhiteOre\"' block.",
            type = ObjectiveType.CustomEvent,
            customEventId = "MineInsideIf"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use collect() function inside that same 'if' block.",
            type = ObjectiveType.CustomEvent,
            customEventId = "CollectInsideIf"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 1;
        cargoSize = 2;
    }

    private void Mine()
    {
        int startLine = 1;
        int endLine = 1;

        if (PythonExecutor.instance != null && !string.IsNullOrEmpty(PythonExecutor.instance.currentCode))
        {
            string[] codeLines = PythonExecutor.instance.currentCode.Split('\n');
            endLine = Mathf.Max(1, codeLines.Length);
        }

        bool passesCheck = PythonExecutor.instance.CheckASTPattern(startLine, endLine, "FuncInsideIfWhiteOre", "mine");

        if (passesCheck)
        {
            ObjectiveManager.instance.TriggerCustomEvent("MineInsideIf");
            PrintToDisplay("Robot action: Excavating target ore node material...");
        }
        else
        {
            PrintToDisplay("Action Blocked! You must verify the tile equals \"WhiteOre\" using an if statement before mining.");
        }
    }

    private void Collect()
    {
        bool passesCheck = PythonExecutor.instance.CheckASTPattern(1, 999, "FuncInsideIfWhiteOre", "collect");

        if (passesCheck)
        {
            ObjectiveManager.instance.TriggerCustomEvent("CollectInsideIf");
            PrintToDisplay("Robot action: Securing processed item container into storage cargo slot...");
        }
        else
        {
            PrintToDisplay("Action Blocked! You must verify the tile equals \"WhiteOre\" using an if statement before collecting.");
        }
    }
}
