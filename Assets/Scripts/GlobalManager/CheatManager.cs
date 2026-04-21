using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour, IResourceUpdatable
{
    public static CheatManager instance;
    [Header("UI References")]
    public GameObject cheatPanel;
    public Transform buttonParent;
    public GameObject buttonPrefab;
    public TextMeshProUGUI saveDataText;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SetupCheatMenu();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (cheatPanel != null)
            {
                cheatPanel.SetActive(!cheatPanel.activeSelf);
                UpdateResource(SaveManager.saveData);
            }
        }
    }

    public void UpdateResource(SaveData saveData)
    {
        if (saveData == null || saveDataText == null) return;

        saveDataText.text = $"White Ore: {saveData.whiteOre}\n" +
                            $"Purple Liquid: {saveData.purpleLiquid}\n" +
                            $"Black Ore: {saveData.blackOre}\n" +
                            $"Last Saved: {saveData.lastSavedTime}";

    }

    void SetupCheatMenu()
    {
        if (cheatPanel != null) cheatPanel.SetActive(false);

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        CreateCheatButton("100 White Ore", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.saveData.whiteOre += 100;
                Debug.Log("<color=green>CHEAT: Added 100 White Ore!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("100 Purple Liquid", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.saveData.purpleLiquid += 100;
                Debug.Log("<color=green>CHEAT: Added 100 Purple Liquid!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("100 Black Ore", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.saveData.blackOre += 100;
                Debug.Log("<color=green>CHEAT: Added 100 Black Ore!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Save Data", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.SaveGame(1);
                Debug.Log("<color=green>CHEAT: Game Saved.</color>");
            }
        });

        CreateCheatButton("Load Save", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.LoadGame(1);
                Debug.Log("<color=green>CHEAT: Save Loaded.</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Delete Save", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.DeleteSave(1);
                SaveManager.saveData = new SaveData();
                Debug.Log("<color=red>CHEAT: Save File Deleted.</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });
    }

    void CreateCheatButton(string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        if (buttonPrefab == null || buttonParent == null) return;

        GameObject newBtn = Instantiate(buttonPrefab, buttonParent);

        Button btnComp = newBtn.GetComponent<Button>();
        if (btnComp != null)
        {
            btnComp.onClick.AddListener(onClickAction);

            TextMeshProUGUI txt = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = buttonText;
            }
        }
    }

#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}