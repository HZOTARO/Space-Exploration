using UnityEngine;
using System.Collections.Generic;

public class TileManager_Puzzle : TileManager_Cave
{
    [Header("Maze Settings")]
    public Vector2Int goalCoordinate;
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
        Stack<Vector2Int> cellStack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(startX, startZ);

        objectsArray[current.y, current.x].type = TileType.Floor;

        List<Vector2Int> neighbors = new List<Vector2Int>();

        int failsafe = 0;
        while (failsafe < 5000)
        {
            failsafe++;
            neighbors.Clear();

            Vector2Int[] directions = { new Vector2Int(0, 2), new Vector2Int(0, -2), new Vector2Int(2, 0), new Vector2Int(-2, 0) };
            foreach (var dir in directions)
            {
                Vector2Int check = current + dir;
                if (check.x >= 0 && check.x < width && check.y >= 0 && check.y < length)
                {
                    if (objectsArray[check.y, check.x].type == TileType.Wall)
                    {
                        neighbors.Add(check);
                    }
                }
            }

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];

                Vector2Int wallBetween = current + (chosen - current) / 2;
                objectsArray[wallBetween.y, wallBetween.x].type = TileType.Floor;

                objectsArray[chosen.y, chosen.x].type = TileType.Floor;

                cellStack.Push(current);
                current = chosen;
            }
            else if (cellStack.Count > 0)
            {
                current = cellStack.Pop();
            }
            else
            {
                break;
            }
        }
    }

    private void PlaceGoalTile()
    {
        for (int z = length - 1; z >= 0; z--)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                if (objectsArray[z, x].type == TileType.Floor)
                {
                    objectsArray[z, x].type = TileType.Goal;
                    floorArray[z, x].type = TileType.Goal;
                    goalCoordinate = new Vector2Int(x, z);
                    return;
                }
            }
        }
    }
}