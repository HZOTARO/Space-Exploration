using System.Collections;
using UnityEngine;

public class GameManager_PartExploration : GameManager
{
    protected override void Start()
    {
        base.Start();

        if (tileManager is TileManager_PartExploration expManager)
        {
            activeEnemies = expManager.spawnedEnemies;
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
        if (cargoSizeUpgrade)
        {
            int upgradeLevel = UpgradeManager.instance.GetUpgradeLevel(cargoSizeUpgrade.id);
            cargoSize = 6 + 2 * upgradeLevel;
        }
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        base.RegisterLevelSpecificPythonCommands();

        Bind("go_back", Return);
        Bind("collect", Collect);

        BindWithArg<int>("discard", Discard);
    }

    public override bool InAction()
    {
        if (isSequenceRunning) return true;
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
            if (!enemy.isDead && enemy.gridLoc == forwardLoc)
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
            Debug.Log("You are facing the edge of the map!");
            return;
        }

        if (cargoComponent && cargoComponent.IsFull())
        {
            Debug.Log("Cargo is full. Cannot collect more resources.");
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

                    targetTile.type = TileType.Floor;

                    if (targetTile.tileInstance != null)
                    {
                        Destroy(targetTile.tileInstance.gameObject);
                        targetTile.tileInstance = null;
                    }
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

                    targetTile.type = TileType.Floor;

                    if (targetTile.tileInstance != null)
                    {
                        Destroy(targetTile.tileInstance.gameObject);
                        targetTile.tileInstance = null;
                    }
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
                if (!enemy.isDead && enemy.gridLoc == forwardLoc)
                {
                    Debug.Log("<color=red>Scanned: Enemy detected!</color>");
                    return "Enemy";
                }
            }
        }

        return base.Scan();
    }

    protected virtual void TriggerEnemyTurns()
    {
        if (activeEnemies == null || activeEnemies.Count == 0) return;

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy.isDead) continue;

            Vector2Int nextPos = enemy.GetNextPatrolNode();

            if (nextPos == playerGridLoc)
            {
                enemy.pathDirection *= -1;
                nextPos = enemy.GetNextPatrolNode();

                if (nextPos == playerGridLoc)
                {
                    nextPos = enemy.gridLoc;
                }
            }

            if (nextPos != enemy.gridLoc)
            {
                enemy.AdvancePathIndex();

                enemy.MoveForward(nextPos, () =>
                {
                    if (IsPlayerAdjacent(enemy.gridLoc) && !enemy.isDead)
                    {
                        TriggerEnemyCatch(enemy);
                    }
                    else
                    {
                        enemy.inAction = false;
                        enemy.PlayIdle();
                    }
                });
            }
            else if (IsPlayerAdjacent(enemy.gridLoc))
            {
                TriggerEnemyCatch(enemy);
            }
        }
    }

    private bool IsPlayerAdjacent(Vector2Int enemyLoc)
    {
        int dx = Mathf.Abs(enemyLoc.x - playerGridLoc.x);
        int dy = Mathf.Abs(enemyLoc.y - playerGridLoc.y);

        return dx <= 1 && dy <= 1;
    }

    private void TriggerEnemyCatch(Enemy enemy)
    {
        Debug.Log("<color=red>An Enemy is attacking!</color>");

        Vector2Int dirToPlayer = playerGridLoc - enemy.gridLoc;
        if (dirToPlayer != Vector2Int.zero)
        {
            enemy.SnapRotationToDirection(dirToPlayer);
        }

        enemy.PerformAction(EnemyAction.Attack, null);

        StartCoroutine(DamagePlayerAfterDelay(0.5f));
    }

    private IEnumerator DamagePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (healthComponent != null)
        {
            healthComponent.DamagePlayer(50);
        }
    }

    public override void Move(string directionString, int distance)
    {
        if (distance <= 0) return;
        StartCoroutine(ExplorationMoveRoutine(directionString, distance));
    }

    private IEnumerator ExplorationMoveRoutine(string directionString, int distance)
    {
        isSequenceRunning = true;

        yield return null;

        Direction physicalDirection = Direction.Forward;
        int zModifier = 0;
        int xModifier = 0;

        if (directionString == "forward")
        {
            physicalDirection = Direction.Forward;
            if (playerFacing == 0) zModifier = 1;
            else if (playerFacing == 1) xModifier = 1;
            else if (playerFacing == 2) zModifier = -1;
            else if (playerFacing == 3) xModifier = -1;
        }
        else if (directionString == "backward")
        {
            physicalDirection = Direction.Backward;
            if (playerFacing == 0) zModifier = -1;
            else if (playerFacing == 1) xModifier = -1;
            else if (playerFacing == 2) zModifier = 1;
            else if (playerFacing == 3) xModifier = 1;
        }
        else
        {
            PythonExecutor.instance.TriggerRuntimeError($"Invalid direction: '{directionString}'.");
            isSequenceRunning = false;
            yield break;
        }

        for (int i = 0; i < distance; i++)
        {
            int checkZ = playerGridLoc.x + zModifier;
            int checkX = playerGridLoc.y + xModifier;
            Vector2Int checkLoc = new Vector2Int(checkZ, checkX);

            bool hitEnemy = false;
            if (activeEnemies != null)
            {
                foreach (Enemy enemy in activeEnemies)
                {
                    if (!enemy.isDead && enemy.gridLoc == checkLoc)
                    {
                        hitEnemy = true;
                        break;
                    }
                }
            }

            if (hitEnemy)
            {
                PythonExecutor.instance.TriggerRuntimeError("Collision Error: The robot crashed into an enemy!");
                break;
            }

            if (checkX < 0 || checkX >= tileManager.width || checkZ < 0 || checkZ >= tileManager.length || !IsTileWalkable(checkZ, checkX))
            {
                PythonExecutor.instance.TriggerRuntimeError("Collision Error: The robot crashed into an obstacle!");
                break;
            }

            playerGridLoc.x = checkZ;
            playerGridLoc.y = checkX;

            player.Move(physicalDirection);
            TriggerEnemyTurns();

            while (player.inAction || EnemiesInAction())
            {
                yield return null;
            }

            if (healthComponent != null && healthComponent.currentHealth <= 0)
            {
                break;
            }

            TriggerSuccessfulMoveEvent();
        }

        isSequenceRunning = false;
    }

    public override void Wait()
    {
        StartCoroutine(ExplorationWaitRoutine());
    }

    private System.Collections.IEnumerator ExplorationWaitRoutine()
    {
        isSequenceRunning = true;

        yield return null;

        TriggerEnemyTurns();

        while (EnemiesInAction())
        {
            yield return null;
        }

        if (healthComponent != null && healthComponent.currentHealth <= 0)
        {
            isSequenceRunning = false;
            yield break;
        }

        isSequenceRunning = false;
    }

    private bool EnemiesInAction()
    {
        if (activeEnemies != null)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null && enemy.inAction) return true;
            }
        }
        return false;
    }
}