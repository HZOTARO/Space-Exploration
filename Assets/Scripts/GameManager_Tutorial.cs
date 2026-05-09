using System.Collections.Generic;
using UnityEngine;

public class GameManager_Tutorial : GameManager
{
    [Header("Training Data")]
    public string trainingId = "training_01_movement";

    protected override void SetLevelAllowedSyntax()
    {
        //allowedSyntaxNodes.AddRange(SyntaxDictionary.Variables);
    }

    protected override void SetupMap()
    {
        tileManager.GenerateMap();
    }

    protected override void Start()
    {
        levelSize = 5;
        inventorySize = 0;
        base.Start();
    }

    protected override void LevelComplete()
    {
        // Mark this training module as completed in the save data!
        //if (!SaveManager.saveData.completedTrainings.Contains(trainingId))
        //{
        //    SaveManager.saveData.completedTrainings.Add(trainingId);
        //    SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        //}

        Debug.Log("<color=green>Training Completed!</color>");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
}