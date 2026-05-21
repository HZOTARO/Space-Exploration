using UnityEngine;

public class GameManager_PartExploration : GameManager
{
    protected override void Start()
    {
        base.Start();

        if (tileManager is TileManager_PartExploration expManager)
        {
            this.activeEnemies = expManager.spawnedEnemies;
        }
    }

    protected override void StartValuesSetup()
    {
        base.StartValuesSetup();

        if (mapSizeUpgrade)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(mapSizeUpgrade.id);
            levelWidth = 10 * (upgradeLevel);
            levelLength = 10 * (upgradeLevel);
        }
        if (cargoSizeUpgrade && cargoComponent)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(cargoSizeUpgrade.id);
            cargoComponent.cargoSize = 6 + 2 * upgradeLevel;
        }
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        BindReturn("move_forward", MoveForward);
        BindReturn("move_backward", MoveBackward);
        Bind("turn_right", TurnRight);
        Bind("turn_left", TurnLeft);
        Bind("wait", Wait);

        Bind("go_back", Return);

        Bind("collect", Collect);

        if (UpgradeManager.instance)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("scan")) BindReturn("scan", Scan);
            if (UpgradeManager.instance.IsUpgradeUnlocked("measure")) BindReturn("measure", Measure);
        }
    }

    public override bool InAction()
    {
        if (base.InAction()) return true;

        if (activeEnemies != null)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null && enemy.inAction) return true;
            }
        }
        return false;
    }

    public override TileObject GetTileInFront()
    {
        Vector2Int forwardLoc = GetForwardGridLoc();

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy.gridLoc == forwardLoc)
            {
                TileObject enemyTile = new TileObject();
                enemyTile.type = TileType.Enemy;

                return enemyTile;
            }
        }

        return base.GetTileInFront();
    }

    public void Collect()
    {
        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("Nothing to collect. You are facing the edge of the map!");
            return;
        }

        if (targetTile.type == TileType.Gear)
        {
            ValueTile_Floating gear = targetTile.tileInstance as ValueTile_Floating;
            player.PerformAction(PlayerAction.Collect, () =>
            {
                int amountCollected = gear.Collect();
                if (amountCollected > 0)
                {
                    cargoComponent.AddToCargo(gear.itemOnTile, amountCollected);
                    Debug.Log("<color=#aaaaaa>Collected 1 Gear.</color>");
                }
            });
        }

        else if (targetTile.type == TileType.Screw)
        {
            ValueTile_Floating screw = targetTile.tileInstance as ValueTile_Floating;
            player.PerformAction(PlayerAction.Collect, () =>
            {
                int amountCollected = screw.Collect();
                if (amountCollected > 0)
                {
                    cargoComponent.AddToCargo(screw.itemOnTile, amountCollected);
                    Debug.Log("<color=#aaaaaa>Collected 1 Screw.</color>");
                }
            });
        }
        else
        {
            Debug.Log("There is nothing collectible here.");
        }
    }

    public override string Scan()
    {
        Vector2Int forwardLoc = GetForwardGridLoc();

        if (activeEnemies != null)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy.gridLoc == forwardLoc)
                {
                    Debug.Log("<color=red>Scanned: Enemy detected!</color>");
                    return "Enemy";
                }
            }
        }

        TileObject targetTile = GetTileInFront();

        if (targetTile == null)
        {
            Debug.Log("Scanned: Edge of map.");
            return "Empty";
        }

        Debug.Log($"Scanned: {targetTile.type.ToString()}");
        return targetTile.type.ToString();
    }

    protected virtual void TriggerEnemyTurns()
    {
        if (activeEnemies == null || activeEnemies.Count == 0) return;

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy.isDead) continue;

            if (enemy.gridLoc == playerGridLoc)
            {
                TriggerEnemyCatch(enemy);
                continue;
            }

            Vector2Int nextPos = enemy.GetNextPatrolNode();

            enemy.MoveForward(nextPos);
            enemy.AdvancePathIndex();

            if (enemy.gridLoc == playerGridLoc)
            {
                TriggerEnemyCatch(enemy);
            }
        }
    }

    private void TriggerEnemyCatch(Enemy enemy)
    {
        Debug.Log("<color=red>An Enemy is attacking!</color>");

        enemy.PerformAction(EnemyAction.Attack, () =>
        {
            if (healthComponent != null)
            {
                healthComponent.DamagePlayer(9999);
            }
        });
    }

    public override bool MoveForward()
    {
        bool result = base.MoveForward();
        if (result) TriggerEnemyTurns();
        return result;
    }
    public override bool MoveBackward()
    {
        bool result = base.MoveBackward();
        if (result) TriggerEnemyTurns();
        return result;
    }

    public void Wait()
    {
        TriggerEnemyTurns();
    }
}