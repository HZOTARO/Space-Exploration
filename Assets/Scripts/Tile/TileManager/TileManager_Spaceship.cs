using UnityEngine;

public class TileManager_Spaceship : TileManager
{
    [Header("Spaceship Floor Settings")]
    public BaseTile floorEven;
    public BaseTile floorOdd;

    protected override void GenerateFloorTile(int z, int x, TileObject currentFloorData)
    {
        if (floorEven != null && floorOdd != null)
        {
            BaseTile prefabToUse = ((x + z) % 2 == 0) ? floorEven : floorOdd;
            currentFloorData.tileInstance = InstantiateTileVisual(z, x, prefabToUse);
            currentFloorData.type = TileType.Floor;
        }
    }
}