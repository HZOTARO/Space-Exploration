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
    None,
    Floor,
    WhiteOre,
    PurpleVein,
    BlackOre,
    Goal,
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
public struct ItemAmount
{
    public ItemSO item;
    public int amount;
}