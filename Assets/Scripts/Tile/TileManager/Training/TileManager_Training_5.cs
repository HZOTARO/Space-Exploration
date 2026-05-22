using UnityEngine;

public class TileManager_Training_5 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        for (int x = 0; x < width; x++)
        {
            SetTile(0, x, TileType.Floor, asFloorToo: true);
        }

        int goalX = Random.Range(width / 2, width);

        SetTile(0, goalX, TileType.Goal, asFloorToo: false);
    }
}