using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ObjectiveType { Syntax, VariableState, ReachGoal }

[System.Serializable]
public class LevelObjective
{
    public string description; // What the UI displays (e.g., "Write a For Loop")
    public ObjectiveType type;

    [Header("Syntax Settings")]
    public string targetSyntaxNode; // e.g., "For" or "List"

    [Header("Variable Settings")]
    public string targetVariableName; // e.g., "password"
    public string targetVariableValue; // e.g., "1234"

    [HideInInspector] public bool isComplete = false;
    [HideInInspector] public TextMeshProUGUI uiTextRef; // The spawned UI text
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    [Header("Objectives")]
    public List<LevelObjective> objectives = new List<LevelObjective>();

    [Header("UI References")]
    public GameObject objectivePrefab; // A prefab containing a TextMeshPro object
    public Transform objectiveContainer; // The Layout Group that holds the checklist

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 1. Spawn the UI Checklist
        foreach (var obj in objectives)
        {
            GameObject newObjUI = Instantiate(objectivePrefab, objectiveContainer);
            obj.uiTextRef = newObjUI.GetComponentInChildren<TextMeshProUGUI>();
            UpdateUI(obj);
        }

        // 2. Hook into Python execution for Real-Time checking
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

    // =========================================================
    // RUNS EVERY TIME A LINE OF PYTHON EXECUTES
    // =========================================================
    private void EvaluateRuntimeObjectives(int startLine, int endLine)
    {
        if (PythonExecutor.instance == null) return;

        foreach (var obj in objectives)
        {
            // 1. Evaluate Dynamic Syntax (Did the line we just ran contain the required node?)
            if (obj.type == ObjectiveType.Syntax && !obj.isComplete)
            {
                if (PythonExecutor.instance.CurrentLineContainsSyntax(startLine, obj.targetSyntaxNode))
                {
                    obj.isComplete = true;
                    UpdateUI(obj);
                }
            }

            // 2. Evaluate Dynamic Variables (Did the line we just ran set the correct variable?)
            if (obj.type == ObjectiveType.VariableState && !obj.isComplete)
            {
                string currentValue = PythonExecutor.instance.GetVariableValue(obj.targetVariableName);

                if (currentValue == obj.targetVariableValue)
                {
                    obj.isComplete = true;
                    UpdateUI(obj);
                }
            }
        }
    }

    // =========================================================
    // CALLED BY GOAL TILE: When the robot steps on the finish line
    // =========================================================
    public void CompleteGoalObjective()
    {
        foreach (var obj in objectives)
        {
            if (obj.type == ObjectiveType.ReachGoal)
            {
                obj.isComplete = true;
                UpdateUI(obj);
            }
        }
    }

    public bool AreAllObjectivesComplete()
    {
        foreach (var obj in objectives)
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
            obj.uiTextRef.text = $"[<color=green>V</color>] {obj.description}";
            obj.uiTextRef.color = Color.green; // Or grayed out, depending on your style
        }
        else
        {
            obj.uiTextRef.text = $"[<color=red>X</color>] {obj.description}";
            obj.uiTextRef.color = Color.white;
        }
    }
}