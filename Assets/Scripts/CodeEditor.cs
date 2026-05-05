using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        gameManager = FindFirstObjectByType<GameManager>();

        List<string> bannedKeywordsForUI = new List<string>();

        InitializeSyntaxGroups();

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

    public void InitializeSyntaxGroups()
    {
        List<string> controlFlow = new List<string>();
        List<string> logicAndTypes = new List<string> { "True", "False", "None" };
        List<string> gameCommands = new List<string>();
        List<string> builtInFunctions = new List<string>();
        List<string> listMethods = new List<string>();

        if (gameManager != null)
        {
            if (gameManager.allowedSyntaxNodes.Contains("For"))
            {
                controlFlow.Add("for");
                controlFlow.Add("in");
            }
            if (gameManager.allowedSyntaxNodes.Contains("While")) controlFlow.Add("while");
            if (gameManager.allowedSyntaxNodes.Contains("Break")) controlFlow.Add("break");
            if (gameManager.allowedSyntaxNodes.Contains("Continue")) controlFlow.Add("continue");

            if (gameManager.allowedSyntaxNodes.Contains("If"))
            {
                controlFlow.Add("if");
                controlFlow.Add("else");
                controlFlow.Add("elif");
            }
            if (gameManager.allowedSyntaxNodes.Contains("Match"))
            {
                controlFlow.Add("match");
                controlFlow.Add("case");
            }
            if (gameManager.allowedSyntaxNodes.Contains("Pass")) controlFlow.Add("pass");

            if (gameManager.allowedSyntaxNodes.Contains("FunctionDef"))
            {
                controlFlow.Add("def");
                controlFlow.Add("return");
            }
            if (gameManager.allowedSyntaxNodes.Contains("ClassDef")) controlFlow.Add("class");
            if (gameManager.allowedSyntaxNodes.Contains("Yield")) controlFlow.Add("yield");
            if (gameManager.allowedSyntaxNodes.Contains("Lambda")) controlFlow.Add("lambda");

            if (gameManager.allowedSyntaxNodes.Contains("Try")) { controlFlow.Add("try"); controlFlow.Add("except"); controlFlow.Add("finally"); }
            if (gameManager.allowedSyntaxNodes.Contains("Raise")) controlFlow.Add("raise");
            if (gameManager.allowedSyntaxNodes.Contains("Assert")) controlFlow.Add("assert");

            if (gameManager.allowedSyntaxNodes.Contains("Import") || gameManager.allowedSyntaxNodes.Contains("ImportFrom")) { controlFlow.Add("import"); controlFlow.Add("from"); }
            if (gameManager.allowedSyntaxNodes.Contains("With")) { controlFlow.Add("with"); controlFlow.Add("as"); }

            if (gameManager.allowedSyntaxNodes.Contains("Global")) controlFlow.Add("global");
            if (gameManager.allowedSyntaxNodes.Contains("Nonlocal")) controlFlow.Add("nonlocal");
            if (gameManager.allowedSyntaxNodes.Contains("Delete")) controlFlow.Add("del");

            if (gameManager.allowedSyntaxNodes.Contains("BoolOp")) { logicAndTypes.Add("and"); logicAndTypes.Add("or"); }
            if (gameManager.allowedSyntaxNodes.Contains("UnaryOp")) logicAndTypes.Add("not");
            if (gameManager.allowedSyntaxNodes.Contains("Compare")) logicAndTypes.Add("is");

            builtInFunctions = new List<string>(gameManager.allowedFunctions);
            gameCommands = new List<string>();

            if (PythonExecutor.instance != null && PythonExecutor.instance.registeredFunctionNames != null)
            {
                gameCommands.AddRange(PythonExecutor.instance.registeredFunctionNames);
            }

            if (gameManager.allowedSyntaxNodes.Contains("List"))
            {
                listMethods.AddRange(new string[] { "append", "extend", "insert", "remove", "pop", "clear", "index", "count", "sort", "reverse", "copy" });
            }
        }

        syntaxGroups = new SyntaxGroup[]
        {
            new SyntaxGroup { groupName = "Game Commands", color = new Color(0.2f, 0.8f, 0.4f), keywords = gameCommands.ToArray() },
            new SyntaxGroup { groupName = "Control Flow", color = new Color(0.85f, 0.43f, 0.83f), keywords = controlFlow.ToArray() },
            new SyntaxGroup { groupName = "Logic & Types", color = new Color(0.33f, 0.66f, 1f), keywords = logicAndTypes.ToArray() },
            new SyntaxGroup { groupName = "Built-in Functions", color = new Color(0.96f, 0.96f, 0.67f), keywords = builtInFunctions.ToArray() },
            
            new SyntaxGroup { groupName = "List Methods", color = new Color(0.86f, 0.86f, 0.67f), keywords = listMethods.ToArray() }
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
        //float bottomY = textInfo.lineInfo[visualEndLine].descender;
        float bottomY = textInfo.lineInfo[visualStartLine].descender;

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

    public void InsertTextAtCaret(string textToInsert)
    {
        if (inputField == null) return;

        int pos = inputField.caretPosition;
        inputField.text = inputField.text.Insert(pos, textToInsert);
        inputField.caretPosition = pos + textToInsert.Length;
        inputField.ForceLabelUpdate();
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

        // Auto-Indent logic
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Canvas.ForceUpdateCanvases();

            int caretPos = inputField.caretPosition;
            string text = inputField.text;

            int lineStart = text.LastIndexOf('\n', Mathf.Max(0, caretPos - 2)) + 1;
            string prevLine = text.Substring(lineStart, caretPos - lineStart);

            if (prevLine.TrimEnd().EndsWith(":"))
            {
                string indent = "";
                foreach (char c in prevLine)
                {
                    if (c == ' ') indent += " ";
                    else break;
                }
                // Just add the extra indentation
                InsertTextAtCaret(indent + new string(' ', tabSize));
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
            Vector2 inputScrollPos = inputField.textComponent.rectTransform.anchoredPosition;
            float scrollY = inputScrollPos.y;

            if (syntaxOverlayText != null)
            {
                syntaxOverlayText.rectTransform.anchoredPosition = inputScrollPos;
            }

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

        string extractionPattern = "(\"\"\"[\\s\\S]*?(\"\"\"|$)|'''[\\s\\S]*?('''|$)|#.*|\"[^\\n]*?\"|'[^\\n]*?'|\"[^\\n]*|'[^\\n]*)";

        List<string> extractedTexts = new List<string>();
        List<string> extractedColors = new List<string>();

        string stringColorHex = "CE9178";
        string commentColorHex = "6A9955";

        string processedText = Regex.Replace(rawText, extractionPattern, match =>
        {
            extractedTexts.Add(match.Value);

            if (match.Value.StartsWith("#"))
                extractedColors.Add(commentColorHex);
            else
                extractedColors.Add(stringColorHex);

            return $"___EXT{extractedTexts.Count - 1}___";
        });

        string SafeReplace(string input, string pattern, string replacement)
        {
            string safePattern = @"(?:<color=#[A-Fa-f0-9]{6}>|</color>)|" + pattern;

            return Regex.Replace(input, safePattern, m =>
            {
                if (m.Value.StartsWith("<color=") || m.Value == "</color>")
                    return m.Value;

                return m.Result(replacement);
            });
        }

        processedText = SafeReplace(processedText, @"\b\d+(\.\d+)?\b", "<color=#B5CEA8>$0</color>");

        processedText = SafeReplace(processedText, @"([+\-*/%&|^<>!={}-])", "<color=#D4D4D4>$1</color>");

        foreach (SyntaxGroup group in syntaxGroups)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(group.color);

            foreach (string word in group.keywords)
            {
                if (group.groupName == "List Methods")
                {
                    string pattern = @"(\.\s*)\b(" + word + @")\b";
                    processedText = Regex.Replace(processedText, pattern, $"$1<color=#{hexColor}>$2</color>");
                }
                else
                {
                    string pattern = @"\b" + word + @"\b";
                    processedText = Regex.Replace(processedText, pattern, $"<color=#{hexColor}>{word}</color>");
                }
            }
        }

        for (int i = 0; i < extractedTexts.Count; i++)
        {
            string coloredElement = $"<color=#{extractedColors[i]}>{extractedTexts[i]}</color>";
            processedText = processedText.Replace($"___EXT{i}___", coloredElement);
        }

        syntaxOverlayText.text = processedText;
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