using UnityEngine;

public class GameManager_Training : GameManager
{
    [Header("Training Data")]
    public string trainingId = "dummy";

    protected override void Start()
    {
        SetLevelObjectives();
        base.Start();
        ObjectiveManager.instance.InitiateAllTask();
    }
    protected virtual void SetLevelObjectives()
    {
        if (ObjectiveManager.instance == null) return;
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