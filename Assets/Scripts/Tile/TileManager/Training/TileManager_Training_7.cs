using UnityEngine;
using System.Collections.Generic;

public class TileManager_Training_7 : TileManager_Training
{
    [HideInInspector] public List<int> expectedOreValues = new List<int>();
    [HideInInspector] public int numberOfOres;
    List<int> chosenSpaces;

    public override void GenerateMap(bool setAllFloor = true)
    {
        expectedOreValues.Clear();
        base.GenerateMap(false);
        foreach (int x in chosenSpaces)
        {
            TileObject spawnedTile = objectsArray[0, x];

            if (spawnedTile != null && spawnedTile.tileInstance is ValueTile valueTile)
            {
                valueTile.notRandomized = true;
                int forcedTutorialValue = Random.Range(1, 11);
                valueTile.value = forcedTutorialValue;
                expectedOreValues.Add(forcedTutorialValue);
            }
            else
            {
                expectedOreValues.Add(10);
            }
        }
    }

    protected override void GenerateMapContent()
    {
        expectedOreValues.Clear();

        for (int x = 0; x < width; x++)
        {
            SetTile(0, x, TileType.Floor, asFloorToo: true);
        }

        numberOfOres = Random.Range(6, 11);

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