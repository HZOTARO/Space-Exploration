using UnityEngine;

public class GameManager_Training_7 : GameManager_Training
{
    protected override void SetLevelAllowedSyntax()
    {
        allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
        //allowedSyntaxNodes.AddRange(SyntaxDictionary.Logic);
        //allowedSyntaxNodes.AddRange(SyntaxDictionary.Loops);
        //allowedSyntaxNodes.AddRange(SyntaxDictionary.Lists);
    }

    protected override void SetLevelObjectives()
    {
        base.SetLevelObjectives();

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Use move_forward() to move the player.",
            type = ObjectiveType.FunctionCall,
            targetFunctionName = "move_forward"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Reach the goal!",
            type = ObjectiveType.CustomEvent,
            customEventId = "ReachedGoal"
        });
    }

    protected override void StartValuesSetup()
    {
        //levelSize = 3;
        cargoSize = 4;
    }
}
