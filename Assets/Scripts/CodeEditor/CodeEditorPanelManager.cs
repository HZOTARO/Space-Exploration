using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CodeEditorPanelManager : MonoBehaviour
{
    [Header("Terminal Tab")]
    public Button terminalButton;
    public Image terminalButtonImage;
    public GameObject terminalPanel;

    [Header("Variables Tab")]
    public Button variablesButton;
    public Image variablesButtonImage;
    public GameObject variablesPanel;

    [Header ("Button Sprites")]
    public Sprite buttonActive;
    public Sprite buttonInactive;

    private CodeEditor codeEditor;

    private void Start()
    {
        codeEditor = GetComponent<CodeEditor>();

        terminalButton.onClick.AddListener(ToggleTerminal);
        variablesButton.onClick.AddListener(ToggleVariables);

        SetActive(true, terminalPanel, terminalButtonImage);
        SetActive(true, variablesPanel, variablesButtonImage);
    }

    public void ToggleTerminal()
    {
        bool active = terminalPanel.activeSelf;
        SetActive(!active, terminalPanel, terminalButtonImage);
    }

    public void ToggleVariables()
    {
        bool active = variablesPanel.activeSelf;
        SetActive(!active, variablesPanel, variablesButtonImage);
    }

    public void SetActive(bool active, GameObject panel, Image buttonImage)
    {
        buttonImage.sprite = active ? buttonActive : buttonInactive;
        panel.SetActive(active);
        if (codeEditor != null)
        {
            StartCoroutine(RefreshHighlightNextFrame());
        }
    }

    private IEnumerator RefreshHighlightNextFrame()
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        if (codeEditor.currentStartLine >= 0 && codeEditor.currentEndLine >= 0)
        {
            codeEditor.TriggerHighlight(codeEditor.currentStartLine, codeEditor.currentEndLine);
        }
    }
}
