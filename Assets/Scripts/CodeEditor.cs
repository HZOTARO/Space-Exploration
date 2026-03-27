using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour
{
    PythonExecutor pythonExecutor;

    [Header("References")]
    public TMP_InputField inputField;
    public Button playButton;
    TextMeshProUGUI playButtonText;
    public Button pauseButton;
    TextMeshProUGUI pauseButtonText;
    public Button stepButton;
    TextMeshProUGUI stepButtonText;

    [Header("State")]
    bool isPlaying = false;
    bool isPaused = false;

    void Start()
    {
        if (!pythonExecutor) pythonExecutor = FindAnyObjectByType<PythonExecutor>();

        if (playButton)
        {
            playButtonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            playButtonText.text = "Play";
            playButton.onClick.AddListener(PlayAbort);
        }
        if (pauseButton)
        {
            pauseButtonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            pauseButtonText.text = "Pause";
            pauseButton.onClick.AddListener(PauseContinue);
        }
        if (stepButton)
        {
            stepButtonText = stepButton.GetComponentInChildren<TextMeshProUGUI>();
            stepButtonText.text = "Step";
            stepButton.onClick.AddListener(Step);
        }
    }
    void PlayAbort()
    {
        if (!isPlaying)
        {
            Play();
            isPlaying = true;
            playButtonText.text = "Abort";
        }
        else
        {
            Abort();
            isPlaying = false;
            playButtonText.text = "Play";

            if (isPaused)
            {
                isPaused = false;
                pauseButtonText.text = "Pause";
            }
        }
    }
    void Play()
    {
        pythonExecutor.currentCode = null;
        pythonExecutor.continuous = true;
        pythonExecutor.Exec(inputField.text);
    }

    void Abort()
    {
        pythonExecutor.currentCode = null;
        pythonExecutor.continuous = false;
    }
    void PauseContinue()
    {
        if (!isPlaying) return;

        if (!isPaused)
        {
            Pause();
            isPaused = true;
            pauseButtonText.text = "Continue";
        }
        else
        {
            Continue();
            isPaused = false;
            pauseButtonText.text = "Pause";
        }
    }
    void Pause()
    {
        pythonExecutor.continuous = false;
    }

    void Continue()
    {
        pythonExecutor.continuous = true;
    }
    void Step()
    {
        if (!isPlaying)
        {
            pythonExecutor.continuous = false;
            pythonExecutor.Exec(inputField.text);
        }
    }
}