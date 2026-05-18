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
}
