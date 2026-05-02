using Python.Runtime;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameBootstrapper
{
    // This runs automatically once before the first scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeAllSystems()
    {
        if (!PythonEngine.IsInitialized)
        {
            Runtime.PythonDLL = Path.Combine(Application.streamingAssetsPath, "python-3.13.11-embed-amd64", "python313.dll");
            PythonEngine.Initialize();

            Application.quitting += ShutDownPython;
            Debug.Log("Bootstrapper: Python Engine Started.");
        }

        SpawnSystemPrefab("PythonExecutor");
        SpawnSystemPrefab("CraftingManager");
        SpawnSystemPrefab("InventoryManager");
        SpawnSystemPrefab("UpgradeManager");
        SpawnSystemPrefab("LevelManager");
        SpawnSystemPrefab("HintManager");
        SpawnSystemPrefab("SaveManager");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        SpawnSystemPrefab("CheatManager");
        if (SceneManager.GetActiveScene().name != "Main Menu Scene")
        {
            SaveManager.instance.LoadGame(SaveManager.saveSlotInUse);
        }
#endif
    }

    private static void SpawnSystemPrefab(string prefabName)
    {
        if (GameObject.Find(prefabName) != null)
        {
            Debug.LogWarning($"Bootstrapper: Skipped spawning {prefabName} because one is already in the scene.");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabName);

        if (prefab != null)
        {
            GameObject spawned = GameObject.Instantiate(prefab);
            spawned.name = prefabName;
        }
        else
        {
            Debug.LogError($"Bootstrapper Crash: Could not find '{prefabName}' in a Resources folder!");
        }
    }

    private static void ShutDownPython()
    {
        if (PythonExecutor.instance != null)
        {
            PythonExecutor.instance.CleanupPythonEnvironment();
        }

        if (PythonEngine.IsInitialized)
        {
            PythonEngine.Shutdown();
            Debug.Log("Bootstrapper: Python Engine safely shut down.");
        }
    }
}