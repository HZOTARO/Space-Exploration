using UnityEngine;

public class GameManager_ResourceExploration : GameManager
{
    protected override void StartValuesSetup()
    {
        base.StartValuesSetup();

        if (mapSizeUpgrade)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(mapSizeUpgrade.id);
            levelWidth = 5 * (upgradeLevel + 1);
            levelLength = 5 * (upgradeLevel + 1);
        }
        if (cargoSizeUpgrade && cargoComponent)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(cargoSizeUpgrade.id);
            cargoComponent.cargoSize = 4 + 2 * upgradeLevel;
        }
    }
    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();

        Bind("go_back", Return);

        Bind("mine", Mine);
        Bind("collect", Collect);

        if (UpgradeManager.instance)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("purple_liquid"))
            {
                Bind("drill", Drill);
                Bind("pump", Pump);
            }
            if (UpgradeManager.instance.IsUpgradeUnlocked("black_ore"))
            {
                Bind("purify", Purify);
            }
        }
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
            if (!ore.isMined) player.PerformAction(PlayerAction.Mine, () => { if (ore.Mine()) healthComponent.DamagePlayer(60); });
            else Debug.Log("This Black Ore has already been mined.");
        }
        else
        {
            Debug.Log("No mineable resource in front of you.");
        }
    }

    public void Collect()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }

        if (cargoComponent && cargoComponent.IsFull())
        {
            Debug.Log("Cargo is full. Cannot collect more resources.");
            return;
        }

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
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
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
                        cargoComponent.AddToCargo(ore.itemOnTile, amountCollected);
                        Debug.Log($"<color=black>Collected {amountCollected} Black Ore.</color>");
                    }
                });
            }
        }
    }

    public void Drill()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }
        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (!vein.isDrilled) player.PerformAction(PlayerAction.Drill, () => vein.Drill());
        }
    }

    public void Pump()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile == null)
        {
            Debug.Log("You are facing the edge of the map!");
            return;
        }
        if (targetTile.type == TileType.PurpleEssence)
        {
            CaveTile_PurpleVein vein = targetTile.tileInstance as CaveTile_PurpleVein;
            if (vein.isDrilled && !vein.isPumped)
            {
                player.PerformAction(PlayerAction.Pump, () =>
                {
                    int amountPumped = vein.Pump();
                    if (amountPumped > 0)
                    {
                        cargoComponent.AddToCargo(vein.itemOnTile, amountPumped);
                        Debug.Log($"<color=purple>Collected {amountPumped} Purple Liquid.</color>");
                    }
                });
            }
        }
    }

    public void Purify()
    {
        TileObject targetTile = GetTileInFront();
        if (targetTile.type == TileType.BlackOre)
        {
            CaveTile_BlackOre ore = targetTile.tileInstance as CaveTile_BlackOre;
            if (!ore.isPurified) player.PerformAction(PlayerAction.Purify, () => ore.Purify());
        }
    }
}