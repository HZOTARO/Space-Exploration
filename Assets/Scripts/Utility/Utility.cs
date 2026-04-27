using UnityEngine;

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right
}

public enum ResourceType
{
    None,
    WhiteOre,
    BlackOre,
    PurpleLiquid,
    PartA,
    PartB,
    PartC
}

public enum TileType
{
    None,
    Floor, // Floor
    WhiteOre,
    PurpleVein,
    BlackOre,
}

public enum PlayerAction
{
    Mine,
    Collect,
    Purify,
    Drill,
    Pump
}

[System.Serializable]
public struct ResourceImage
{
    public ResourceType type;
    public Sprite image;
}