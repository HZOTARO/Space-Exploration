using UnityEngine;

public class GeneralUIScript: MonoBehaviour
{
    [Header("Escape")]
    public bool enableEscape = true;
    public GameObject pauseUI;
    public void PlayGame()
    {
        SaveManager.instance.LoadGame(1);
        OpenHubScene();
    }
    public void BackToMainMenu()
    {
        SaveManager.instance.SaveGame(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
    public void OpenHubScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Hub Scene");
    }
    public void OpenResourceExplorationScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Cave Scene");
    }
    public void OpenPartsExplorationScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Ruins Scene");
    }
    public void OpenCraftingScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Crafting Scene");
    }
    public void OpenUpgradeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Upgrade Scene");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && enableEscape && pauseUI)
        {
            if (pauseUI.activeSelf)
                UnPauseGame();
            else
                PauseGame();
        }
    }
    public void PauseGame()
    {
        pauseUI.SetActive(false);
    }
    public void UnPauseGame()
    {
        pauseUI.SetActive(true);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
