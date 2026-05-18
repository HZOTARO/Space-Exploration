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

    private void Start()
    {
        terminalButton.onClick.AddListener(ToggleTerminal);
        variablesButton.onClick.AddListener(ToggleVariables);

        SetActive(false, terminalPanel, terminalButtonImage);
        SetActive(false, variablesPanel, variablesButtonImage);
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
    }
}
