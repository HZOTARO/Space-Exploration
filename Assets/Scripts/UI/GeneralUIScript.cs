using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GeneralUIScript : MonoBehaviour
{
    [Header("Escape")]
    public bool enableEscape = true;
    public GameObject pauseUI;

    [Header("Sliders References")]
    public TextMeshProUGUI stepDelayText;
    public Slider stepDelaySlider;
    public TextMeshProUGUI gameSpeedText;
    public Slider gameSpeedSlider;

    public void Start()
    {
        if (stepDelaySlider)
            stepDelaySlider.value = SaveManager.saveData != null ? SaveManager.saveData.stepDelay * 20f : 0f;
        if (stepDelayText)
            SetStepDelay(SaveManager.saveData != null ? SaveManager.saveData.stepDelay * 20f : 0f);
        if (gameSpeedSlider)
            gameSpeedSlider.value = SaveManager.saveData != null ? SaveManager.saveData.gameSpeed : 1f;
        if (gameSpeedText)
            SetGameSpeed(SaveManager.saveData != null ? SaveManager.saveData.gameSpeed : 1f);
    }

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
        pauseUI.SetActive(true);
    }
    public void UnPauseGame()
    {
        pauseUI.SetActive(false);
    }
    public void SetActive(GameObject UI)
    {
        UI.SetActive(true);
    }
    public void SetInActive(GameObject UI)
    {
        UI.SetActive(false);
    }

    public void DeleteSaveData()
    {
        SaveManager.instance.DeleteSave(SaveManager.saveSlotInUse);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetStepDelay(float delay)
    {
        delay = delay / 20f;

        if (stepDelayText != null)
            stepDelayText.text = $"Step Delay: {delay:0.00}s";

        if (SaveManager.instance)
            SaveManager.saveData.stepDelay = delay;

        if (PythonExecutor.instance)
            PythonExecutor.instance.stepDelay = delay;
    }

    public void SetGameSpeed(float speed)
    {
        if (gameSpeedText != null)
            gameSpeedText.text = $"Game Speed: {speed:0.00}x";

        if (SaveManager.instance)
            SaveManager.saveData.gameSpeed = speed;

        if (SceneManager.GetActiveScene().name != "Hub Scene")
            Time.timeScale = speed;
    }
}
