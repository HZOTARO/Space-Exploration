using TMPro;
using UnityEngine;

public class CodeEditor : MonoBehaviour
{
    PythonExecutor pythonExecutor;

    [Header("References")]
    public TMP_InputField inputField;

    void Start()
    {
        pythonExecutor = FindAnyObjectByType<PythonExecutor>();
    }

    public void Play()
    {
        pythonExecutor.continuous = true;
        pythonExecutor.Exec(inputField.text);
    }

    public void Step()
    {
        pythonExecutor.continuous = false;
        pythonExecutor.Exec(inputField.text);
    }
}