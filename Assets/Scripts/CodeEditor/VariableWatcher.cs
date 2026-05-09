using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class WatchedVariable
{
    public string variableName;
    public TextMeshProUGUI uiText;
}

public class VariableWatcher : MonoBehaviour
{
    public List<WatchedVariable> watchedVariables = new List<WatchedVariable>();

    void Start()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted += UpdateWatchedVariables;
            PythonExecutor.instance.OnExecutionFinished += UpdateWatchedVariables;
        }

        ResetUI();
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted -= UpdateWatchedVariables;
            PythonExecutor.instance.OnExecutionFinished -= UpdateWatchedVariables;
        }
    }

    private void UpdateWatchedVariables(int startLine, int endLine)
    {
        UpdateWatchedVariables();
    }

    public void UpdateWatchedVariables()
    {
        if (PythonExecutor.instance == null) return;

        foreach (var watch in watchedVariables)
        {
            if (watch.uiText != null && !string.IsNullOrEmpty(watch.variableName))
            {
                string liveValue = PythonExecutor.instance.GetVariableValue(watch.variableName);

                watch.uiText.text = $"{watch.variableName}: <color=yellow>{liveValue}</color>";
            }
        }
    }

    public void ResetUI()
    {
        foreach (var watch in watchedVariables)
        {
            if (watch.uiText != null)
            {
                watch.uiText.text = $"{watch.variableName}: <color=grey>Undefined</color>";
            }
        }
    }
}