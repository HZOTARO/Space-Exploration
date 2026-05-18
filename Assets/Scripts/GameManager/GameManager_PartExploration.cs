using System.Collections.Generic;
using UnityEngine;

public class GameManager_PartExploration : GameManager
{
    List<Enemy> activeEnemies;
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

        Bind("collect", Collect);

        if (UpgradeManager.instance)
        {
            if (UpgradeManager.instance.IsUpgradeUnlocked("scan")) BindReturn("scan", Scan);
            if (UpgradeManager.instance.IsUpgradeUnlocked("measure")) BindReturn("measure", Measure);
        }
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

    public void Shoot()
    {
        Vector2Int currentCheckLoc = GetForwardGridLoc();
        bool hitSomething = false;

        // Play the shooting animation/sound
        // player.PerformAction(PlayerAction.Shoot); 

        while (currentCheckLoc.x >= 0 && currentCheckLoc.x < tileManager.width &&
               currentCheckLoc.y >= 0 && currentCheckLoc.y < tileManager.length)
        {
            TileObject staticTile = tileManager.objectsArray[currentCheckLoc.y, currentCheckLoc.x];
            if (staticTile.type == TileType.Wall)
            {
                Debug.Log("Your shot hit a Wall.");
                hitSomething = true;
                break; 
            }

            Enemy hitEnemy = null;
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy.gridLoc == currentCheckLoc)
                {
                    hitEnemy = enemy;
                    break;
                }
            }

            if (hitEnemy != null)
            {
                Debug.Log("<color=orange>Enemy destroyed!</color>");

                activeEnemies.Remove(hitEnemy);

                Destroy(hitEnemy.gameObject);

                hitSomething = true;
                break;
            }

            if (playerFacing == 0) currentCheckLoc.y++; 
            else if (playerFacing == 1) currentCheckLoc.x++;  
            else if (playerFacing == 2) currentCheckLoc.y--; 
            else if (playerFacing == 3) currentCheckLoc.x--;  
        }

        if (!hitSomething)
        {
            Debug.Log("You shot into empty space.");
        }
    }
    protected virtual void TriggerEnemyTurns()
    {
        // If there are no enemies, just skip this completely
        if (activeEnemies == null || activeEnemies.Count == 0) return;

        foreach (Enemy enemy in activeEnemies)
        {
            // 1. Check if the player stepped directly ONTO the enemy first
            if (enemy.gridLoc == playerGridLoc)
            {
                CatchPlayer();
                continue;
            }

            // 2. Calculate where the enemy wants to step next
            Vector2Int nextPos = enemy.gridLoc + enemy.patrolDir;

            // 3. Check if the next tile is out of bounds or no longer an EnemyPath
            if (nextPos.x < 0 || nextPos.x >= tileManager.width ||
                nextPos.y < 0 || nextPos.y >= tileManager.length ||
                tileManager.objectsArray[nextPos.y, nextPos.x].type != TileType.EnemyPath)
            {
                // Reverse direction 180 degrees!
                enemy.patrolDir *= -1;
                nextPos = enemy.gridLoc + enemy.patrolDir;
            }

            // 4. Update the enemy's logic coordinate
            enemy.gridLoc = nextPos;

            // 5. Tell the enemy to update its 3D model position
            enemy.UpdateVisualPosition();

            // 6. Check if the enemy just stepped ONTO the player
            if (enemy.gridLoc == playerGridLoc)
            {
                CatchPlayer();
            }
        }
    }

    private void CatchPlayer()
    {
        Debug.Log("<color=red>The Player was caught by an Enemy!</color>");

        // Use the exact damage method you used in your Mine() logic
        if (healthComponent != null)
        {
            healthComponent.DamagePlayer(9999);
        }
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
}