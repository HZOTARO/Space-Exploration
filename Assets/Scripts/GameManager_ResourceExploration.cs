using UnityEngine;
using System;

public class GameManager_ResourceExploration : GameManager
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

        Bind("mine", Mine);
        Bind("collect", Collect);
        Bind("purify", Purify);
        Bind("drill", Drill);
        Bind("pump", Pump);
        Bind("measure", Measure);
    }

    public void Mine()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("Nothing to mine. You are facing the edge of the map!");
            return;
        }

        if (targetTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = targetTile.tileInstance as CaveTile_WhiteOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => ore.Mine());
            else Debug.Log("This White Ore has already been mined.");
        }
        else if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => { if (ore.Mine()) DamagePlayer(60); });
            else Debug.Log("This Black Ore has already been mined.");
        }
        else
        {
            Debug.Log("No mineable resource in front of you.");
        }
    }

    public void Collect()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.WhiteOre)
        {
            CaveTile_WhiteOre ore = currentTile.tileInstance as CaveTile_WhiteOre;
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
        else if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
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

    public void Drill()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            if (!vein.isDrilled) player.PerformAction(PlayerAction.Drill, () => vein.Drill());
        }
    }

    public void Pump()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.PurpleVein)
        {
            CaveTile_PurpleVein vein = currentTile.tileInstance as CaveTile_PurpleVein;
            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        AddToInventory(vein.itemOnTile, amountPumped);
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                    }
                });
            }
        }
    }

    public void Purify()
    {
        TileObject currentTile = GetCurrentTile();
        if (currentTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = currentTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isPurified) player.PerformAction(PlayerAction.Purify, () => ore.Purify());
        }
    }

    public void Measure()
    {
        IMeasureable measureableTile = GetCurrentTile().tileInstance as IMeasureable;
        if (measureableTile != null) Debug.Log("Measurement result: " + measureableTile.Measured());
    }
}