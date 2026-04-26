using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

[System.Serializable]
public struct SyntaxGroup
{
    public string groupName;
    public Color color;
    public string[] keywords;
}

public class CodeEditor : MonoBehaviour
{
    GameManager gameManager;

    [Header("References")]
    public TMP_InputField inputField;

    [Header("Highlight System")]
    public Image highlightImage;
    private float currentHighlightCenterY = 0f;

    [Header("Syntax Color")]
    public TextMeshProUGUI syntaxOverlayText;
    private SyntaxGroup[] syntaxGroups;

    [Header("Editor Settings")]
    public int tabSize = 2;

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

    // For text change detection
    private string lastKnownText = "";
    private float updateDelay = 0.15f;
    private float currentUpdateTimer = 0f;
    private bool needsHeavyUpdate = false;

    void Start()
    {
        InitializeSyntaxGroups();

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

        RemoveHighlight();
    }

    private void InitializeSyntaxGroups()
    {
        syntaxGroups = new SyntaxGroup[]
        {
            new SyntaxGroup
            {
                groupName = "Control Flow",
                color = new Color(0.85f, 0.43f, 0.83f),
                keywords = new string[] { "for", "in", "while", "if", "else", "elif", "return", "def", "class" }
            },
            new SyntaxGroup
            {
                groupName = "Logic",
                color = new Color(0.33f, 0.66f, 1f),
                keywords = new string[] { "and", "or", "not", "is" }
            },
            new SyntaxGroup
            {
                groupName = "Booleans & Types",
                color = new Color(1f, 0.64f, 0f),
                keywords = new string[] { "True", "False", "None", "int", "float", "str", "bool" }
            },
            new SyntaxGroup
            {
                groupName = "Built-in Functions",
                color = new Color(0.86f, 0.86f, 0.67f),
                keywords = new string[] { "print", "range", "len", "type" }
            }
        };
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

        lineNumbersText.text = numbers.ToString();
    }

    private void TriggerHighlight(int startLogicalLine, int endLogicalLine)
    {
        if (highlightImage == null || inputField == null) return;

        highlightImage.transform.SetAsFirstSibling();

        TMP_TextInfo textInfo = inputField.textComponent.textInfo;
        string rawText = inputField.text;

        if (textInfo.characterCount == 0 || string.IsNullOrEmpty(rawText)) return;

        int currentLogicalLine = 1;
        int visualStartLine = -1;
        int visualEndLine = -1;

        for (int i = 0; i < textInfo.lineCount; i++)
        {
            int firstCharIdx = textInfo.lineInfo[i].firstCharacterIndex;

            bool isNewLogicalLine = false;

            if (firstCharIdx == 0)
            {
                isNewLogicalLine = true;
            }
            else if (firstCharIdx > 0 && firstCharIdx <= rawText.Length)
            {
                char prevChar = rawText[firstCharIdx - 1];
                isNewLogicalLine = (prevChar == '\n' || prevChar == '\r');
            }

            if (isNewLogicalLine)
            {
                if (currentLogicalLine == startLogicalLine) visualStartLine = i;
                if (currentLogicalLine == endLogicalLine) visualEndLine = i;
                currentLogicalLine++;
            }
            else
            {
                if (currentLogicalLine - 1 == endLogicalLine) visualEndLine = i;
            }
        }

        if (visualStartLine == -1) visualStartLine = 0;
        if (visualEndLine == -1) visualEndLine = textInfo.lineCount - 1;

        float topY = textInfo.lineInfo[visualStartLine].ascender;
        float bottomY = textInfo.lineInfo[visualEndLine].descender;

        float totalLineSize = topY - bottomY;

        currentHighlightCenterY = (topY + bottomY) / 2f;

        highlightImage.rectTransform.sizeDelta = new Vector2(highlightImage.rectTransform.sizeDelta.x, totalLineSize);
        highlightImage.rectTransform.localPosition = new Vector3(
            highlightImage.rectTransform.localPosition.x,
            inputField.textComponent.transform.localPosition.y + currentHighlightCenterY,
            0f
        );

        Color c = highlightImage.color;
        c.a = 1f;
        highlightImage.color = c;
    }

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
    bool Play()
    {
        PythonValidationResult result = PythonExecutor.instance.ValidateCode(inputField.text);

        if (!result.is_valid)
        {
            isPlaying = false;
            gameManager.PrintToDisplay($"<color=red>Error on line {result.line}: {result.error_msg}</color>");
            TriggerHighlight(result.line, result.line);
            return false;
        }

        PythonExecutor.instance.currentCode = null;
        PythonExecutor.instance.continuous = true;
        PythonExecutor.instance.Exec(inputField.text);
        return true;
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
            PythonValidationResult result = PythonExecutor.instance.ValidateCode(inputField.text);
            if (!result.is_valid)
            {
                gameManager.PrintToDisplay($"<color=red>Error on line {result.line}: {result.error_msg}</color>");

                TriggerHighlight(result.line, result.line);
                return;
            }

            PythonExecutor.instance.continuous = false;
            PythonExecutor.instance.Exec(inputField.text);
        }
    }

    private void Update()
    {
        // Halt aborting text change
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

        // Handle tab key replacement
        if (inputField != null && inputField.text.Contains("\t"))
        {
            int caretPos = Mathf.Clamp(inputField.caretPosition, 0, inputField.text.Length);

            string textBeforeCaret = inputField.text.Substring(0, caretPos);
            int tabsBeforeCaret = textBeforeCaret.Split('\t').Length - 1;

            string spaces = new string(' ', tabSize);
            inputField.text = inputField.text.Replace("\t", spaces);

            inputField.caretPosition = caretPos + (tabsBeforeCaret * (tabSize - 1));
        }

        // Detect text changes
        if (inputField != null && inputField.text != lastKnownText)
        {
            lastKnownText = inputField.text;

            UpdateSyntaxHighlighting(lastKnownText);
            RemoveHighlight();

            FastAbortCheck();

            needsHeavyUpdate = true;
            currentUpdateTimer = updateDelay;
        }

        if (needsHeavyUpdate)
        {
            currentUpdateTimer -= Time.deltaTime;

            if (currentUpdateTimer <= 0f)
            {
                needsHeavyUpdate = false;
                RunHeavyUIUpdates(lastKnownText);
            }
        }

        // Sync line numbers and highlight with scrolling
        if (inputField != null)
        {
            float scrollY = inputField.textComponent.rectTransform.anchoredPosition.y;

            if (lineNumbersText != null)
            {
                Vector2 numberPos = lineNumbersText.rectTransform.anchoredPosition;
                numberPos.y = scrollY;
                lineNumbersText.rectTransform.anchoredPosition = numberPos;
            }

            if (highlightImage != null && highlightImage.color.a > 0f)
            {
                Vector3 highlightPos = highlightImage.rectTransform.localPosition;
                highlightPos.y = scrollY + currentHighlightCenterY;
                highlightImage.rectTransform.localPosition = highlightPos;
            }
        }
    }

    private void UpdateSyntaxHighlighting(string rawText)
    {
        if (syntaxOverlayText == null || syntaxGroups == null) return;

        string coloredText = rawText;

        foreach (SyntaxGroup group in syntaxGroups)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(group.color);

            foreach (string word in group.keywords)
            {
                string pattern = @"\b" + word + @"\b";

                coloredText = Regex.Replace(coloredText, pattern, $"<color=#{hexColor}>{word}</color>");
            }
        }

        syntaxOverlayText.text = coloredText;
    }

    private void FastAbortCheck()
    {
        if (isPlaying || isPaused)
        {
            Abort();
            isPlaying = false;

            if (gameManager.InAction()) { aborting = true; }
            else { playButtonText.text = "Play"; }

            if (isPaused)
            {
                isPaused = false;
                pauseButtonText.text = "Pause";
            }
        }
    }

    private void RunHeavyUIUpdates(string currentText)
    {
        UpdateLineNumbers(currentText);
    }

    void RemoveHighlight()
    {
        if (highlightImage == null) return;
        Color c = highlightImage.color;
        c.a = 0f;
        highlightImage.color = c;
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