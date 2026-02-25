using Python.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PythonExecutor : MonoBehaviour
{
    GameManager gameManager;

    PyModule pyScope;
    dynamic pyPrepareFunc;
    dynamic pyStepFunc;
    private Dictionary<string, Delegate> pythonFunctions;
    string currentCode;

    public bool continuous = false;
    bool lockDelay = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPython()
    {
        Runtime.PythonDLL = System.IO.Path.Combine(Application.streamingAssetsPath, "python-3.13.11-embed-amd64", "python313.dll"); ;

        PythonEngine.Initialize();
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        pythonFunctions = new Dictionary<string, Delegate>()
        {
            { "move_up", new Action(() => Move("N")) },
            { "move_down", new Action(() => Move("S")) },
            { "move_left", new Action(() => Move("W")) },
            { "move_right", new Action(() => Move("E")) }
        };

        using (Py.GIL())
        {
            pyScope = Py.CreateScope();
        }

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
            pyScope.Set("unity_log", new Action<string>(LogFromPython));
            pyScope.Exec(@"
import sys

class UnityLogger:
    def __init__(self):
        self.buffer = ''

    def write(self, message):
        self.buffer += message
        if '\n' in self.buffer:
            line, self.buffer = self.buffer.split('\n', 1)
            if line.strip():
                unity_log(line)

    def flush(self):
        if self.buffer.strip():
            unity_log(self.buffer)
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
            foreach (var function in pythonFunctions)
            {
                pyScope.Set(function.Key, function.Value);
            }

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

        collector = GlobalCollector()
        collector.visit(tree)

        transformer = YieldInserter()
        tree = transformer.visit(tree)

        body = tree.body

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

        env = globals().copy() 
        env['__wrap_call__'] = __wrap_call__
        exec(compiled, env)

        __gen__ = env[""__runner__""]()
        
    except Exception as e:
        print(f""{type(e).__name__}: {e}"")
        __gen__ = None

def step():
    global __gen__
    
    if __gen__ is None:
        return ""ERROR""

    try:
        next(__gen__)
        return ""STEP""
    except StopIteration:
        return ""DONE""
    except Exception as e:
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
        if (gameManager.InAction())
            return;

        using (Py.GIL())
        {
            var result = pyStepFunc().ToString();

            if (result == "DONE")
            {
                continuous = false;
                currentCode = null;
            }
        }

        lockDelay = true;
        Invoke("UnlockDelay", 0.1f);
    }

    void UnlockDelay()
    {
        lockDelay = false;
    }

    private void Update()
    {
        if (continuous && !lockDelay && !gameManager.InAction())
        {
            Step();
        }
    }
    void LogFromPython(string message)
    {
        Debug.Log(message);

        if (gameManager != null)
        {
            gameManager.PrintToDisplay(message);
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
                foreach (var function in pythonFunctions)
                    pyScope.Set(function.Key, null);
                pyScope.Set("unity_log", null);

                pyScope.Dispose();
                pyScope = null;
            }
            PythonEngine.Exec("import gc; gc.collect()");
        }
        PythonEngine.Shutdown();
    }

    void Move(string dir) { gameManager.Move(dir); }
}
