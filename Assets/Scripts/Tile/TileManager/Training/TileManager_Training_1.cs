public class TileManager_Training_1 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(true);
    }
    protected override void GenerateMapContent()
    {
        SetTile(2, 1, TileType.Goal);
    }
}