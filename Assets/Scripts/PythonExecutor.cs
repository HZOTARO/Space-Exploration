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

class GlobalCollector(ast.NodeVisitor):
    def __init__(self):
        self.global_names = set()
        
    def visit_Name(self, node):
        if isinstance(node.ctx, ast.Store):
            self.global_names.add(node.id)
            
    def visit_FunctionDef(self, node):
        self.global_names.add(node.name)
        
    def visit_ClassDef(self, node):
        self.global_names.add(node.name)
        
    def visit_Import(self, node):
        for alias in node.names:
            name = alias.asname or alias.name
            self.global_names.add(name.split('.')[0])
            
    def visit_ImportFrom(self, node):
        for alias in node.names:
            self.global_names.add(alias.asname or alias.name)

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

__gen__ = None

def prepare(code):
    global __gen__
    
    try:
        tree = ast.parse(code)

        # 1. Find all top-level variables before making changes
        collector = GlobalCollector()
        collector.visit(tree)

        # 2. Insert the yields
        transformer = YieldInserter()
        tree = transformer.visit(tree)

        body = tree.body
        # 3. Inject the 'global' declarations at the start of __runner__
        if collector.global_names:
            body.insert(0, ast.Global(names=list(collector.global_names)))

        func_def = ast.FunctionDef(
            name=""__runner__"",
            args=ast.arguments(
                posonlyargs=[], args=[], kwonlyargs=[],
                kw_defaults=[], defaults=[]
            ),
            body=body,
            decorator_list=[]
        )

        module = ast.Module(body=[func_def], type_ignores=[])
        ast.fix_missing_locations(module)

        compiled = compile(module, ""<player_code>"", ""exec"")

        env = {'__wrap_call__': __wrap_call__}
        exec(compiled, env)

        __gen__ = env[""__runner__""]()
        
    except Exception as e:
        # Catches SyntaxError, IndentationError, etc. during parsing
        print(f""{type(e).__name__}: {e}"")
        __gen__ = None

def step():
    global __gen__
    
    # If prepare() failed, __gen__ will be None. Prevent crashing.
    if __gen__ is None:
        return ""ERROR""

    try:
        next(__gen__)
        return ""STEP""
    except StopIteration:
        print(""Program complete."")
        return ""DONE""
    except Exception as e:
        # Catches runtime errors like TypeError, NameError, etc.
        print(f""{type(e).__name__}: {e}"")
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
