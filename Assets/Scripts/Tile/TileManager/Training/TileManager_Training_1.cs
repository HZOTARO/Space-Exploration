public class TileManager_Training_1 : TileManager_Training
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        base.GenerateMap(false);
    }
    protected override void GenerateMapContent()
    {
        SetTile(0, 0, TileType.Floor);
        SetTile(1, 0, TileType.Floor);
        SetTile(2, 0, TileType.Floor);
        SetTile(2, 1, TileType.Goal);
    }
}