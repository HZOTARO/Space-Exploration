using System;
using UnityEngine;

public class GameManager_Training_1 : GameManager_Training
{
    protected override void Awake()
    {
        base.Awake();
        trainingId = "training_1";
    }

    public override void MoveForward()
    {
        base.MoveForward();
        CheckGoal();
    }

    public override void MoveBackward()
    {
        base.MoveBackward();
        CheckGoal();
    }

    private void CheckGoal()
    {
        if (GetCurrentTile().type == TileType.Goal)
        {
            ObjectiveManager.instance.TriggerCustomEvent("ReachedGoal");
        }
    }

    protected override void SetLevelAllowedSyntax()
    {
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
        levelSize = 3;
        cargoSize = 0;
    }
}
