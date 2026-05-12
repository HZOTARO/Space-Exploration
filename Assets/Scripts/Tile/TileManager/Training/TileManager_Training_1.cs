public class TileManager_Training_1 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }
    protected override void GenerateMapContent()
    {
        objectsArray[0, 0].type = TileType.Floor;
        objectsArray[1, 0].type = TileType.Floor;
        objectsArray[2, 0].type = TileType.Floor;
        objectsArray[2, 1].type = TileType.Goal;
        floorArray[2, 1].type = TileType.Goal;
    }
}