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
public struct ItemCost
{
    public ItemSO item;
    public int amount;
}