using UnityEngine;
using System;

public class GameManager_PartExploration : GameManager
{
    protected override void StartValuesSetup()
    {
        base.StartValuesSetup();

        if (mapSizeUpgrade)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(mapSizeUpgrade.id);
            levelLength = 5 * (upgradeLevel + 1);
            levelWidth = 5 * (upgradeLevel + 1);
        }
        if (cargoSizeUpgrade && cargoComponent)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(cargoSizeUpgrade.id);
            cargoComponent.cargoSize = 4 + 2 * upgradeLevel;
        }
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

    public void Wait()
    {
        // Tell the player to skip a turn while the enemy patrols
        //player.PerformAction(PlayerAction.Idle, () => Debug.Log("Player waited for 1 turn."));
    }
}