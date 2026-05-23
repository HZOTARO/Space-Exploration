using UnityEngine;

public class TileManager_Training_6 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        SetTile(0, 0, TileType.Floor);
        SetTile(1, 1, TileType.Wall, false);

        SetRandomResource(1, 0);
        SetRandomResource(0, 1);
    }

    private void SetRandomResource(int z, int x)
    {
        int rand = Random.Range(0, 3);

        if (rand == 0)
            SetTile(z, x, TileType.WhiteOre, asFloorToo: false);
        else if (rand == 1)
            SetTile(z, x, TileType.BlackOre, asFloorToo: false);
        else
            SetTile(z, x, TileType.PurpleEssence, asFloorToo: true);
    }
}