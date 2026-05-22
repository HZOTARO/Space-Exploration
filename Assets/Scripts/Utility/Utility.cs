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
    PurpleEssence,
    BlackOre,

    Enemy,
    EnemyPath,
    Gear,
    Screw,
    Wall,

    Goal,
}

public enum PlayerAction
{
    Mine,
    Collect,
    Purify,
    Drill,
    Pump,

    Shoot,
    Hurt,
    Death,
    DeathWaits
}

[System.Serializable]
public struct ItemAmount
{
    public ItemSO item;
    public int amount;
}