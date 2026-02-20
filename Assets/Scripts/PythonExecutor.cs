using Python.Runtime;
using System;
using UnityEngine;

public class PythonExecutor : MonoBehaviour
{
    public GameObject cube;

    PyModule pyScope;
    dynamic pyPrepareFunc;
    dynamic pyStepFunc;
    dynamic pyUpdateFunc;
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

    private void Update()
    {
        if (pyUpdateFunc != null)
        {
            using (Py.GIL())
            {
                pyUpdateFunc(Time.deltaTime);
            }
        }
    }

    void SetupStep()
    {
        using (Py.GIL())
        {
            pyScope = Py.CreateScope();
            pyScope.Set("gameObject", cube.ToPython());
            pyScope.Exec(@"
import math
from UnityEngine import Vector3

t = 0
def update(dt):
    global t
    t += dt
    # Move object in a circle
    x = math.cos(t) * 5
    z = math.sin(t) * 5
    gameObject.transform.position = Vector3(x, 0, z)
"
            );
            pyUpdateFunc = pyScope.Get("update");

            pyScope.Exec(@"
import ast
import types

class YieldInserter(ast.NodeTransformer):
    def insert_yield(self, node):
        return [
            node,
            ast.Expr(value=ast.Yield(value=ast.Constant(value=None)))
        ]

    def visit_Expr(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_Assign(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_AugAssign(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_Return(self, node):
        self.generic_visit(node)
        return [
            ast.Expr(value=ast.Yield(value=ast.Constant(value=None))),
            node
        ]

    def visit_If(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_For(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_While(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_Call(self, node):
        self.generic_visit(node)
        
        wrapper = ast.Name(id='__wrap_call__', ctx=ast.Load())
        new_args = [node.func] + node.args
        new_call = ast.Call(func=wrapper, args=new_args, keywords=node.keywords)
        
        return ast.YieldFrom(value=new_call)

def __wrap_call__(func, *args, **kwargs):
    res = func(*args, **kwargs)
    
    if isinstance(res, types.GeneratorType):
        return (yield from res)
        
    return res

def prepare(code):
    global __gen__

    tree = ast.parse(code)

    transformer = YieldInserter()
    tree = transformer.visit(tree)

    func_def = ast.FunctionDef(
        name=""__runner__"",
        args=ast.arguments(
            posonlyargs=[], args=[], kwonlyargs=[],
            kw_defaults=[], defaults=[]
        ),
        body=tree.body,
        decorator_list=[]
    )

    module = ast.Module(body=[func_def], type_ignores=[])
    ast.fix_missing_locations(module)

    compiled = compile(module, ""<player_code>"", ""exec"")

    env = {'__wrap_call__': __wrap_call__}
    exec(compiled, env)

    __gen__ = env[""__runner__""]()

def step():
    global __gen__

    try:
        next(__gen__)
        return ""STEP""
    except StopIteration:
        print(""Program complete."")
        return ""DONE""
    except Exception as e:
        print(f""Error: {e}"")
        return ""ERROR""
"
            );
            pyPrepareFunc = pyScope.Get("prepare");
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
                pyPrepareFunc(currentCode);
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
                currentCode = null;
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
