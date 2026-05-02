using UnityEngine;
using System;

public class GameManager_Tutorial : GameManager
{
    [Header("Lecture Specifics")]
    public string lectureWinCondition; // e.g., "Reached the green tile"

    // IMPORTANT: By overriding this and leaving it empty, we stop the Base GameManager 
    // from destroying your hand-built Unity scene map!
    protected override void SetupMap()
    {
        // Do nothing! The map is already built in the Unity Scene.
        Debug.Log("Lecture Level: Using pre-built map.");
    }

    protected override void RegisterLevelSpecificPythonCommands()
    {
        // Lectures might just use the basic movement commands, or introduce a specific tool
        // for the very first time!
    }

    // You could override Return() or create a custom method to check if the student passed the lecture
    public void CheckLectureComplete()
    {
        //TileObject currentTile = GetCurrentTile();
        //if (currentTile.type == TileType.Objective)
        //{
        //    Debug.Log("Lecture Passed!");
        //    LevelComplete();
        //}
    }
}