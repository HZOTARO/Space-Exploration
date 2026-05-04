using UnityEngine;

public class TileManager_ResourceExploration : TileManager_Cave
{
    protected override void GenerateMapContent()
    {
        bool whiteOreUnlocked = true;
        
        bool blackOreUnlocked = UpgradeManager.instance.IsUpgradeUnlocked("black_ore");
        bool purpleVeinUnlocked = UpgradeManager.instance.IsUpgradeUnlocked("purple_liquid");

        int totalResourceCount = Mathf.RoundToInt(Random.Range(15f, 25f) / 100f * width * length);
        int remainingResources = totalResourceCount;

        int whiteOreCount = 0;
        int blackOreCount = 0;
        int veinCount = 0;

        int unlockedTypes = (whiteOreUnlocked ? 1 : 0) + (blackOreUnlocked ? 1 : 0) + (purpleVeinUnlocked ? 1 : 0);

        float minPercent = (unlockedTypes == 2) ? 40f : 30f;
        float maxPercent = (unlockedTypes == 2) ? 60f : 40f;

        if (unlockedTypes > 0)
        {
            if (whiteOreUnlocked)
            {
                unlockedTypes--;
                whiteOreCount = (unlockedTypes == 0) ? remainingResources : Mathf.RoundToInt(Random.Range(minPercent, maxPercent) / 100f * remainingResources);
                remainingResources -= whiteOreCount;
            }

            if (blackOreUnlocked)
            {
                unlockedTypes--;
                blackOreCount = (unlockedTypes == 0) ? remainingResources : Mathf.RoundToInt(Random.Range(minPercent, maxPercent) / 100f * remainingResources);
                remainingResources -= blackOreCount;
            }

            if (purpleVeinUnlocked)
            {
                veinCount = remainingResources;
            }
        }

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
                    randZ = Random.Range(0, z == segmentLength - 1 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == segmentWidth - 1 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor)
                    {
                        floorArray[randZ, randX].type = TileType.PurpleVein;
                        objectsArray[randZ, randX].type = TileType.PurpleVein;

                        segmentVeinCount--;
                    }
                }

                while (segmentWhiteOreCount > 0)
                {
                    randZ = Random.Range(0, z == segmentLength - 1 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == segmentWidth - 1 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

                    if (objectsArray[randZ, randX].type == TileType.Floor)
                    {
                        objectsArray[randZ, randX].type = TileType.WhiteOre;

                        segmentWhiteOreCount--;
                    }
                }

                while (segmentBlackOreCount > 0)
                {
                    randZ = Random.Range(0, z == segmentLength - 1 ? segmentSize + segmentLengthRemainder : segmentSize) + z * segmentSize;
                    randX = Random.Range(0, x == segmentWidth - 1 ? segmentSize + segmentWidthRemainder : segmentSize) + x * segmentSize;

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
