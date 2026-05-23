using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileManager_Training_11 : TileManager_Training
{
    [HideInInspector] public List<int> expectedOreValues = new List<int>();
    [HideInInspector] public int numberOfOres = 6;
    [HideInInspector] public int expectedTotalValue;

    private List<int> chosenSpaces = new List<int>();

    public override void GenerateMap(bool setAllFloor = true)
    {
        expectedOreValues.Clear();
        expectedTotalValue = 0;

        base.GenerateMap(false);

        foreach (int x in chosenSpaces)
        {
            TileObject spawnedTile = objectsArray[0, x];

            if (spawnedTile != null && spawnedTile.tileInstance is ValueTile valueTile)
            {
                valueTile.notRandomized = true;
                int forcedTutorialValue = Random.Range(1, 100);
                valueTile.value = forcedTutorialValue;
                expectedOreValues.Add(forcedTutorialValue);
            }
            else
            {
                expectedOreValues.Add(10);
            }
        }

        int targetHalfCount = numberOfOres / 2;

        expectedTotalValue = expectedOreValues
                    .OrderByDescending(val => val)
                    .Take(targetHalfCount)
                    .Sum();
    }

    protected override void GenerateMapContent()
    {
        for (int x = 0; x < width; x++)
        {
            SetTile(0, x, TileType.Floor, asFloorToo: true);
        }

        List<int> availableSpaces = new List<int>();
        for (int i = 1; i < width - 1; i++)
        {
            availableSpaces.Add(i);
        }

        chosenSpaces = new List<int>();

        for (int i = 0; i < numberOfOres; i++)
        {
            if (availableSpaces.Count == 0) break;

            int randomIndex = Random.Range(0, availableSpaces.Count);
            chosenSpaces.Add(availableSpaces[randomIndex]);
            availableSpaces.RemoveAt(randomIndex);
        }

        chosenSpaces.Sort();

        foreach (int x in chosenSpaces)
        {
            SetTile(0, x, TileType.WhiteOre, asFloorToo: true);
        }
    }
}