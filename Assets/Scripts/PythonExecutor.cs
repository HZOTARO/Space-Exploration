using Python.Runtime;
using System;
using UnityEngine;

public class PythonExecutor : MonoBehaviour
{
    PyModule pyScope;
    dynamic pyStepFunc;
    string currentCode;
    public bool continuous = false;

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
        SetupStep();
    }

    /// <summary>
    /// Setup for converting print() to Debug.Log()
    /// </summary>
    void SetupLogger()
    {
        using (Py.GIL())
        {
            PythonEngine.Exec(@"
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
"
            );
        }
    }

    void SetupStep()
    {
        using (Py.GIL())
        {
            // keep the scope alive for the lifetime of this component so
            // the step() function and its globals (nodes, env, current) live together
            pyScope = Py.CreateScope();
            pyScope.Exec(@"
import ast

def step():
    global current

    if current >= len(nodes):
        print('Program complete.')
        current = 0
        return ""DONE""

    node = nodes[current]

    single = ast.Module([node], type_ignores=[])
    compiled = compile(single, ""<player_code>"", ""exec"")
    exec(compiled, env)

    current += 1
    return env
"
            );
            pyStepFunc = pyScope.Get("step");
        }
    }

    public void Exec(string code)
    {
        // only rebuild parse state when the code changed
        if (currentCode == null || !String.Equals(currentCode, code))
        {
            currentCode = code;
            using (Py.GIL())
            {
                // reuse the persistent scope so step() can access nodes, env, current
                pyScope.Set("code", currentCode);
                pyScope.Exec(@"
tree = ast.parse(code)
nodes = tree.body        # list of statements
env = { }                # variables live here
current = 0              # which line we are on
"
                );
            }
        }
        Step();
    }

    void Step()
    {
        using (Py.GIL())
        {
            var result = pyStepFunc().ToString();

            if (result == "DONE")
            {
                continuous = false;
            }
        }

        if (continuous)
        {
            Invoke("Step", 0.1f);
        }
    }

    void OnDestroy()
    {
        if (!PythonEngine.IsInitialized)
            return;

        using (Py.GIL())
        {
            if (pyScope != null)
            {
                pyScope.Dispose();
                pyScope = null;
            }
        }

        PythonEngine.Shutdown();
    }
}
