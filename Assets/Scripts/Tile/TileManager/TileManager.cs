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

    public virtual void GenerateMap(bool setAllFloor = true)
    {
        if (!tilesContainer) return;

        objectsArray = new TileObject[length, width];
        floorArray = new TileObject[length, width];

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                objectsArray[z, x] = new TileObject();
                objectsArray[z, x].type = setAllFloor ? TileType.Floor : TileType.None;

                floorArray[z, x] = new TileObject();
                floorArray[z, x].type = setAllFloor ? TileType.Floor : TileType.None;
            }
        }

        objectsArray[0, 0].type = TileType.None;

        GenerateMapContent();

        objectsArray[0, 0].type = TileType.Floor;

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

                if (currentObj.type == TileType.None || currentFloor.type == TileType.None)
                {
                    continue;
                }

                // Floor
                if (currentFloor.type == TileType.Floor)
                {
                    GenerateFloorTile(z, x, currentFloor);
                }
                else
                {
                    currentFloor.tileInstance = InstantiateTileVisual(z, x, FindTilePrefab(currentFloor.type));
                }

                // Object
                if (currentObj.type == currentFloor.type)
                {
                    currentObj.tileInstance = currentFloor.tileInstance;
                }
                else
                {
                    currentObj.tileInstance = InstantiateTileVisual(z, x, FindTilePrefab(currentObj.type));
                }
            }
        }
    }

    protected virtual void GenerateFloorTile(int z, int x, TileObject currentFloorData)
    {
        currentFloorData.tileInstance = InstantiateTileVisual(z, x, (floorTile) ? floorTile : FindTilePrefab(TileType.Floor));
    }
}