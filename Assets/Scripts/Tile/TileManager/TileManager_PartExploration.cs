using System.Collections.Generic;
using UnityEngine;

public enum EnemyPathDirection
{
    Up, Down, Left, Right
}

public class TileManager_PartExploration : TileManager_Cave
{
    [Header("Enemies Settings")]
    public int numberOfEnemies;

    [Header("Items Settings")]
    public int gearCount;
    public int screwCount;

    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [HideInInspector] public List<Enemy> spawnedEnemies = new List<Enemy>();

    public override void GenerateMap(bool setAllfloor = true)
    {
        base.GenerateMap(setAllfloor);

        EnemyPath tilePrefab = FindTilePrefab(TileType.EnemyPath) as EnemyPath;

        if (tilePrefab == null) return;

        foreach (Enemy enemy in spawnedEnemies)
        {
            EnemyPath prevTile = null;
            Vector2Int prevPos = enemy.patrolPath[0];

            foreach (Vector2Int path in enemy.patrolPath)
            {
                EnemyPath currentTile = InstantiateTileVisual(path.x, path.y, tilePrefab) as EnemyPath;

                if (prevTile == null)
                {
                    currentTile.SetPathDirection(EnemyMark.Middle);

                    prevTile = currentTile;
                    prevPos = path;
                    continue;
                }

                Vector2Int direction = path - prevPos;
                EnemyPathDirection pathDirection = EnemyPathDirection.Right;

                if (direction.y > 0) pathDirection = EnemyPathDirection.Right;
                else if (direction.y < 0) pathDirection = EnemyPathDirection.Left;
                else if (direction.x > 0) pathDirection = EnemyPathDirection.Up;
                else if (direction.x < 0) pathDirection = EnemyPathDirection.Down;

                switch (pathDirection)
                {
                    case EnemyPathDirection.Left:
                        prevTile.SetPathDirection(EnemyMark.Right);
                        currentTile.SetPathDirection(EnemyMark.Left);
                        break;
                    case EnemyPathDirection.Right:
                        prevTile.SetPathDirection(EnemyMark.Left);
                        currentTile.SetPathDirection(EnemyMark.Right);
                        break;
                    case EnemyPathDirection.Up:
                        prevTile.SetPathDirection(EnemyMark.Down);
                        currentTile.SetPathDirection(EnemyMark.Up);
                        break;
                    case EnemyPathDirection.Down:
                        prevTile.SetPathDirection(EnemyMark.Up);
                        currentTile.SetPathDirection(EnemyMark.Down);
                        break;
                }

                prevTile = currentTile;
                prevPos = path;
            }

            prevTile.SetPathDirection(EnemyMark.Middle);
        }
    }

    protected override void GenerateMapContent()
    {
        numberOfEnemies = width / 10 * 2;
        int itemCount = (width * length) / 20;
        gearCount = Mathf.RoundToInt(itemCount * Random.Range(0.4f, 0.6f));
        screwCount = itemCount - gearCount;

        spawnedEnemies.Clear();

        GenerateSegmentedEnemies();
        GenerateSegmentedItems();

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < length; x++)
            {
                if (objectsArray[z, x].type == TileType.EnemyPath)
                {
                    objectsArray[z, x].type = TileType.Floor;
                }
            }
        }
    }

    private void GenerateSegmentedEnemies()
    {
        int segmentSize = 10;

        int segmentLength = Mathf.RoundToInt((float)length / segmentSize);
        int segmentWidth = Mathf.RoundToInt((float)width / segmentSize);
        int segmentCount = segmentLength * segmentWidth;

        if (segmentCount <= 0) return;

        int enemyPerSegment = Mathf.Max(1, Mathf.RoundToInt((float)numberOfEnemies / segmentCount));
        int minPatrolLength = 30 / enemyPerSegment;
        int maxPatrolLength = 50 / enemyPerSegment;

        for (int segmentZ = 0; segmentZ < segmentLength; segmentZ++)
        {
            for (int segmentX = 0; segmentX < segmentWidth; segmentX++)
            {
                int currentEnemyCount = enemyPerSegment;

                List<Vector2Int> validPos = new List<Vector2Int>();
                for (int z = segmentZ * segmentSize; z < (segmentZ + 1) * segmentSize && z < length; z++)
                {
                    for (int x = segmentX * segmentSize; x < (segmentX + 1) * segmentSize && x < width; x++)
                    {
                        if (z < 2 && x < 2) continue;

                        if (objectsArray[z, x].type == TileType.Floor)
                        {
                            validPos.Add(new Vector2Int(z, x));
                        }
                    }
                }

                while (currentEnemyCount > 0 && validPos.Count > 0)
                {
                    List<Vector2Int> patrolPath = new List<Vector2Int>();

                    Vector2Int startPos = validPos[Random.Range(0, validPos.Count)];

                    patrolPath.Add(startPos);
                    validPos.Remove(startPos);

                    objectsArray[startPos.x, startPos.y].type = TileType.EnemyPath;

                    Vector2Int currentPos = startPos;
                    int patrolLength = Random.Range(minPatrolLength, maxPatrolLength + 1);

                    while (patrolLength > 0)
                    {
                        List<EnemyPathDirection> validDirections = CheckAdjacent(currentPos, validPos);

                        if (validDirections.Count <= 0)
                        {
                            break;
                        }

                        EnemyPathDirection direction = validDirections[Random.Range(0, validDirections.Count)];

                        int preferredLength = Random.Range(3, 9);

                        while (preferredLength > 0 && patrolLength > 0)
                        {
                            Vector2Int nextPos = currentPos;
                            switch (direction)
                            {
                                case EnemyPathDirection.Right: nextPos.y++; break;
                                case EnemyPathDirection.Up: nextPos.x++; break;
                                case EnemyPathDirection.Left: nextPos.y--; break;
                                case EnemyPathDirection.Down: nextPos.x--; break;
                            }
                            if (validPos.Contains(nextPos))
                            {
                                patrolPath.Add(nextPos);
                                validPos.Remove(nextPos);

                                objectsArray[nextPos.x, nextPos.y].type = TileType.EnemyPath;

                                currentPos = nextPos;

                                preferredLength--;
                                patrolLength--;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (enemyPrefab != null && patrolPath.Count > 0)
                    {
                        GameObject spawnedEnemy = Instantiate(enemyPrefab);
                        spawnedEnemy.transform.localScale = Vector3.one;
                        spawnedEnemy.transform.SetParent(tilesContainer.transform, false);

                        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
                        enemyScript.Setup(patrolPath);

                        spawnedEnemies.Add(enemyScript);
                    }

                    currentEnemyCount--;
                }
            }
        }
    }

    private List<EnemyPathDirection> CheckAdjacent(Vector2Int currentPosition, List<Vector2Int> availablePosition)
    {
        List<EnemyPathDirection> validDirections = new List<EnemyPathDirection>();

        if (availablePosition.Contains(new Vector2Int(currentPosition.x, currentPosition.y + 1)))
        {
            validDirections.Add(EnemyPathDirection.Right);
        }
        if (availablePosition.Contains(new Vector2Int(currentPosition.x, currentPosition.y - 1)))
        {
            validDirections.Add(EnemyPathDirection.Left);
        }
        if (availablePosition.Contains(new Vector2Int(currentPosition.x + 1, currentPosition.y)))
        {
            validDirections.Add(EnemyPathDirection.Up);
        }
        if (availablePosition.Contains(new Vector2Int(currentPosition.x - 1, currentPosition.y)))
        {
            validDirections.Add(EnemyPathDirection.Down);
        }

        return validDirections;
    }

    private void GenerateSegmentedItems()
    {
        int segmentSize = 10;

        int segmentLength = Mathf.RoundToInt((float)length / segmentSize);
        int segmentWidth = Mathf.RoundToInt((float)width / segmentSize);

        if (segmentLength <= 0 || segmentWidth <= 0) return;

        int currentGearCount = gearCount;
        int currentScrewCount = screwCount;

        List<Vector2Int>[,] segmentValidPos = new List<Vector2Int>[segmentLength, segmentWidth];
        int totalValidTiles = 0;

        for (int segmentZ = 0; segmentZ < segmentLength; segmentZ++)
        {
            for (int segmentX = 0; segmentX < segmentWidth; segmentX++)
            {
                List<Vector2Int> validPos = new List<Vector2Int>();
                for (int z = segmentZ * segmentSize; z < (segmentZ + 1) * segmentSize && z < length; z++)
                {
                    for (int x = segmentX * segmentSize; x < (segmentX + 1) * segmentSize && x < width; x++)
                    {
                        if (z < 2 && x < 2) continue;

                        if (objectsArray[z, x].type == TileType.Floor)
                        {
                            validPos.Add(new Vector2Int(z, x));
                        }
                    }
                }

                segmentValidPos[segmentZ, segmentX] = validPos;
                totalValidTiles += validPos.Count;
            }
        }

        for (int segmentZ = 0; segmentZ < segmentLength; segmentZ++)
        {
            for (int segmentX = 0; segmentX < segmentWidth; segmentX++)
            {
                List<Vector2Int> validPos = segmentValidPos[segmentZ, segmentX];
                int validCountInSegment = validPos.Count;

                if (totalValidTiles <= 0) break;

                float proportion = (float)validCountInSegment / totalValidTiles;

                int gearInThisSegment = Mathf.RoundToInt(currentGearCount * proportion);
                gearInThisSegment = Mathf.Clamp(gearInThisSegment, 0, currentGearCount);
                currentGearCount -= gearInThisSegment;

                int screwInThisSegment = Mathf.RoundToInt(currentScrewCount * proportion);
                screwInThisSegment = Mathf.Clamp(screwInThisSegment, 0, currentScrewCount);
                currentScrewCount -= screwInThisSegment;

                while (gearInThisSegment > 0 && validPos.Count > 0)
                {
                    Vector2Int spot = validPos[Random.Range(0, validPos.Count)];
                    objectsArray[spot.x, spot.y].type = TileType.Gear;

                    validPos.Remove(spot);
                    gearInThisSegment--;
                }

                while (screwInThisSegment > 0 && validPos.Count > 0)
                {
                    Vector2Int spot = validPos[Random.Range(0, validPos.Count)];
                    objectsArray[spot.x, spot.y].type = TileType.Screw;

                    validPos.Remove(spot);
                    screwInThisSegment--;
                }

                totalValidTiles -= validCountInSegment;
            }
        }
    }
}