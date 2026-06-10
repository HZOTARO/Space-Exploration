
public class GameManager_Training_4 : GameManager_Training
{
    // Only mine and collect inside if white ore
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
            description = "Use mine() inside an 'if scan() == \"WhiteOre\"' block.",
            type = ObjectiveType.CustomEvent,
            customEventId = "MineInsideIf"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use collect() inside an 'if scan() == \"WhiteOre\"' block.",
            type = ObjectiveType.CustomEvent,
            customEventId = "CollectInsideIf"
        });
    }

    protected override void StartValuesSetup()
    {
        levelLength = 2;
        levelWidth = 1;
        cargoSize = 1;
    }

    public override void Mine()
    {
        if (PythonExecutor.instance != null)
        {
            if (!ValidateFunctionCallCount("mine", 1, true)) return;
            bool usedInsideIf = PythonExecutor.instance.CheckASTPattern(1, 999, "FuncInsideIfWhiteOre", "mine");

            if (usedInsideIf)
            {
                ObjectiveManager.instance.TriggerCustomEvent("MineInsideIf");
                base.Mine();
            }
            else
            {
                PrintToDisplay("<color=red>Error: You must use mine() inside an 'if' block checking for 'WhiteOre'!</color>");

                PythonExecutor.instance.StopRunningCode();

                return;
            }
        }
        else
        {
            base.Mine();
        }
    }

    public override void Collect()
    {
        if (PythonExecutor.instance != null)
        {
            if (!ValidateFunctionCallCount("collect", 1, true)) return;
            bool usedInsideIf = PythonExecutor.instance.CheckASTPattern(1, 999, "FuncInsideIfWhiteOre", "collect");

            if (usedInsideIf)
            {
                ObjectiveManager.instance.TriggerCustomEvent("CollectInsideIf");
                base.Collect();
            }
            else
            {
                PrintToDisplay("<color=red>Error: You must use collect() inside an 'if' block checking for 'WhiteOre'!</color>");
                PythonExecutor.instance.StopRunningCode();

                return;
            }
        }
        else
        {
            base.Collect();
        }
    }
}