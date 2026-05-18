public class GameManager_Training_1 : GameManager_Training
{
    // Introduction
    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
    }

    public override bool MoveForward()
    {
        bool result = base.MoveForward();
        CheckGoal();
        return result;
    }

    public override bool MoveBackward()
    {
        bool result = base.MoveBackward();
        CheckGoal();
        return result;
    }

    private void CheckGoal()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile != null && currentTile.type == TileType.Goal)
        {
            ObjectiveManager.instance.TriggerCustomEvent("ReachedGoal");
        }
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
        levelLength = 3;
        levelWidth = 2;
        cargoSize = 0;
    }
}
