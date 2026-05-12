public class TileManager_Training : TileManager_Cave
{
    protected void SetTile(int z, int x, TileType type, bool asFloorToo = true)
    {
        if (x < 0 || x >= width || z < 0 || z >= length)
        {
            return;
        }
        objectsArray[z, x].type = type;

        if (asFloorToo)
        {
            floorArray[z, x].type = type;
        }
        else
        {
            floorArray[z, x].type = TileType.Floor;
        }
    }
}