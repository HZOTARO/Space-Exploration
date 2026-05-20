using System.Collections.Generic;
using UnityEngine;

public class TileManager_PartExploration : TileManager_Cave
{
    [Header("Exploration Settings")]
    public int wallDensityPercentage = 15;
    public int numberOfEnemies = 2;
    public int patrolPathLength = 4;

    [Tooltip("Total amount to spawn across the whole map")]
    public int gearCount = 5;
    public int screwCount = 5;

    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [HideInInspector] public List<Enemy> spawnedEnemies = new List<Enemy>();

    protected override void GenerateMapContent()
    {
        numberOfEnemies = width / 10 * 2;
        int itemCount = (width * length) / 10;
        gearCount = itemCount * Random.Range(40, 60) / 100;
        screwCount = itemCount - gearCount;

        spawnedEnemies.Clear();

        GenerateEnemyPaths();
        //GenerateWalls();
        GenerateSegmentedItems();
    }

    private void GenerateEnemyPaths()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                int startX = Random.Range(3, width - patrolPathLength);
                int startZ = Random.Range(3, length - patrolPathLength);
                bool isHorizontal = Random.value > 0.5f;

                bool isClear = true;
                for (int step = 0; step < patrolPathLength; step++)
                {
                    int x = isHorizontal ? startX + step : startX;
                    int z = isHorizontal ? startZ : startZ + step;

                    if (objectsArray[z, x].type != TileType.Floor)
                    {
                        isClear = false;
                        break;
                    }
                }

                if (isClear)
                {
                    for (int step = 0; step < patrolPathLength; step++)
                    {
                        int x = isHorizontal ? startX + step : startX;
                        int z = isHorizontal ? startZ : startZ + step;

                        floorArray[z, x].type = TileType.EnemyPath;
                        objectsArray[z, x].type = TileType.EnemyPath;
                    }

                    if (enemyPrefab != null)
                    {
                        GameObject spawnedEnemy = Instantiate(enemyPrefab);
                        spawnedEnemy.transform.localScale = Vector3.one;
                        spawnedEnemy.transform.SetParent(tilesContainer.transform, false);
                        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();


                        Vector2Int patrolDirection = isHorizontal ? new Vector2Int(1, 0) : new Vector2Int(0, 1);
                        enemyScript.Setup(new Vector2Int(startX, startZ), patrolDirection);

                        spawnedEnemies.Add(enemyScript);
                    }
                    break;
                }
            }
        }
    }

    private void GenerateWalls()
    {
        int totalTiles = width * length;
        int targetWallCount = (totalTiles * wallDensityPercentage) / 100;
        int placedWalls = 0;

        for (int attempts = 0; attempts < 1000 && placedWalls < targetWallCount; attempts++)
        {
            int x = Random.Range(0, width);
            int z = Random.Range(0, length);

            if (x < 2 && z < 2) continue;

            if (objectsArray[z, x].type == TileType.Floor)
            {
                objectsArray[z, x].type = TileType.Wall;
                placedWalls++;
            }
        }
    }

    private void GenerateSegmentedItems()
    {
        int currentGearCount = gearCount;
        int currentScrewCount = screwCount;

        int segmentSize = 5;
        int segmentLength = Mathf.RoundToInt((float)length / segmentSize);
        int segmentLengthRemainder = length % segmentSize;
        int segmentWidth = Mathf.RoundToInt((float)width / segmentSize);
        int segmentWidthRemainder = width % segmentSize;
        int segmentCount = segmentLength * segmentWidth;

        if (segmentCount <= 0) return;

        int randZ, randX;
        for (int z = 0; z < segmentLength; z++)
        {
            for (int x = 0; x < segmentWidth; x++)
            {
                int segmentGearCount = Mathf.RoundToInt((float)currentGearCount / segmentCount);
                currentGearCount -= segmentGearCount;

                int segmentScrewCount = Mathf.RoundToInt((float)currentScrewCount / segmentCount);
                currentScrewCount -= segmentScrewCount;

                int failsafe = 0;
                while (segmentGearCount > 0 && failsafe < 50)
                {
                    failsafe++;
                    randZ = Random.Range(0, z == segmentLength - 1 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == segmentWidth - 1 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor && !(randZ == 0 && randX == 0))
                    {
                        objectsArray[randZ, randX].type = TileType.Gear;
                        segmentGearCount--;
                    }
                }

                failsafe = 0;
                while (segmentScrewCount > 0 && failsafe < 50)
                {
                    failsafe++;
                    randZ = Random.Range(0, z == segmentLength - 1 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == segmentWidth - 1 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor && !(randZ == 0 && randX == 0))
                    {
                        objectsArray[randZ, randX].type = TileType.Screw;
                        segmentScrewCount--;
                    }
                }

                segmentCount--;
            }
        }
    }
}