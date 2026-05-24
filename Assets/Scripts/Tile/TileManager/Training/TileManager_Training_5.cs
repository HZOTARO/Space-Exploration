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