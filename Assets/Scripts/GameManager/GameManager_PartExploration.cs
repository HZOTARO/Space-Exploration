using UnityEngine;
using System;

public class GameManager_PartExploration : GameManager
{
    public UpgradeSO mapSizeUpgrade;
    public UpgradeSO inventorySizeUpgrade;
    protected override void Start()
    {
        if (UpgradeManager.instance)
        {
            if (mapSizeUpgrade) levelSize = 5 * (UpgradeManager.instance.GetUpgradeLevel(mapSizeUpgrade.id) + 1);
            if (inventorySizeUpgrade) inventorySize = 4 + 2 * (UpgradeManager.instance.GetUpgradeLevel(inventorySizeUpgrade.id));
        }

        base.Start();
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        void Bind(string pyName, Action action) => PythonExecutor.instance.RegisterPythonFunction(pyName, action);

        Bind("collect", Collect);
        Bind("measure", Measure);
        Bind("wait", Wait);
    }

    public void Collect()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        AddToInventory(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=white>Collected {amountCollected} White Ore.</color>");
                    }
                });
            }
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (ore.isMined && !ore.isCollected)
            {
                player.PerformAction(PlayerAction.Collect, () =>
                {
                    int amountCollected = ore.Collect();
                    if (amountCollected > 0)
                    {
                        AddToInventory(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");
                    }
                });
            }
        }
    }

    public void Wait()
    {
        // Tell the player to skip a turn while the enemy patrols
        //player.PerformAction(PlayerAction.Idle, () => Debug.Log("Player waited for 1 turn."));
    }
}