using UnityEngine;

public enum LevelType
{
    MainMenu,
    Hub,
    ResourceExploration,
    PartsExploration,
    CraftingLevel,
    UpgradeLevel,

    Training_1,
    Training_2,
    Training_3,
    Training_4,
    Training_5,
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate LevelManager destroyed!");
            Destroy(gameObject);
        }
    }
    public void OpenScene(LevelType levelType)
    {
        float timeScale = 1.0f;
        if (levelType != LevelType.Hub && levelType != LevelType.MainMenu)
        {
            if (SaveManager.instance)
                timeScale = SaveManager.saveData.gameSpeed;
        }
        Time.timeScale = timeScale;
        switch (levelType)
        {
            case LevelType.MainMenu:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu Scene");
                break;
            case LevelType.Hub:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
                break;
            case LevelType.ResourceExploration:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Resource Exploration Scene");
                break;
            case LevelType.PartsExploration:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Part Exploration Scene");
                break;
            case LevelType.CraftingLevel:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Crafting Scene");
                break;
            case LevelType.UpgradeLevel:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Upgrade Scene");
                break;

            case LevelType.Training_1:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Training Scene 1");
                break;
            case LevelType.Training_2:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Training Scene 2");
                break;
            case LevelType.Training_3:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Training Scene 3");
                break;
            case LevelType.Training_4:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Training Scene 4");
                break;
            case LevelType.Training_5:
                UnityEngine.SceneManagement.SceneManager.LoadScene("Training Scene 5");
                break;
        }
    }
}