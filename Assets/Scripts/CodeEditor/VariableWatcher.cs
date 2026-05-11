using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class VariableWatcher : MonoBehaviour
{
    [Header("Menu UI (The Editor)")]
    public GameObject menuPanel;
    public Button addVariableButton;
    public Transform menuRowContainer;
    public GameObject menuRowPrefab;

    [Header("Gameplay Display UI (The HUD)")]
    public GameObject displayPanel;
    public TextMeshProUGUI singleDisplayText;

    public LayoutElement displayPanelLayoutElement;
    public float heightPadding = 25f;

    [Header("Settings")]
    public int maxVariables = 5;

    private List<VariableSetupRow> activeRows = new List<VariableSetupRow>();
    private List<string> cachedVariables = new List<string>();

    void Start()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted += HandleLineExecuted;
            PythonExecutor.instance.OnExecutionFinished += UpdateHUD;
        }

        if (addVariableButton != null)
        {
            addVariableButton.onClick.AddListener(AddEmptyRow);
        }

        for (int i = 0; i < maxVariables; i++)
        {
            AddEmptyRow();
        }

        SaveAndApply();
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted -= HandleLineExecuted;
            PythonExecutor.instance.OnExecutionFinished -= UpdateHUD;
        }
    }

    private void AddEmptyRow()
    {
        if (activeRows.Count >= maxVariables) return;

        GameObject rowObj = Instantiate(menuRowPrefab, menuRowContainer);
        VariableSetupRow rowScript = rowObj.GetComponent<VariableSetupRow>();

        string name = "Variable " + (activeRows.Count + 1) + ":";
        rowScript.Setup(name, RemoveRow);
        activeRows.Add(rowScript);

        UpdateAddButtonState();
    }

    private void RemoveRow(VariableSetupRow rowScript)
    {
        activeRows.Remove(rowScript);
        Destroy(rowScript.gameObject);
        UpdateAddButtonState();
    }

    private void UpdateAddButtonState()
    {
        if (addVariableButton != null)
        {
            addVariableButton.interactable = (activeRows.Count < maxVariables);
        }
    }

    public void OpenMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void SaveAndApply()
    {
        cachedVariables.Clear();

        foreach (VariableSetupRow row in activeRows)
        {
            string vName = row.VariableName;

            if (!string.IsNullOrWhiteSpace(vName) && !cachedVariables.Contains(vName))
            {
                cachedVariables.Add(vName);
            }
        }

        if (menuPanel != null) menuPanel.SetActive(false);

        UpdateHUD();
    }

    private void HandleLineExecuted(int startLine, int endLine)
    {
        UpdateHUD();
    }
    private void UpdateHUD()
    {
        if (singleDisplayText == null || PythonExecutor.instance == null) return;

        if (cachedVariables.Count == 0)
        {
            if (displayPanel != null) displayPanel.SetActive(false);
            singleDisplayText.text = "";
            return;
        }

        if (displayPanel != null) displayPanel.SetActive(true);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < cachedVariables.Count; i++)
        {
            string vName = cachedVariables[i];
            string vValue = PythonExecutor.instance.GetVariableValue(vName);

            if (i > 0) sb.Append("\n");
            sb.Append($"{vName}: <color=yellow>{vValue}</color>");
        }

        singleDisplayText.text = sb.ToString();

        if (displayPanelLayoutElement != null)
        {
            singleDisplayText.ForceMeshUpdate();

            float exactTextHeight = singleDisplayText.preferredHeight;
            displayPanelLayoutElement.preferredHeight = exactTextHeight + heightPadding;
        }
    }
}