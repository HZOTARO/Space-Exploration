using UnityEngine;

public class GeneralUIScript: MonoBehaviour
{
    [Header("Escape")]
    public bool enableEscape = true;
    public GameObject pauseUI;
    public void PlayGame()
    {
        SaveManager.instance.LoadGame(SaveManager.saveSlotInUse);
        OpenScene(LevelType.Hub);
    }
    public void BackToMainMenu()
    {
        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        SaveManager.saveData = null;
        OpenScene(LevelType.MainMenu);
    }

    private void OpenScene(LevelType levelType)
    {
        if (LevelManager.instance)
            LevelManager.instance.OpenScene(levelType);
        else
            Debug.LogError("LevelManager instance not found!");
    }
    public void OpenHubScene() => OpenScene(LevelType.Hub);
    public void OpenResourceExplorationScene() => OpenScene(LevelType.ResourceExploration);
    public void OpenPartsExplorationScene() => OpenScene(LevelType.PartsExploration);
    public void OpenCraftingScene() => OpenScene(LevelType.CraftingLevel);
    public void OpenUpgradeScene() => OpenScene(LevelType.UpgradeLevel);

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
    public void SetActive(GameObject UI)
    {
        UI.SetActive(true);
    }
    public void SetInActive(GameObject UI)
    {
        UI.SetActive(false);
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
