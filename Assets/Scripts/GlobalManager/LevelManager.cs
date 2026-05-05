using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelType
{
    MainMenu,
    Hub,
    ResourceExploration,
    PartsExploration,
    CraftingLevel,
    UpgradeLevel
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
        }
    }
}