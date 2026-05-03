using UnityEngine;

public class TileManager_ResourceExploration : TileManager_Cave
{
    protected override void GenerateMapContent()
    {
        int resourceCount = Mathf.RoundToInt(Random.Range(15f, 25f) / 100 * width * length);
        int whiteOreCount = Mathf.RoundToInt(Random.Range(30f, 40f) / 100 * resourceCount);
        resourceCount -= whiteOreCount;
        int blackOreCount = Mathf.RoundToInt(Random.Range(30f, 40f) / 100 * resourceCount);
        resourceCount -= blackOreCount;
        int veinCount = resourceCount;

        int segmentSize = 5;
        int segmentLength = Mathf.RoundToInt((float)length / segmentSize);
        int segmentLengthRemainder = length % segmentSize;
        int segmentWidth = Mathf.RoundToInt((float)width / segmentSize);
        int segmentWidthRemainder = width % segmentSize;
        int segmentCount = segmentLength * segmentWidth;

        int randZ, randX;
        for (int z = 0; z < segmentLength; z++)
        {
            for (int x = 0; x < segmentWidth; x++)
            {
                int segmentWhiteOreCount = Mathf.RoundToInt((float)whiteOreCount / segmentCount);
                whiteOreCount -= segmentWhiteOreCount;

                int segmentBlackOreCount = Mathf.RoundToInt((float)blackOreCount / segmentCount);
                blackOreCount -= segmentBlackOreCount;

                int segmentVeinCount = Mathf.RoundToInt((float)veinCount / segmentCount);
                veinCount -= segmentVeinCount;

                while (segmentVeinCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor)
                    {
                        objectsArray[randZ, randX].type = TileType.PurpleVein;
                        segmentVeinCount--;
                    }
                }

                while (segmentWhiteOreCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor)
                    {
                        objectsArray[randZ, randX].type = TileType.WhiteOre;
                        segmentWhiteOreCount--;
                    }
                }

                while (segmentBlackOreCount > 0)
                {
                    randZ = Random.Range(0, z == 0 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == 0 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor)
                    {
                        objectsArray[randZ, randX].type = TileType.BlackOre;
                        segmentBlackOreCount--;
                    }
                }

                segmentCount--;
            }
        }
    }
}
