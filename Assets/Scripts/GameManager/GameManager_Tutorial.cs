using UnityEngine;

public class GameManager_Tutorial : GameManager
{
    [Header("Training Data")]
    public string trainingId = "training_01_movement";

    [Header("Level Completion Objectives")]
    [Tooltip("Comma separated AST nodes required to win. e.g., 'For,List'")]
    public string requiredSyntaxNodes;

    [Tooltip("Leave blank if no specific variable state is required.")]
    public string requiredVariableName; // e.g., "secret_password"
    public string requiredVariableValue; // e.g., "1234"

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

    public void ReachGoal()
    {
        // 1. Tick off the "Reach Goal" objective
        if (ObjectiveManager.instance != null)
        {
            ObjectiveManager.instance.CompleteGoalObjective();

            // 2. Check if the whole list is green!
            if (!ObjectiveManager.instance.AreAllObjectivesComplete())
            {
                Debug.Log("You reached the goal, but you didn't finish your checklist!");
                FindFirstObjectByType<CodeEditor>().ShowError(1, "Checklist incomplete! Make sure all tasks are ticked green.");
                return; // Stop them from winning
            }
        }

        // 3. You Win!
        LevelComplete();
    }
}