using UnityEngine;

public class TileManager_Training_5 : TileManager_Training
{
    [HideInInspector] public int highestValue;

    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);

        int val1 = Random.Range(1, 50);
        int val2 = Random.Range(1, 50);

        while (val1 == val2) val2 = Random.Range(1, 50);

        SetupOre(objectsArray[1, 0], val1);
        SetupOre(objectsArray[0, 1], val2);

        highestValue = Mathf.Max(val1, val2);
    }

    private void SetupOre(TileObject oreTile, int val)
    {
        if (oreTile != null && oreTile.tileInstance is ValueTile vTile)
        {
            vTile.notRandomized = true;
            vTile.value = val;
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