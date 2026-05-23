using UnityEngine;

public class TileManager_Training_5 : TileManager_Training
{
    [HideInInspector] public int expectedMines;
    [HideInInspector] public int expectedCollects;

    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);

        expectedMines = 0;
        expectedCollects = 0;

        TileObject ore1 = objectsArray[1, 0];
        TileObject ore2 = objectsArray[0, 1];

        SetupOre(ore1);
        SetupOre(ore2);
    }

    private void SetupOre(TileObject oreTile)
    {
        if (oreTile != null && oreTile.tileInstance is ValueTile vTile)
        {
            vTile.notRandomized = true;
            vTile.value = Random.Range(1, 16);

            if (vTile.value > 5) expectedMines++;
            if (vTile.value > 10) expectedCollects++;
        }
    }

    protected override void GenerateMapContent()
    {
        SetTile(0, 0, TileType.Floor);
        SetTile(1, 1, TileType.Wall, false);

        SetTile(1, 0, TileType.WhiteOre, asFloorToo: false);
        SetTile(0, 1, TileType.WhiteOre, asFloorToo: false);
    }
}