using TMPro;
using UnityEngine;
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

    [Header("Slider Mapping Settings")]
    private Vector2 delaySliderRange = new Vector2(0f, 8f);
    private Vector2 actualDelayRange = new Vector2(0f, 0.8f);
    [Space]
    public float[] allowedGameSpeeds = { 0.25f, 0.5f, 0.75f, 1f, 2f, 4f, 8f };

    public void Start()
    {
        float currentStepDelay = 0f;
        float currentGameSpeed = 1f;

        if (SaveManager.saveData != null)
        {
            currentStepDelay = SaveManager.saveData.stepDelay;
            currentGameSpeed = SaveManager.saveData.gameSpeed;
        }

        if (stepDelaySlider)
        {
            float sliderVal = Remap(currentStepDelay, actualDelayRange.x, actualDelayRange.y, delaySliderRange.x, delaySliderRange.y);
            stepDelaySlider.value = Mathf.Round(sliderVal);
            SetStepDelay(sliderVal);
        }

        if (gameSpeedSlider)
        {
            int closestIndex = 3;
            float smallestDiff = Mathf.Abs(currentGameSpeed - allowedGameSpeeds[0]);
            for (int i = 0; i < allowedGameSpeeds.Length; i++)
            {
                float diff = Mathf.Abs(currentGameSpeed - allowedGameSpeeds[i]);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closestIndex = i;
                }
            }
            gameSpeedSlider.value = closestIndex;
            SetGameSpeed(closestIndex);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && enableEscape && pauseUI)
        {
            if (pauseUI.activeSelf) UnPauseGame();
            else PauseGame();
        }
    }


    public void SetStepDelay(float sliderValue)
    {
        float delay = Remap(sliderValue, delaySliderRange.x, delaySliderRange.y, actualDelayRange.x, actualDelayRange.y);

        if (stepDelayText != null)
            stepDelayText.text = $"Step Delay: {delay:0.00}s";

        if (SaveManager.saveData != null)
            SaveManager.saveData.stepDelay = delay;

        if (PythonExecutor.instance)
            PythonExecutor.instance.stepDelay = delay;
    }

    public void SetGameSpeed(float sliderValue)
    {
        int index = Mathf.Clamp(Mathf.RoundToInt(sliderValue), 0, allowedGameSpeeds.Length - 1);

        float speed = allowedGameSpeeds[index];

        if (gameSpeedText != null)
            gameSpeedText.text = $"Game Speed: {speed}x";

        if (SaveManager.saveData != null)
            SaveManager.saveData.gameSpeed = speed;

        Time.timeScale = speed;
    }
    private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float percentage = Mathf.InverseLerp(fromMin, fromMax, value);
        return Mathf.Lerp(toMin, toMax, percentage);
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
}
