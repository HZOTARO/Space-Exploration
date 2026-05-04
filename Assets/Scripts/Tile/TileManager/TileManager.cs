using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [HideInInspector] public int width = 5;
    [HideInInspector] public int length = 5;

    [Header("References")]
    public GameObject tilesContainer;

    [Header("Prefab")]
    public GameObject grid;
    public BaseTile floorTile;
    public List<TileReference> tileData = new List<TileReference>();
    protected BaseTile currentTilePrefab;

    [HideInInspector]
    public TileObject[,] objectsArray;
    [HideInInspector]
    public TileObject[,] floorArray;

    public virtual void GenerateMap()
    {
        if (!tilesContainer) return;

        objectsArray = new TileObject[length, width];
        floorArray = new TileObject[length, width];

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                objectsArray[z, x] = new TileObject();
                objectsArray[z, x].type = TileType.Floor;

                floorArray[z, x] = new TileObject();
                floorArray[z, x].type = TileType.Floor;
            }
        }

        GenerateMapContent();

        SpawnTilesVisual();
    }

    protected virtual void GenerateMapContent()
    {
    }

    protected BaseTile FindTilePrefab(TileType tileType)
    {
        foreach (TileReference item in tileData)
        {
            if (item.type == tileType) return item.tilePrefab;
        }
        return null;
    }

    protected BaseTile InstantiateTileVisual(int z, int x, BaseTile prefab)
    {
        if (!prefab) return null;

        BaseTile spawnedTile = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
        spawnedTile.transform.localScale = Vector3.one;
        spawnedTile.transform.SetParent(tilesContainer.transform, false);
        spawnedTile.name = $"{prefab.name}_{z}_{x}";
        spawnedTile.z = z;
        spawnedTile.x = x;

        return spawnedTile;
    }

    protected void SpawnTilesVisual()
    {
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                TileObject currentObj = objectsArray[z, x];
                TileObject currentFloor = floorArray[z, x];

                if (currentFloor.tileInstance == null)
                {
                    GenerateFloorTile(z, x, currentFloor);
                }

                if (currentObj.type != TileType.Floor && currentObj.tileInstance == null)
                {
                    if (currentObj.type == currentFloor.type)
                    {
                        currentObj.tileInstance = currentFloor.tileInstance;
                    }
                    else
                    {
                        BaseTile objPrefab = FindTilePrefab(currentObj.type);
                        if (objPrefab != null)
                        {
                            currentObj.tileInstance = InstantiateTileVisual(z, x, objPrefab);
                        }
                    }
                }
                else if (currentObj.type == TileType.Floor)
                {
                    currentObj.tileInstance = currentFloor.tileInstance;
                }
            }
        }
    }

    protected virtual void GenerateFloorTile(int z, int x, TileObject currentFloorData)
    {
        BaseTile prefabToSpawn = FindTilePrefab(currentFloorData.type);

        if (prefabToSpawn == null)
        {
            prefabToSpawn = floorTile != null ? floorTile : FindTilePrefab(TileType.Floor);
            currentFloorData.type = TileType.Floor;
        }

        if (prefabToSpawn != null)
        {
            currentFloorData.tileInstance = InstantiateTileVisual(z, x, prefabToSpawn);
        }
    }

    public void PlaceSpecificTile(int z, int x, TileType tileType, bool placeFloorUnderneath = true)
    {
        BaseTile prefab = FindTilePrefab(tileType);
        if (prefab == null) return;

        objectsArray[z, x].type = tileType;
        objectsArray[z, x].tileInstance = InstantiateTileVisual(z, x, prefab);

        if (placeFloorUnderneath && floorArray[z, x].tileInstance == null)
        {
            GenerateFloorTile(z, x, floorArray[z, x]);
        }
    }
}