using Python.Runtime;
using System;
using UnityEngine;

public class PythonExecutor : MonoBehaviour
{
    PyModule pyScope;
    dynamic pyPrepareFunc;
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
            pyScope = Py.CreateScope();

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
        # Yield BEFORE returning so we can pause and see the final state
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
        # 1. Visit children first (arguments might be nested calls!)
        self.generic_visit(node)
        
        # 2. Wrap all function calls in: __wrap_call__(func, *args, **kwargs)
        wrapper = ast.Name(id='__wrap_call__', ctx=ast.Load())
        new_args = [node.func] + node.args
        new_call = ast.Call(func=wrapper, args=new_args, keywords=node.keywords)
        
        # 3. Return as a yield from expression to bubble the steps up
        return ast.YieldFrom(value=new_call)

def __wrap_call__(func, *args, **kwargs):
    # Execute the function
    res = func(*args, **kwargs)
    
    # If the function was modified by us, it will be a generator.
    # We 'yield from' it so the main runner can pull its internal steps.
    if isinstance(res, types.GeneratorType):
        return (yield from res)
        
    # If it's a native/built-in function (like print), just return the result immediately.
    return res

def prepare(code):
    global __gen__

    tree = ast.parse(code)

    transformer = YieldInserter()
    tree = transformer.visit(tree) # generic_visit handles body list processing dynamically

    # Wrap inside generator function
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

    # Pass our wrapper into the execution environment so wrapped calls can find it
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
