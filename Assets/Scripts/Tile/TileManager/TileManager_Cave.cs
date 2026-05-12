using UnityEngine;

public class TileManager_Cave : TileManager
{
    public override void GenerateMap(bool setAllFloor = true)
    {
        if (!tilesContainer) return;

        if (grid)
        {
            GameObject spawnedGrid = Instantiate(grid, new Vector3(0, 0, -0.005f), Quaternion.identity);
            spawnedGrid.transform.SetParent(tilesContainer.transform, false);
            spawnedGrid.transform.localScale = new Vector3(width, 1, length);
            spawnedGrid.name = "Grid";
        }

        base.GenerateMap();
    }
}