using UnityEngine;

public class TileManager_Training_4 : TileManager_Training
{
    [HideInInspector] public int goalX = 100;
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        for (int x = 0; x < width - 1; x++)
        {
            SetTile(0, x, TileType.Floor, asFloorToo: true);
        }

        SetTile(0, goalX, TileType.Goal, asFloorToo: true);
    }
}