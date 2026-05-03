using UnityEngine;

public class TileManager_Puzzle : TileManager_Spaceship
{
    [Header("Objective Settings")]
    public TileType goalTileType; // E.g., an Exit Door or Computer Terminal
    public Vector2Int goalLocation = new Vector2Int(9, 9); // Top Right corner

    public TileType playerStartTileType; // Optional: A visual marker for where the robot spawns
    public Vector2Int playerStartLocation = new Vector2Int(0, 0); // Bottom Left corner

    [Header("Obstacles (The Maze)")]
    public TileType wallTileType;
    public int numberOfWalls = 15;

    [Header("Interactables")]
    public TileType pushableCrateType;
    public int numberOfCrates = 3;

    // We only override the math hook! The base script handles the arrays and visuals.
    protected override void GenerateMapContent()
    {
        // 1. Place the fixed Objective and Start points
        objectsArray[goalLocation.y, goalLocation.x].type = goalTileType;
        objectsArray[playerStartLocation.y, playerStartLocation.x].type = playerStartTileType;

        // 2. Generate the walls to create a maze
        ScatterTiles(wallTileType, numberOfWalls);

        // 3. Scatter crates that the player might have to push or destroy
        ScatterTiles(pushableCrateType, numberOfCrates);
    }

    // A helper method so we don't have to copy/paste the while-loop twice!
    private void ScatterTiles(TileType typeToPlace, int amount)
    {
        int failsafe = 0;
        while (amount > 0 && failsafe < 1000)
        {
            failsafe++;
            Vector2Int pos = GetRandomEmptySpot();

            // If the grid is completely full, stop trying to place things
            if (pos.x == -1) break;

            objectsArray[pos.y, pos.x].type = typeToPlace;
            amount--;
        }
    }

    private Vector2Int GetRandomEmptySpot()
    {
        for (int i = 0; i < 100; i++)
        {
            int randZ = Random.Range(0, length);
            int randX = Random.Range(0, width);

            // Ensure we only place things on completely empty floor tiles!
            if (objectsArray[randZ, randX].type == TileType.Floor)
            {
                return new Vector2Int(randX, randZ);
            }
        }
        return new Vector2Int(-1, -1);
    }
}