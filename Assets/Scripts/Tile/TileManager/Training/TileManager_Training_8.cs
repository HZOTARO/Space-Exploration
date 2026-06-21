using System.Collections.Generic;
using UnityEngine;

public class TileManager_Training_8 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        int goalX = Random.Range(width / 2, width);

        for (int x = 0; x < width; x++)
        {
            SetTile(0, x, TileType.Floor, asFloorToo: true);
        }

        SetTile(0, goalX, TileType.Goal, false);


        int numberOfOres = Random.Range(4, 9);

        List<int> availableSpaces = new List<int>();
        for (int i = 1; i < width - 1; i++)
        {
            if (i == goalX) break;
            availableSpaces.Add(i);
        }

        for (int i = 0; i < numberOfOres; i++)
        {
            int choosenIndex = Random.Range(0, availableSpaces.Count);
            int x = availableSpaces[choosenIndex];

            SetTile(0, x, TileType.WhiteOre, asFloorToo: false);

            availableSpaces.RemoveAt(choosenIndex);
        }
    }
}