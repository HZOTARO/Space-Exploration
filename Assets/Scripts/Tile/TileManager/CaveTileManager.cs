using UnityEngine;

public class CaveTileManager : TileManager
{
    override protected void GenerateMap()
    {
        base.GenerateMap();

        float resourcePercentage = Random.Range(15f, 25f);
        int resourceCount = Mathf.RoundToInt(resourcePercentage / 100 * width * length);
        //Debug.Log($"Resource Percentage: {resourcePercentage / 100}%, Resource Count: {resourceCount}");
        float orePercentage = Random.Range(40f, 60f);
        int oreCount = Mathf.RoundToInt(orePercentage / 100 * resourceCount);
        //Debug.Log($"Ore Percentage: {orePercentage / 100}%, Ore Count: {oreCount}");
        int mineralCount = resourceCount - oreCount;
        //Debug.Log($"Mineral Count: {mineralCount}");

        int segmentSize = 5;
        int segmentLength = Mathf.RoundToInt(length / segmentSize);
        int segmentLengthRemainder = length % segmentSize;
        int segmentWidth = Mathf.RoundToInt(width / segmentSize);
        int segmentWidthRemainder = width % segmentSize;
        int segmentCount = segmentLength * segmentWidth;
        //Debug.Log($"Segment Size: {segmentSize}, Segment Length: {segmentLength}, Segment Length Remainder: {segmentLengthRemainder}, Segment Width: {segmentWidth}, Segment Width Remainder: {segmentWidthRemainder}, Segment Count: {segmentCount}");

        int randZ, randX;
        for (int z = 0; z < segmentLength; z++)
        {
            for (int x = 0; x < segmentWidth; x++)
            {
                int segmentMineralCount = Mathf.RoundToInt((float)mineralCount / segmentCount);
                mineralCount -= segmentMineralCount;

                int segmentOreCount = Mathf.RoundToInt((float)oreCount / segmentCount);
                oreCount -= segmentOreCount;

                currentTilePrefab = FindTilePrefab(TileType.PurpleVein);
                while (segmentMineralCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (gridArray[randZ, randX].type == TileType.Default)
                    {
                        gridArray[randZ, randX].type = TileType.PurpleVein;
                        gridArray[randZ, randX].tileInstance = SpawnTileVisual(randZ, randX, currentTilePrefab);
                        segmentMineralCount--;
                    }
                }

                currentTilePrefab = FindTilePrefab(TileType.WhiteOre);
                while (segmentOreCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (gridArray[randZ, randX].type == TileType.Default)
                    {
                        gridArray[randZ, randX].type = TileType.WhiteOre;
                        gridArray[randZ, randX].tileInstance = SpawnTileVisual(randZ, randX, currentTilePrefab);
                        segmentOreCount--;
                    }
                }

                segmentCount--;
            }
        }

        SpawnTilesVisual();
    }
}
