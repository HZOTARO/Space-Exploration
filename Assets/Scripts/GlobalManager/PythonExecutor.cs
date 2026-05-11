using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct PythonValidationRequest
{
    public string code;
    public string[] allowed_nodes;
    public string[] allowed_functions;
}

[System.Serializable]
public struct PythonValidationResult
{
    public bool is_valid;
    public string error_msg;
    public int line;
}

public class PythonExecutor : MonoBehaviour
{
    public static PythonExecutor instance;

    public float stepDelay = 0.1f;

    [Header("Python Settings")]
    PyModule pyScope;
    dynamic pyPrepareFunc;
    dynamic pyStepFunc;
    dynamic pyGetVarFunc;
    dynamic pyGetLineSyntaxMap;
    public Dictionary<int, HashSet<string>> currentSyntaxMap = new Dictionary<int, HashSet<string>>();
    public List<string> registeredFunctionNames = new List<string>();

    [HideInInspector] public string currentCode;
    public bool continuous = false;
    bool lockDelay = false;

    public event Action OnExecutionFinished;
    public event Action<string> OnPythonPrint;
    public event Action<int, int> OnLineExecuted;
    public event Action<int, string> OnRuntimeError;

    public Func<bool> CanStepCode;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            using (Py.GIL())
            {
                pyScope = Py.CreateScope();
            }

            SetupLogger();
            SetupStep();

            SceneManager.sceneLoaded += OnLevelLoaded;
            SceneManager.sceneUnloaded += OnLevelExited;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializePythonAllowed(string[] nodes, string[] functions)
    {
        if (!PythonEngine.IsInitialized || pyScope == null) return;

        PythonValidationRequest request = new PythonValidationRequest
        {
            allowed_nodes = nodes,
            allowed_functions = functions
        };
        string jsonRequest = JsonUtility.ToJson(request);

        using (Py.GIL())
        {
            dynamic pyInitFunc = pyScope.Get("initialize_allowed");
            pyInitFunc(jsonRequest);
        }
    }

    public void ClearPythonBan(string unlockName)
    {
        if (!PythonEngine.IsInitialized || pyScope == null) return;

        using (Py.GIL())
        {
            dynamic pyClearFunc = pyScope.Get("clear_ban");
            pyClearFunc(unlockName);
        }
    }

    public PythonValidationResult ValidateCode(string playerCode)
    {
        if (!PythonEngine.IsInitialized || pyScope == null)
        {
            return new PythonValidationResult { is_valid = false, error_msg = "Python Engine Error", line = 0 };
        }

        using (Py.GIL())
        {
            dynamic pyValidateFunc = pyScope.Get("validate_code");
            string jsonResponse = pyValidateFunc(playerCode).ToString();
            return JsonUtility.FromJson<PythonValidationResult>(jsonResponse);
        }
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance == this)
        {
            StopRunningCode();

            OnExecutionFinished = null;
            OnPythonPrint = null;
            CanStepCode = null;
            OnLineExecuted = null;
        }
    }

    void OnLevelExited(Scene unloadedScene)
    {
        if (!PythonEngine.IsInitialized || pyScope == null) return;

        using (Py.GIL())
        {
            if (registeredFunctionNames != null)
            {
                foreach (string funcName in registeredFunctionNames)
                {
                    pyScope.Set(funcName, null);
                }
                registeredFunctionNames.Clear();
            }
        }
        Debug.Log($"Cleaned up Python functions from {unloadedScene.name}");
    }

    public void RegisterPythonFunction(string pythonName, Delegate csharpFunction)
    {
        if (!PythonEngine.IsInitialized || pyScope == null) return;

        using (Py.GIL())
        {
            pyScope.Set(pythonName, csharpFunction);

            if (!registeredFunctionNames.Contains(pythonName))
            {
                registeredFunctionNames.Add(pythonName);
            }

            dynamic pyUnlockFunc = pyScope.Get("unlock_syntax");
            pyUnlockFunc(pythonName);
        }
    }   

    /// <summary>
    /// Stop python running, and clear for next scene
    /// </summary>
    public void StopRunningCode()
    {
        continuous = false;
        currentCode = null;

        using (Py.GIL())
        {
            if (pyScope != null)
            {
                pyScope.Exec("global __gen__\n__gen__ = None");
            }
        }
    }

    /// <summary>
    /// Setup for converting print() to Debug.Log()
    /// </summary>
    void SetupLogger()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "PythonSystem", "logger_setup.py");
        if (File.Exists(filePath))
        {
            using (Py.GIL())
            {
                pyScope.Set("unity_log", new Action<string>(LogFromPython));

                pyScope.Exec(File.ReadAllText(filePath));
            }
        }
    }

    void SetupStep()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "PythonSystem", "ast_setup.py");
        if (File.Exists(filePath))
        {
            using (Py.GIL())
            {
                pyScope.Exec(File.ReadAllText(filePath));
                pyPrepareFunc = pyScope.Get("prepare");
                pyStepFunc = pyScope.Get("step");
                pyGetVarFunc = pyScope.Get("get_variable_value");
                pyGetLineSyntaxMap = pyScope.Get("get_line_syntax_map");
            }
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
            BuildSyntaxMap(code);
        }

        Step();
    }

    void Step()
    {
        if (CanStepCode != null && !CanStepCode.Invoke())
            return;

        using (Py.GIL())
        {
            var result = pyStepFunc().ToString();

            if (result.StartsWith("RUNTIME_ERROR|"))
            {
                continuous = false;
                currentCode = null;

                string[] parts = result.Split('|', 3);

                int.TryParse(parts[1], out int line);
                string msg = parts.Length > 2 ? parts[2] : "Unknown Error";

                OnRuntimeError?.Invoke(line, msg);
                return;
            }

            if (result == "DONE")
            {
                continuous = false;
                currentCode = null;
                OnExecutionFinished?.Invoke();
            }

            else
            {
                if (result.Contains(","))
                {
                    string[] parts = result.Split(',');
                    if (int.TryParse(parts[0], out int startLine) && int.TryParse(parts[1], out int endLine))
                    {
                        OnLineExecuted?.Invoke(startLine, endLine);
                    }
                }
                else if (int.TryParse(result, out int singleLine))
                {
                    OnLineExecuted?.Invoke(singleLine, singleLine);
                }
            }
        }

        if (stepDelay > 0 && continuous)
        {
            lockDelay = true;
            Invoke("UnlockDelay", stepDelay * Time.timeScale);
        }
    }

    void UnlockDelay()
    {
        lockDelay = false;
    }

    private void Update()
    {
        bool canStep = CanStepCode == null || CanStepCode.Invoke();
        if (continuous && !lockDelay && canStep)
        {
            Step();
        }
    }

    void LogFromPython(string message)
    {
        OnPythonPrint?.Invoke(message);
    }

    void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= OnLevelLoaded;
        CleanupPythonEnvironment();
    }

    public void CleanupPythonEnvironment()
    {
        if (!PythonEngine.IsInitialized) return;

        using (Py.GIL())
        {
            if (pyScope != null)
            {
                pyScope.Exec(@"
import sys
if hasattr(sys, '__stdout__'):
    sys.stdout = sys.__stdout__
    sys.stderr = sys.__stderr__

global __gen__
__gen__ = None
");
                pyScope.Set("unity_log", null);

                if (pyPrepareFunc is IDisposable p) p.Dispose();
                if (pyStepFunc is IDisposable s) s.Dispose();
                if (pyGetVarFunc is IDisposable g) g.Dispose();

                pyPrepareFunc = null;
                pyStepFunc = null;

                pyScope.Dispose();
                pyScope = null;
            }

            PythonEngine.Exec("import gc; gc.collect()");
        }

        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    public string GetVariableValue(string varName)
    {
        if (pyGetVarFunc == null) return "Undefined";

        using (Py.GIL())
        {
            try
            {
                return pyGetVarFunc(varName).ToString();
            }
            catch
            {
                return "Undefined";
            }
        }
    }
    private void BuildSyntaxMap(string code)
    {
        currentSyntaxMap.Clear();
        if (pyGetLineSyntaxMap == null) return;

        using (Py.GIL())
        {
            string mapString = pyGetLineSyntaxMap(code).ToString();
            if (string.IsNullOrEmpty(mapString)) return;

            string[] lines = mapString.Split('|');
            foreach (string lineData in lines)
            {
                string[] parts = lineData.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int lineNum))
                {
                    HashSet<string> nodes = new HashSet<string>(parts[1].Split(','));
                    currentSyntaxMap[lineNum] = nodes;
                }
            }
        }
    }

    public bool CurrentLineContainsSyntax(int line, string requiredSyntax)
    {
        if (currentSyntaxMap.ContainsKey(line))
        {
            return currentSyntaxMap[line].Contains(requiredSyntax);
        }
        return false;
    }
}