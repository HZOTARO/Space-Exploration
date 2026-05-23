public class TileManager_Training_9 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }

    protected override void GenerateMapContent()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                SetTile(z, x, TileType.Floor, asFloorToo: true);
            }
        }
    }
}