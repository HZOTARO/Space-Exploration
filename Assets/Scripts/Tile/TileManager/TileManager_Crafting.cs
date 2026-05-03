using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftingRequirement
{
    public TileType tileType;
    public int requiredAmount;
}

public class TileManager_Crafting : TileManager_Spaceship
{
    [Header("Crafting Settings")]
    public TileType machineTileType;
    public Vector2Int machineLocation = new Vector2Int(4, 4);

    [Tooltip("How much EXTRA material to spawn. 1.5 = 50% extra items on the map.")]
    public float abundanceMultiplier = 1.5f;

    public List<CraftingRequirement> recipeRequirements;

    protected override void GenerateMapContent()
    {
        objectsArray[machineLocation.y, machineLocation.x].type = machineTileType;

        if (recipeRequirements != null)
        {
            foreach (CraftingRequirement req in recipeRequirements)
            {
                SpreadIngredient(req);
            }
        }
    }

    private void SpreadIngredient(CraftingRequirement req)
    {
        int totalToSpawn = Mathf.RoundToInt(req.requiredAmount * abundanceMultiplier);

        while (totalToSpawn > 0)
        {
            Vector2Int pos = GetRandomEmptySpot();

            if (pos.x == -1) break;

            objectsArray[pos.y, pos.x].type = req.tileType;
            totalToSpawn--;
        }
    }

    private Vector2Int GetRandomEmptySpot()
    {
        for (int i = 0; i < 100; i++)
        {
            int randZ = Random.Range(0, length);
            int randX = Random.Range(0, width);

            if (objectsArray[randZ, randX].type == TileType.Floor && !(randZ == 0 && randX == 0))
            {
                return new Vector2Int(randX, randZ);
            }
        }
        return new Vector2Int(-1, -1);
    }
}