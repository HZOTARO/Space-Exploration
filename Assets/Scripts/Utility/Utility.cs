using UnityEngine;

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right
}

public enum TileType
{
    Default, // Floor
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