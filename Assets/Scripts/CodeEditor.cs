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
        pythonExecutor.Exec(inputField.text);
    }
}