using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [HideInInspector]
    public int width = 5;
    [HideInInspector]
    public int length = 5;

    [Header("References")]
    public GameObject container;

    [Header("Prefab")]
    public GameObject grid;
    public List<TileReference> tileData = new List<TileReference>();
    protected BaseTile currentTilePrefab;

    [HideInInspector]
    public TileObject[,] gridArray;

    public virtual void GenerateMap()
    {
        if (!container) return;
        GameObject spawnedGrid = Instantiate(grid, new Vector3(0, 0, -0.005f), Quaternion.identity);
        spawnedGrid.transform.SetParent(container.transform, false);
        spawnedGrid.transform.localScale = new Vector3(width, 1, length);
        spawnedGrid.name = "Grid";

        gridArray = new TileObject[length, width];
    }

    protected void SpawnTilesVisual()
    {
        currentTilePrefab = FindTilePrefab(TileType.Default);

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (gridArray[z, x].type == TileType.Default || !gridArray[z, x].tileInstance.haveTile)
                {
                    SpawnTileVisual(z, x, currentTilePrefab);
                }
            }
        }
    }
    protected BaseTile FindTilePrefab(TileType tileType)
    {
        foreach (var item in tileData)
        {
            if (item.type == tileType)
            {
                return item.tilePrefab;
            }
        }
        return null;
    }
    protected BaseTile SpawnTileVisual(int z, int x, BaseTile tilePrefab)
    {
        if (tilePrefab)
        {
            BaseTile spawnedTile = Instantiate(tilePrefab, new Vector3(x, 0, z), Quaternion.identity);
            spawnedTile.transform.localScale = Vector3.one;
            spawnedTile.transform.SetParent(container.transform, false);
            spawnedTile.name = $"{tilePrefab.name}_{z}_{x}";
            spawnedTile.z = z;
            spawnedTile.x = x;
            return spawnedTile;
        }
        return null;
    }
}
