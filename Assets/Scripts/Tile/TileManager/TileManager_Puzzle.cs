using UnityEngine;
using System.Collections.Generic;

public class TileManager_Puzzle : TileManager_Cave
{
    [Header("Maze Settings")]
    public Vector2Int goalCoordinate;
    public override void GenerateMap(bool setAllfloor = true)
    {
        if (width % 2 == 0) width++;
        if (length % 2 == 0) length++;

        base.GenerateMap(setAllfloor);
    }

    protected override void GenerateMapContent()
    {
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                objectsArray[z, x].type = TileType.Wall;
            }
        }

        CarveMazeDFS(0, 0);

        PlaceGoalTile();
    }

    private void CarveMazeDFS(int startZ, int startX)
    {
        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        Vector2Int currentLoc = new Vector2Int(startX, startZ);

        objectsArray[currentLoc.y, currentLoc.x].type = TileType.Floor;

        List<Vector2Int> neighbors = new List<Vector2Int>();

        while (true)
        {
            neighbors.Clear();

            Vector2Int[] directions = { new Vector2Int(0, 2), new Vector2Int(0, -2), new Vector2Int(2, 0), new Vector2Int(-2, 0) };
            foreach (Vector2Int dir in directions)
            {
                Vector2Int targetLoc = currentLoc + dir;

                if (targetLoc.x >= 0 && targetLoc.x < width && targetLoc.y >= 0 && targetLoc.y < length)
                {
                    if (objectsArray[targetLoc.y, targetLoc.x].type == TileType.Wall)
                    {
                        neighbors.Add(targetLoc);
                    }
                }
            }

            if (neighbors.Count > 0)
            {
                Vector2Int targetLoc = neighbors[Random.Range(0, neighbors.Count)];

                Vector2Int connectingLoc = currentLoc + (targetLoc - currentLoc) / 2;

                objectsArray[connectingLoc.y, connectingLoc.x].type = TileType.Floor;

                objectsArray[targetLoc.y, targetLoc.x].type = TileType.Floor;

                pathStack.Push(currentLoc);
                currentLoc = targetLoc;
            }

            else if (pathStack.Count > 0)
            {
                currentLoc = pathStack.Pop();
            }

            else
            {
                break;
            }
        }
    }

    private void PlaceGoalTile()
    {
        List<Vector2Int> validGoals = new List<Vector2Int>();
        List<Vector2Int> backupGoals = new List<Vector2Int>();

        float minX = width * 0.75f;
        float minZ = length * 0.75f;

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (objectsArray[z, x].type == TileType.Floor)
                {
                    if (x == 0 && z == 0) continue;

                    Vector2Int pos = new Vector2Int(x, z);
                    backupGoals.Add(pos);

                    if (x >= minX || z >= minZ)
                    {
                        validGoals.Add(pos);
                    }
                }
            }
        }

        Vector2Int chosenGoal;

        if (validGoals.Count > 0)
        {
            chosenGoal = validGoals[Random.Range(0, validGoals.Count)];
        }
        else if (backupGoals.Count > 0)
        {
            chosenGoal = backupGoals[Random.Range(0, backupGoals.Count)];
        }
        else
        {
            return;
        }

        objectsArray[chosenGoal.y, chosenGoal.x].type = TileType.Goal;
        goalCoordinate = chosenGoal;
    }
}