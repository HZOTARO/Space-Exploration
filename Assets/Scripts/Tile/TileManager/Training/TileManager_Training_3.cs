using UnityEngine;

public class TileManager_Training_3 : TileManager_Training
{
    [HideInInspector] public int expectedTotalValue;

    public override void GenerateMap(bool setAllFloor = true)
    {
        expectedTotalValue = 0;

        base.GenerateMap(false);

        TileObject ore1 = objectsArray[1, 0];
        TileObject ore2 = objectsArray[0, 1];

        if (ore1 != null && ore1.tileInstance is ValueTile valueTile1)
        {
            valueTile1.notRandomized = true;
            valueTile1.value = Random.Range(10, 50);
            expectedTotalValue += valueTile1.value;
        }

        if (ore2 != null && ore2.tileInstance is ValueTile valueTile2)
        {
            valueTile2.notRandomized = true;
            valueTile2.value = Random.Range(10, 50);
            expectedTotalValue += valueTile2.value;
        }
    }

    protected override void GenerateMapContent()
    {
        SetTile(0, 0, TileType.Floor);
        SetTile(1, 0, TileType.WhiteOre, false);
        SetTile(0, 1, TileType.WhiteOre, false);
        SetTile(1, 1, TileType.Wall, false);
    }
}