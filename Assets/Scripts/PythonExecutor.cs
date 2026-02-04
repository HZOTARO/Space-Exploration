using UnityEngine;
using Python.Runtime;

public class PythonExecutor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Runtime.PythonDLL = Application.dataPath + "/Streaming Assets/python-3.13.11-embed-amd64/python313.dll";
        PythonEngine.Initialize();

        // Create a Python class that redirects writes to Unity Debug.Log
        string logger = @"
import sys
import clr
from UnityEngine import Debug

class UnityLogger(object):
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

logger = UnityLogger()
sys.stdout = logger
sys.stderr = logger
";

        PythonEngine.Exec(logger);
        PythonEngine.Exec("print('Hello from Python!')");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
