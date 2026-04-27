using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UpgradeSaveState
{
    public string id;
    public int currentLevel;
}

[System.Serializable]
public class SaveData
{
    public string lastSavedTime = "Never";

    // Currency
    public int whiteOre;
    public int purpleLiquid;
    public int blackOre;
    public int partsA;
    public int partsB;
    public int partsC;
    //public Dictionary<item, int> item
    //public Dictionary<upgrade, bool> upgrade
    public List<UpgradeSaveState> unlockedUpgrades;
}
public interface IResourceUpdatable
{
    void UpdateResource(SaveData saveData);
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public static SaveData saveData;
    public static int saveSlotInUse = 1;

    private IResourceUpdatable[] resourceUpdateables;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnLevelLoaded;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate SaveManager destroyed!");
            Destroy(gameObject);
        }
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        resourceUpdateables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                                        .OfType<IResourceUpdatable>()
                                        .ToArray();
    }

    public void CreateNewSaveData()
    {
        saveData = new SaveData
        {
            unlockedUpgrades = new()
        };
    }

    public void UpdateAllUI()
    {
        foreach (IResourceUpdatable updatable in resourceUpdateables)
        {
            updatable.UpdateResource(SaveManager.saveData);
        }
    }

    private string GetFilePath(int slotNumber)
    {
#if UNITY_EDITOR
        string devPath = Application.dataPath + "/../SaveData";

        if (!Directory.Exists(devPath))
        {
            Directory.CreateDirectory(devPath);
        }

        return devPath + "/SaveSlot_" + slotNumber + ".json";
#else
        return Application.persistentDataPath + "/SaveSlot_" + slotNumber + ".json";
#endif
    }

    public void SaveGame(int slotNumber)
    {
        saveData.lastSavedTime = System.DateTime.Now.ToString("dd-MM-yyyy HH:mm");

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(GetFilePath(slotNumber), json);

        Debug.Log($"Successfully saved game to Slot {slotNumber}");
    }

    public void LoadGame(int slotNumber)
    {
        string path = GetFilePath(slotNumber);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            saveData = JsonUtility.FromJson<SaveData>(json);

            Debug.Log($"Successfully loaded game from Slot {slotNumber}");
        }
        else
        {
            Debug.LogWarning($"Save Slot {slotNumber} is empty. Creating fresh data!");
            CreateNewSaveData();
        }
    }

    public bool DoesSaveExist(int slotNumber)
    {
        return File.Exists(GetFilePath(slotNumber));
    }

    public void DeleteSave(int slotNumber)
    {
        string path = GetFilePath(slotNumber);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save data in Slot {slotNumber}");
        }

        CreateNewSaveData();
    }

    public SaveData GetSaveData(int slotNumber)
    {
        string path = GetFilePath(slotNumber);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData tempSaveData = JsonUtility.FromJson<SaveData>(json);
            return tempSaveData;
        }

        return null;
    }

    public int GetResourceAmount(ResourceType type)
    {
        if (saveData == null) return 0;

        if (type == ResourceType.WhiteOre) return saveData.whiteOre;
        if (type == ResourceType.PurpleLiquid) return saveData.purpleLiquid;
        if (type == ResourceType.BlackOre) return saveData.blackOre;
        return 0;
    }

    public void ConsumeResource(ResourceType type, int amount)
    {
        if (saveData == null) return;

        if (type == ResourceType.WhiteOre) saveData.whiteOre -= amount;
        if (type == ResourceType.PurpleLiquid) saveData.purpleLiquid -= amount;
        if (type == ResourceType.BlackOre) saveData.blackOre -= amount;
    }
}
