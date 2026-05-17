using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CodeEditor))]
public class CodeExecutionController : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button pauseButton;
    public Button stepButton;

    private TextMeshProUGUI playButtonText;
    private TextMeshProUGUI pauseButtonText;
    private TextMeshProUGUI stepButtonText;

    [Header("State")]
    [HideInInspector] public bool isPlaying = false;
    [HideInInspector] public bool isPaused = false;
    [HideInInspector] public bool aborting = false;

    private VariableWatcher variableWatcher;
    private CodeEditor ui;

    void Awake()
    {
        ui = GetComponent<CodeEditor>();
        variableWatcher = GetComponent<VariableWatcher>();
    }

    void Start()
    {
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
        PythonExecutor.instance.OnLineExecuted += HandleLineExecution;
        PythonExecutor.instance.OnRuntimeError += HandleRuntimeError;

        if (ui.inputField != null)
        {
            ui.inputField.onValueChanged.AddListener((text) => FastAbortCheck());
        }
    }

    void Update()
    {
        if (aborting && ui.gameManager != null && !ui.gameManager.InAction())
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

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= OnPythonFinished;
            PythonExecutor.instance.OnLineExecuted -= HandleLineExecution;
            PythonExecutor.instance.OnRuntimeError -= HandleRuntimeError;
        }
    }

    #region --- EXECUTION LOGIC ---

    void PlayAbort()
    {
        if (!isPlaying)
        {
            if (Play())
            {
                isPlaying = true;
                playButtonText.text = "Abort";
            }
        }
        else
        {
            Abort();
            isPlaying = false;
            if (ui.gameManager != null && ui.gameManager.InAction())
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

    bool Play()
    {
        ui.HideError();
        PythonValidationResult result = PythonExecutor.instance.ValidateCode(ui.inputField.text);

        if (!result.is_valid)
        {
            isPlaying = false;
            ui.ShowError(result.line, result.error_msg);
            return false;
        }

        PythonExecutor.instance.currentCode = null;
        PythonExecutor.instance.continuous = true;
        PythonExecutor.instance.Exec(ui.inputField.text);
        return true;
    }

    void Abort()
    {
        PythonExecutor.instance.StopRunningCode();
        ui.HideError();

        if (variableWatcher != null) variableWatcher.ResetAllToUndefined();
    }

    public void FastAbortCheck()
    {
        if (isPlaying || isPaused)
        {
            Abort();
            isPlaying = false;

            if (ui.gameManager != null && ui.gameManager.InAction()) aborting = true;
            else if (playButtonText != null) playButtonText.text = "Play";

            if (isPaused)
            {
                isPaused = false;
                if (pauseButtonText != null) pauseButtonText.text = "Pause";
            }
        }
    }

    void PauseContinue()
    {
        if (!isPlaying) return;

        if (!isPaused)
        {
            PythonExecutor.instance.continuous = false;
            isPaused = true;
            pauseButtonText.text = "Continue";
        }
        else
        {
            PythonExecutor.instance.continuous = true;
            isPaused = false;
            pauseButtonText.text = "Pause";
        }
    }

    void Step()
    {
        if (!isPlaying)
        {
            ui.HideError();
            PythonValidationResult result = PythonExecutor.instance.ValidateCode(ui.inputField.text);
            if (!result.is_valid)
            {
                ui.ShowError(result.line, result.error_msg);
                return;
            }

            isPlaying = true;
            isPaused = true;
            playButtonText.text = "Abort";
            pauseButtonText.text = "Continue";

            PythonExecutor.instance.continuous = false;
            PythonExecutor.instance.Exec(ui.inputField.text);
        }
        else if (isPaused)
        {
            PythonExecutor.instance.Exec(ui.inputField.text);
        }
    }

    #endregion

    #region --- PYTHON CALLBACKS ---

    private void HandleLineExecution(int startLine, int endLine)
    {
        ui.TriggerHighlight(startLine, endLine);
    }

    private void HandleRuntimeError(int line, string errorMessage)
    {
        PythonExecutor.instance.StopRunningCode();
        isPlaying = false;

        if (ui.gameManager != null && ui.gameManager.InAction()) aborting = true;
        else
        {
            if (playButtonText != null) playButtonText.text = "Play";
            if (isPaused)
            {
                isPaused = false;
                if (pauseButtonText != null) pauseButtonText.text = "Pause";
            }
        }

        ui.ShowError(line, "Runtime Error: " + errorMessage);
    }

    private void OnPythonFinished()
    {
        isPlaying = false;

        if (playButtonText != null) playButtonText.text = "Play";

        if (isPaused)
        {
            isPaused = false;
            if (pauseButtonText != null) pauseButtonText.text = "Pause";
        }

        ui.RemoveHighlight();
    }

    #endregion
}