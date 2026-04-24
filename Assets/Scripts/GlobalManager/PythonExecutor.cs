using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PythonExecutor : MonoBehaviour
{
    public static PythonExecutor instance;

    PyModule pyScope;
    dynamic pyPrepareFunc;
    dynamic pyStepFunc;
    private List<string> registeredFunctionNames = new List<string>();

    [HideInInspector] public string currentCode;
    public bool continuous = false;
    bool lockDelay = false;

    public event Action OnExecutionFinished;
    public event Action<string> OnPythonPrint;
    public event Action<int, int> OnLineExecuted;

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
            Debug.Log("Python result: " + result);

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

        lockDelay = true;
        Invoke("UnlockDelay", 0.1f);
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
        Debug.Log(message);

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
}
