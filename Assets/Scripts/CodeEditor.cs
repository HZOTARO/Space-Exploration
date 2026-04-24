using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;

public class CodeEditor : MonoBehaviour
{
    GameManager gameManager;

    [Header("References")]
    public TMP_InputField inputField;

    [Header("Highlight System")]
    public Image highlightImage;

    [Header("Line Number UI")]
    public TextMeshProUGUI lineNumbersText;

    [Header("Buttons")]
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

    private string lastKnownText = "";

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (inputField != null)
        {
            UpdateLineNumbers(inputField.text);
        }

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
        PythonExecutor.instance.OnLineExecuted += TriggerHighlight;

        if (highlightImage)
        {
            Color c = highlightImage.color;
            c.a = 0f;
            highlightImage.color = c;
        }
    }

    private void UpdateLineNumbers(string currentText)
    {
        if (lineNumbersText == null || inputField == null) return;

        if (string.IsNullOrEmpty(currentText))
        {
            lineNumbersText.text = "1";
            return;
        }

        inputField.textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = inputField.textComponent.textInfo;

        StringBuilder numbers = new StringBuilder();
        int currentLogicalLine = 1;

        for (int i = 0; i < textInfo.lineCount; i++)
        {
            int firstCharIdx = textInfo.lineInfo[i].firstCharacterIndex;

            bool isNewLogicalLine = false;

            if (firstCharIdx == 0)
            {
                isNewLogicalLine = true;
            }

            else if (firstCharIdx > 0 && firstCharIdx <= currentText.Length)
            {
                char prevChar = currentText[firstCharIdx - 1];
                isNewLogicalLine = (prevChar == '\n' || prevChar == '\r');
            }

            if (isNewLogicalLine)
            {
                numbers.AppendLine(currentLogicalLine.ToString());
                currentLogicalLine++;
            }
            else
            {
                numbers.AppendLine("");
            }
        }

        //if (currentText.Length > 0)
        //{
        //    char lastChar = currentText[currentText.Length - 1];
        //    if (lastChar == '\n' || lastChar == '\r')
        //    {
        //        numbers.AppendLine(currentLogicalLine.ToString());
        //    }
        //}

        lineNumbersText.text = numbers.ToString();
    }

    private void TriggerHighlight(int startLogicalLine, int endLogicalLine)
    {
        if (highlightImage == null || inputField == null) return;

        highlightImage.transform.SetAsFirstSibling();

        inputField.textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = inputField.textComponent.textInfo;
        string rawText = inputField.text;

        if (textInfo.characterCount == 0 || string.IsNullOrEmpty(rawText)) return;

        int startChar = GetCharacterIndexFromLine(rawText, startLogicalLine);
        int endChar = GetCharacterIndexFromLine(rawText, endLogicalLine + 1) - 1;

        startChar = Mathf.Clamp(startChar, 0, textInfo.characterCount - 1);
        endChar = Mathf.Clamp(endChar, 0, textInfo.characterCount - 1);

        int visualStartLine = textInfo.characterInfo[startChar].lineNumber;
        int visualEndLine = textInfo.characterInfo[endChar].lineNumber;

        float topY = textInfo.lineInfo[visualStartLine].ascender;
        float bottomY = textInfo.lineInfo[visualEndLine].descender;

        float totalLineSize = topY - bottomY;
        float localCenterY = (topY + bottomY) / 2f;

        highlightImage.rectTransform.sizeDelta = new Vector2(highlightImage.rectTransform.sizeDelta.x, totalLineSize);
        highlightImage.rectTransform.localPosition = new Vector3(
            highlightImage.rectTransform.localPosition.x,
            inputField.textComponent.transform.localPosition.y + localCenterY,
            0f
        );

        Color c = highlightImage.color;
        c.a = 1f;
        highlightImage.color = c;
    }

    private int GetCharacterIndexFromLine(string text, int targetLine)
    {
        if (targetLine <= 1) return 0;
        int currentLine = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                currentLine++;
                if (currentLine == targetLine) return i + 1;
            }
        }
        return text.Length - 1;
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
        if (inputField != null && inputField.text != lastKnownText)
        {
            lastKnownText = inputField.text;
            OnCodeEdited(lastKnownText);
        }
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

        if (inputField != null && lineNumbersText != null)
        {
            float scrollY = inputField.textComponent.rectTransform.anchoredPosition.y;

            Vector2 numberPos = lineNumbersText.rectTransform.anchoredPosition;
            numberPos.y = scrollY;
            lineNumbersText.rectTransform.anchoredPosition = numberPos;
        }
    }
    private void OnCodeEdited(string currentText)
    {
        UpdateLineNumbers(currentText);

        if (isPlaying || isPaused)
        {
            Abort();
            isPlaying = false;

            if (gameManager.InAction())
            {
                aborting = true;
            }
            else
            {
                playButtonText.text = "Play";
            }

            if (isPaused)
            {
                isPaused = false;
                pauseButtonText.text = "Pause";
            }

            if (highlightImage != null)
            {
                Color c = highlightImage.color;
                c.a = 0f;
                highlightImage.color = c;
            }
        }
    }
    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnExecutionFinished -= OnPythonFinished;
            PythonExecutor.instance.OnLineExecuted -= TriggerHighlight;
        }
    }
    void OnPythonFinished()
    {
        isPlaying = true;
        PlayAbort();
    }
}