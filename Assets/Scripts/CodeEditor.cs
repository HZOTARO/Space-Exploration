using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeEditor : MonoBehaviour
{
    GameManager gameManager;

    [Header("References")]
    public TMP_InputField inputField;
    public Button playButton;
    TextMeshProUGUI playButtonText;
    public Button pauseButton;
    TextMeshProUGUI pauseButtonText;
    public Button stepButton;
    TextMeshProUGUI stepButtonText;

    [Header("State")]
    [HideInInspector]
    public bool isPlaying = false;
    bool isPaused = false;

    private bool aborting = false;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

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

        PythonExecutor.instance.OnExecutionFinished += OnPythonFinished;
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= OnPythonFinished;
        }
    }
    void OnPythonFinished()
    {
        isPlaying = true;
        PlayAbort();
    }

    [HideInInspector]
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
            if (gameManager.InAction()) 
            { 
                aborting = true;
                return;
            }

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
        PythonExecutor.instance.currentCode = null;
        PythonExecutor.instance.continuous = true;
        PythonExecutor.instance.Exec(inputField.text);
    }

    void Abort()
    {
        PythonExecutor.instance.currentCode = null;
        PythonExecutor.instance.continuous = false;
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
        PythonExecutor.instance.continuous = false;
    }

    void Continue()
    {
        PythonExecutor.instance.continuous = true;
    }
    void Step()
    {
        if (!isPlaying)
        {
            PythonExecutor.instance.continuous = false;
            PythonExecutor.instance.Exec(inputField.text);
        }
    }

    private void Update()
    {
        if (aborting && !gameManager.InAction())
        {
            aborting = false;
            playButtonText.text = "Play";

            if (isPaused)
            {
                isPaused = false;
                pauseButtonText.text = "Pause";
            }
        }
    }
}