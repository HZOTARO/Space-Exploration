using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PythonExecutor : MonoBehaviour
{
    public static PythonExecutor instance;

    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public CodeEditor codeEditor;

    PyModule pyScope;
    dynamic pyPrepareFunc;
    dynamic pyStepFunc;
    private Dictionary<string, Delegate> pythonFunctions;

    [HideInInspector] public string currentCode;
    public bool continuous = false;
    bool lockDelay = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPython()
    {
        if (!PythonEngine.IsInitialized)
        {
            Runtime.PythonDLL = System.IO.Path.Combine(Application.streamingAssetsPath, "python-3.13.11-embed-amd64", "python313.dll");
            PythonEngine.Initialize();

            Application.quitting += ShutDownPythonEngine;
        }

        if (instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("PythonExecutor");

            if (prefab != null)
            {
                Instantiate(prefab);
            }
            else
            {
                Debug.LogError("Could not find 'PythonExecutor' prefab in a Resources folder!");
            }
        }
    }
    static void ShutDownPythonEngine()
    {
        if (instance != null)
        {
            instance.CleanupPythonEnvironment();
        }

        if (PythonEngine.IsInitialized)
        {
            PythonEngine.Shutdown();
            Debug.Log("Python Engine Shutdown");
        }
    }
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (instance == this)
        {
            BindLevelFunctions();
        }
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance == this)
        {
            StopRunningCode();

            BindLevelFunctions();
        }
    }

    /// <summary>
    /// Finds the new managers and overwrites the function in pyScope
    /// </summary>
    void BindLevelFunctions()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        codeEditor = FindFirstObjectByType<CodeEditor>();

        if (gameManager == null) return;

        pythonFunctions = new Dictionary<string, Delegate>()
        {
            { "move_up", new Action(() => gameManager.Move("N")) },
            { "move_down", new Action(() => gameManager.Move("S")) },
            { "move_left", new Action(() => gameManager.Move("W")) },
            { "move_right", new Action(() => gameManager.Move("E")) },

            { "mine", new Action(() => gameManager.Mine()) },
            { "collect", new Action(() => gameManager.Collect()) },
            { "purify", new Action(() => gameManager.Purify()) },
            { "drill", new Action(() => gameManager.Drill()) },
            { "pump", new Action(() => gameManager.Pump()) },

            { "scan", new Func<string>(() => gameManager.Scan()) },
            { "measure", new Action(() => gameManager.Measure()) }
        };

        using (Py.GIL())
        {
            foreach (var function in pythonFunctions)
            {
                pyScope.Set(function.Key, function.Value);
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
                pyScope.Exec(@"
global __gen__
__gen__ = None
"
                );
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
            string scriptCode = File.ReadAllText(filePath);

            using (Py.GIL())
            {
                pyScope.Set("unity_log", new Action<string>(LogFromPython));
                pyScope.Exec(scriptCode);
            }
        }
        else
        {
            Debug.LogError("Could not find logger_setup.py in StreamingAssets!");
        }
    }

    void SetupStep()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "PythonSystem", "ast_setup.py");

        if (File.Exists(filePath))
        {
            string scriptCode = File.ReadAllText(filePath);

            using (Py.GIL())
            {
                pyScope.Exec(scriptCode);

                pyPrepareFunc = pyScope.Get("prepare");
                pyStepFunc = pyScope.Get("step");
            }
        }
        else
        {
            Debug.LogError("Could not find ast_setup.py in StreamingAssets!");
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

                codeEditor.isPlaying = true;
                codeEditor.PlayAbort();
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

                if (pythonFunctions != null)
                {
                    foreach (var function in pythonFunctions)
                    {
                        pyScope.Set(function.Key, null);
                    }
                }

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
