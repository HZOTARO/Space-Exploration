using UnityEngine;

public class TileManager_Training_4 : TileManager_Training
{
    [HideInInspector] public int goalX = 3;
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

        SetTile(0, goalX, TileType.Goal, false);
    }
}