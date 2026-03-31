using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileData
{
    public TileType type;
    public GameObject tileObject;
}
public enum TileType
{
    Floor,
    Mineral,
    Ore
}

public class TileManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 5;
    public int length = 5;

    [Header("References")]
    public GameObject container;

    [Header("Prefab")]
    public GameObject grid;
    public List<TileData> tileData = new List<TileData>();

    [HideInInspector]
    public TileData[,] gridArray;

    void Start()
    {
        if (container)
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        GameObject spawnedGrid = Instantiate(grid, new Vector3(0, 0, -0.005f), Quaternion.identity);
        spawnedGrid.transform.SetParent(container.transform, false);
        spawnedGrid.transform.localScale = new Vector3(width, 1, length);
        spawnedGrid.name = "Grid";

        float resourcePercentage = Random.Range(15f, 25f);
        int resourceCount = Mathf.RoundToInt(resourcePercentage / 100 * width * length);
        //Debug.Log($"Resource Percentage: {resourcePercentage / 100}%, Resource Count: {resourceCount}");
        float orePercentage = Random.Range(40f, 60f);
        int oreCount = Mathf.RoundToInt(orePercentage / 100 * resourceCount);
        //Debug.Log($"Ore Percentage: {orePercentage / 100}%, Ore Count: {oreCount}");
        int mineralCount = resourceCount - oreCount;
        //Debug.Log($"Mineral Count: {mineralCount}");

        gridArray = new TileData[length, width];

        int segmentSize = 5;
        int segmentLength = Mathf.RoundToInt(length/segmentSize);
        int segmentLengthRemainder = length % segmentSize;
        int segmentWidth = Mathf.RoundToInt(width/segmentSize);
        int segmentWidthRemainder = width % segmentSize;
        int segmentCount = segmentLength * segmentWidth;
        //Debug.Log($"Segment Size: {segmentSize}, Segment Length: {segmentLength}, Segment Length Remainder: {segmentLengthRemainder}, Segment Width: {segmentWidth}, Segment Width Remainder: {segmentWidthRemainder}, Segment Count: {segmentCount}");

        int randZ, randX;
        for (int z = 0; z < segmentLength; z++)
        {
            for (int x = 0; x < segmentWidth; x++)
            {
                int segmentMineralCount = Mathf.RoundToInt((float)mineralCount/segmentCount);
                mineralCount -= segmentMineralCount;

                int segmentOreCount = Mathf.RoundToInt((float)oreCount /segmentCount);
                oreCount -= segmentOreCount;

                while (segmentMineralCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (gridArray[randZ, randX].type == TileType.Floor)
                    {
                        gridArray[randZ, randX].type = TileType.Mineral;
                        segmentMineralCount--;
                    }
                }

                while (segmentOreCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (gridArray[randZ, randX].type == TileType.Floor)
                    {
                        gridArray[randZ, randX].type = TileType.Ore;
                        segmentOreCount--;
                    }
                }

                segmentCount--;
            }
        }

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                SpawnTileVisual(z, x, gridArray[z,x].type);
            }
        }
    }

    private void SpawnTileVisual(int z, int x, TileType tileType)
    {
        GameObject prefabToSpawn = null;

        foreach (var item in tileData)
        {
            if (item.type == tileType)
            {
                prefabToSpawn = item.tileObject;
            }
        }

        if (prefabToSpawn)
        {
            GameObject spawnedTile = Instantiate(prefabToSpawn, new Vector3(x, 0, z), Quaternion.identity);
            spawnedTile.transform.SetParent(container.transform, false);
            spawnedTile.name = $"{tileType}_{z}_{x}";

            gridArray[z, x].tileObject = spawnedTile;
        }
    }
}
