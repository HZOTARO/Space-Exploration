public class TileObject
{
    public TileType type;
    public BaseTile tileInstance;
}
[System.Serializable]
public struct TileReference
{
    public TileType type;
    public BaseTile tilePrefab;
    public bool spawnsWithFloor;
}