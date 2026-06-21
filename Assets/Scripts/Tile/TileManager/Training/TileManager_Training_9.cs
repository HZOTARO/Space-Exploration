using System.Collections.Generic;
using UnityEngine;

public class TileManager_Training_9 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                SetTile(z, x, TileType.Floor, asFloorToo: true);
            }
        }

        List<Vector2Int> availableSpaces = new List<Vector2Int>();

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                availableSpaces.Add(new Vector2Int(z, x));
            }
        }

        int oreCount = Random.Range(15, 26);
        for (int i = 0; i < oreCount; i++)
        {
            int randomIndex = Random.Range(0, availableSpaces.Count);
            Vector2Int randomPosition = availableSpaces[randomIndex];
            availableSpaces.RemoveAt(randomIndex);
            SetTile(randomPosition.x, randomPosition.y, TileType.WhiteOre, asFloorToo: false);
        }
    }
}