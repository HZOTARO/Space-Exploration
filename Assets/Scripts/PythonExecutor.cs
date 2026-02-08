using Python.Runtime;
using UnityEngine;

public class PythonExecutor : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPython()
    {
        Runtime.PythonDLL =
            Application.dataPath + "/Streaming Assets/python-3.13.11-embed-amd64/python313.dll";

        PythonEngine.Initialize();
    }

    void Start()
    {
        SetupLogger();
    }

    void SetupLogger()
    {
        string logger = @"
import sys
from UnityEngine import Debug

class UnityLogger:
    def __init__(self):
        self.buffer = ''

    def write(self, message):
        self.buffer += message
        if '\n' in self.buffer:
            line, self.buffer = self.buffer.split('\n', 1)
            if line.strip():
                Debug.Log(line)

    def flush(self):
        if self.buffer.strip():
            Debug.Log(self.buffer)
        self.buffer = ''
        
sys.stdout = UnityLogger()
sys.stderr = sys.stdout
";
        PythonEngine.Exec(logger);
    }

    public void Exec(string code)
    {
        PythonEngine.Exec(code);
    }

    void OnDestroy()
    {
        if (PythonEngine.IsInitialized)
            PythonEngine.Shutdown();
    }
}
