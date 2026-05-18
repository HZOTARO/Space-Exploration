using UnityEngine;

public class GameManager_Puzzle : GameManager
{
    protected override void StartValuesSetup()
    {
        base.StartValuesSetup();

        levelWidth = Random.Range(5, 9) * 2 + 1;
        levelLength = Random.Range(5, 9) * 2 + 1;

        if (cargoComponent) cargoComponent.cargoSize = cargoSize;
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);

        if (UpgradeManager.instance)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("scan")) BindReturn("scan", Scan);
            if (UpgradeManager.instance.IsUpgradeUnlocked("measure")) BindReturn("measure", Measure);
        }
    }

    public override bool MoveForward()
    {
        bool result = base.MoveForward();
        CheckMazeObjective();
        return result;
    }

    public override bool MoveBackward()
    {
        bool result = base.MoveBackward();
        CheckMazeObjective();
        return result;
    }

    private void CheckMazeObjective()
    {
        if (playerGridLoc.x >= 0 && playerGridLoc.x < tileManager.width &&
            playerGridLoc.y >= 0 && playerGridLoc.y < tileManager.length)
        {
            TileObject currentTile = tileManager.objectsArray[playerGridLoc.y, playerGridLoc.x];

            if (currentTile.type == TileType.Goal)
            {
                Debug.Log("<color=green>Maze cleared successfully!</color>");
                LevelComplete();
            }
        }
    }
}