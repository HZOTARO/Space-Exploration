using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ObjectiveType { Syntax, FunctionCall, VariableState, CustomEvent }

[System.Serializable]
public class LevelObjective
{
    public string description;
    public ObjectiveType type;

    [Header("Syntax Settings")]
    public string targetSyntaxNode;

    [Header("Function Settings")]
    public string targetFunctionName;

    [Header("Variable Settings")]
    public string targetVariableName;
    public string targetVariableValue;

    [Header("Custom Event Settings")]
    public string customEventId;

    [HideInInspector] public bool isComplete = false;
    [HideInInspector] public TextMeshProUGUI uiTextRef;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    [Header("Objectives")]
    public List<LevelObjective> objectives = new List<LevelObjective>();

    [Header("UI References")]
    public GameObject objectivePrefab;
    public Transform objectiveContainer;

    public event Action OnAllObjectiveComplete;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted += EvaluateRuntimeObjectives;
        }
    }

    void OnDestroy()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.OnLineExecuted -= EvaluateRuntimeObjectives;
        }
    }

    public void InitiateAllTask()
    {
        foreach (LevelObjective obj in objectives)
        {
            GameObject newObjUI = Instantiate(objectivePrefab, objectiveContainer);
            obj.uiTextRef = newObjUI.GetComponentInChildren<TextMeshProUGUI>();
            UpdateUI(obj);
        }
    }

    private void EvaluateRuntimeObjectives(int startLine, int endLine)
    {
        if (PythonExecutor.instance == null) return;

        foreach (LevelObjective obj in objectives)
        {
            if (obj.isComplete) continue;

            if (obj.type == ObjectiveType.Syntax)
            {
                if (PythonExecutor.instance.CurrentLineContainsSyntax(startLine, obj.targetSyntaxNode))
                {
                    CompleteObjective(obj);
                }
            }
            else if (obj.type == ObjectiveType.FunctionCall)
            {
                if (PythonExecutor.instance.CheckASTPattern(startLine, endLine, "FunctionCall", obj.targetFunctionName))
                {
                    CompleteObjective(obj);
                }
            }
            else if (obj.type == ObjectiveType.VariableState)
            {
                string currentValue = PythonExecutor.instance.GetVariableValue(obj.targetVariableName);

                if (currentValue == obj.targetVariableValue)
                {
                    CompleteObjective(obj);
                }
            }
        }
    }

    public void TriggerCustomEvent(string eventId)
    {
        foreach (LevelObjective obj in objectives)
        {
            if (obj.type == ObjectiveType.CustomEvent && !obj.isComplete)
            {
                if (obj.customEventId == eventId)
                {
                    CompleteObjective(obj);
                }
            }
        }
    }

    private void CompleteObjective(LevelObjective obj)
    {
        obj.isComplete = true;
        UpdateUI(obj);

        if (AreAllObjectivesComplete())
        {
            OnAllObjectiveComplete?.Invoke();
        }
    }

    public bool AreAllObjectivesComplete()
    {
        foreach (LevelObjective obj in objectives)
        {
            if (!obj.isComplete) return false;
        }
        return true;
    }

    private void UpdateUI(LevelObjective obj)
    {
        if (obj.uiTextRef == null) return;

        if (obj.isComplete)
        {
            obj.uiTextRef.text = $"[<color=green>V</color>] <s>{obj.description}</s>";
            obj.uiTextRef.color = Color.green;
        }
        else
        {
            obj.uiTextRef.text = $"[<color=red>X</color>] {obj.description}";
            obj.uiTextRef.color = Color.white;
        }
    }
}