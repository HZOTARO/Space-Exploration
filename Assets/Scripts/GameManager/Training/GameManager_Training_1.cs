public class GameManager_Training_1 : GameManager_Training
{
    // Introduction
    protected override void Start()
    {
        base.Start();
        OnSuccessfulMove += CheckGoal;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnSuccessfulMove -= CheckGoal;
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
            description = "Move the player using move(direction, distance).",
            type = ObjectiveType.FunctionCall,
            targetFunctionName = "move"
        });

        ObjectiveManager.instance.objectives.Add(new LevelObjective()
        {
            description = "Reach the goal marked in green!",
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
