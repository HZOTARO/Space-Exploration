using UnityEngine;
using Python.Runtime;

public class Bootstrap : MonoBehaviour
{
    dynamic pyUpdateFunc;

    void Start()
    {
        Runtime.PythonDLL = Application.dataPath + "/StreamingAssets/python-3.13.7-embed-amd64/python313.dll";
        PythonEngine.Initialize();

        using (Py.GIL())
        {
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
            PythonEngine.Exec("print('2 + 2 = ', 2 + 2)");

            // Provide Unity objects to Python
            using (PyModule scope = Py.CreateScope())
            {
                scope.Set("gameObject", gameObject.ToPython());
                //                scope.Exec(@"
                //import math
                //from UnityEngine import Vector3

                //t = 0
                //def update(dt):
                //    global t
                //    t += dt
                //    # Move object in a circle
                //    x = math.cos(t) * 5
                //    z = math.sin(t) * 5
                //    gameObject.transform.position = Vector3(x, 0, z)
                //");
                scope.Exec(@"
import ast

code = """"""
print(""Hello World"")
print(""Hello World"")
x = 10
y = 2
z = x / y
""""""

# 1. Parse code into AST
tree = ast.parse(code)

# 2. Execution state
nodes = tree.body        # list of statements
env = {}                 # variables live here
current = 0              # which line we are on

def step():
    global current

    if current >= len(nodes):
        return ""DONE""

    node = nodes[current]

    # 3. Compile ONE node and execute it
    single = ast.Module([node], type_ignores=[])
    compiled = compile(single, ""<player_code>"", ""exec"")
    exec(compiled, env)

    current += 1
    return env
                ");
                pyUpdateFunc = scope.Get("step");
            }

        }
    }

    void Update()
    {
        using (Py.GIL())
        {
            //pyUpdateFunc(Time.deltaTime);
        }
    }

    public void DoSomething()
    {
        Debug.Log("Do Something");
    }

    public void OnApplicationQuit()
    {
        if (PythonEngine.IsInitialized)
        {
            PythonEngine.Shutdown();
        }
    }

    public void OnButtonClick()
    {
        //pyUpdateFunc();
    }
}
