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