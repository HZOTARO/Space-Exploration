using UnityEngine;

public class GameManager_Puzzle : GameManager
{
    protected override void StartValuesSetup()
    {
        base.StartValuesSetup();

        int levelSize = PlayerPrefs.GetInt("PuzzleSize", 11);
        levelWidth = levelSize;
        levelLength = levelSize;
        cargoSize = 0;

        string puzzleId = PlayerPrefs.GetString("PuzzleID", "Unknown Puzzle");

        Debug.Log($"<color=cyan>Loaded Puzzle: {puzzleId} ({levelWidth}x{levelLength})</color>");

        if (cargoComponent) cargoComponent.cargoSize = cargoSize;
    }

    public override void MoveForward()
    {
        base.MoveForward();
        CheckMazeObjective();
    }

    public override void MoveBackward()
    {
        base.MoveBackward();
        CheckMazeObjective();
    }

    private void CheckMazeObjective()
    {
        if (playerGridLoc.y >= 0 && playerGridLoc.y < tileManager.width &&
            playerGridLoc.x >= 0 && playerGridLoc.x < tileManager.length)
        {
            TileObject currentTile = tileManager.objectsArray[playerGridLoc.x, playerGridLoc.y];

            if (currentTile != null && currentTile.type == TileType.Goal)
            {
                Debug.Log("<color=green>Maze cleared successfully!</color>");

                string completedPuzzleId = PlayerPrefs.GetString("PuzzleID", "");
                if (!string.IsNullOrEmpty(completedPuzzleId))
                {
                    if (!SaveManager.saveData.levelCompleted.Contains(completedPuzzleId))
                    {
                        SaveManager.saveData.levelCompleted.Add(completedPuzzleId);
                    }
                }

                LevelComplete();
            }
        }
    }
}