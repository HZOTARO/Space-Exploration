public class GameManager_Training_1 : GameManager_Training
{
    // Introduction
    protected override void RegisterLevelSpecificPythonCommands()
    {
        Bind("move_forward", MoveForward);
        Bind("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
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
            description = "Move the player using move_forward().",
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
