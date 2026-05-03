using UnityEngine;

public class TileManager_PartExploration : TileManager_Cave
{
    [Header("Objective Items")]
    [Tooltip("The specific spaceship part the player needs to find")]
    public TileType spaceshipPartType;
    public int numberOfPartsToFind = 3;

    [Header("Cave Clutter (Obstacles)")]
    [Tooltip("Rocks, stalagmites, or rubble to block the player's path")]
    public TileType rubbleTileType;
    public int numberOfRubble = 20;

    // We ONLY override the math hook! The base Cave script handles the arrays and visuals.
    protected override void GenerateMapContent()
    {
        // 1. Hide the spaceship parts randomly around the cave
        ScatterTiles(spaceshipPartType, numberOfPartsToFind);

        // 2. Scatter rocks and rubble to make it feel like a messy, natural cave
        ScatterTiles(rubbleTileType, numberOfRubble);
    }

    // Our trusty helper method to safely drop items into the arrays
    private void ScatterTiles(TileType typeToPlace, int amount)
    {
        int failsafe = 0; // Anti-freeze protection
        while (amount > 0 && failsafe < 1000)
        {
            failsafe++;
            Vector2Int pos = GetRandomEmptySpot();

            // If the cave is completely full, stop trying to place things
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

            // Ensure we only place things on completely empty dirt floor tiles!
            // (And optionally protect 0,0 if that is your player spawn)
            if (objectsArray[randZ, randX].type == TileType.Floor && !(randZ == 0 && randX == 0))
            {
                return new Vector2Int(randX, randZ);
            }
        }
        return new Vector2Int(-1, -1); // Return this if we couldn't find an empty spot
    }
}