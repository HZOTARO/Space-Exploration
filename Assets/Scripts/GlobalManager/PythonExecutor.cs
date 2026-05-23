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
    
    private Dictionary<string, dynamic> pyFunctionCache = new Dictionary<string, dynamic>();

    public Dictionary<int, HashSet<string>> currentSyntaxMap = new Dictionary<int, HashSet<string>>();
    public List<string> registeredFunctionNames = new List<string>();

    [HideInInspector] public string currentCode;
    public bool continuous = false;
    bool lockDelay = false;

    public event Action OnExecutionStarted;
    public event Action OnExecutionFinished;
    public event Action OnExecutionFinishedBefore;
    public event Action<string> OnPythonPrint;
    public event Action<int, int> OnLineExecuted;
    public event Action<int, string> OnRuntimeError;
    public event Action OnExecutionAborted;

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

    private dynamic GetCachedPyFunction(string functionName)
    {
        if (pyScope == null) return null;

        if (!pyFunctionCache.ContainsKey(functionName))
        {
            pyFunctionCache[functionName] = pyScope.Get(functionName);
        }

        return pyFunctionCache[functionName];
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
            GetCachedPyFunction("initialize_allowed")(jsonRequest);
        }
    }

    public void ClearPythonBan(string unlockName)
    {
        if (!PythonEngine.IsInitialized || pyScope == null) return;

        using (Py.GIL())
        {
            GetCachedPyFunction("clear_ban")(unlockName);
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
            string jsonResponse = GetCachedPyFunction("validate_code")(playerCode).ToString();
            return JsonUtility.FromJson<PythonValidationResult>(jsonResponse);
        }
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance == this)
        {
            StopRunningCode();

            OnExecutionStarted = null;
            OnExecutionFinishedBefore = null;
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

            GetCachedPyFunction("unlock_syntax")(pythonName);
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

        OnExecutionAborted?.Invoke();
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
                GetCachedPyFunction("prepare")(currentCode);
            }
            BuildSyntaxMap(code);

            OnExecutionStarted?.Invoke();
        }

        Step();
    }

    void Step()
    {
        if (CanStepCode != null && !CanStepCode.Invoke())
            return;

        using (Py.GIL())
        {
            var result = GetCachedPyFunction("step")().ToString();

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
                OnExecutionFinishedBefore?.Invoke();
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

                foreach ((string name, dynamic value) in pyFunctionCache)
                {
                    if (value is IDisposable d) d.Dispose();
                }
                pyFunctionCache.Clear();

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
        if (pyScope == null) return "Undefined";

        using (Py.GIL())
        {
            try
            {
                return GetCachedPyFunction("get_variable_value")(varName).ToString();
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

        using (Py.GIL())
        {
            string mapString = GetCachedPyFunction("get_line_syntax_map")(code).ToString();
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

            //StringBuilder sb = new StringBuilder();
            //for(int i = 0; i <= currentSyntaxMap.Count; i++)
            //{
            //    currentSyntaxMap.TryGetValue(i, out HashSet<string> syntaxSet);
            //    if (syntaxSet != null) { 
            //        sb.AppendLine($"Line {i}: {string.Join(", ", syntaxSet)}");
            //    }
            //}
            //Debug.Log(sb.ToString());
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
    public bool CheckASTPattern(int startLine, int endLine, string pattern, string target)
    {
        if (pyScope == null || string.IsNullOrEmpty(currentCode)) return false;

        using (Py.GIL())
        {
            try
            {
                string result = GetCachedPyFunction("check_ast_pattern")(currentCode, startLine, endLine, pattern, target).ToString();

                return result == "True";
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("AST Pattern Check Failed: " + e.Message);
                return false;
            }
        }
    }
}