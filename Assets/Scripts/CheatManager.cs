using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
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
                UpdateSaveDataText();
            }
        }
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
            }
        });

        CreateCheatButton("100 Purple Liquid", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.saveData.purpleLiquid += 100;
                Debug.Log("<color=green>CHEAT: Added 100 Purple Liquid!</color>");
            }
        });

        CreateCheatButton("100 Black Ore", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.saveData.blackOre += 100;
                Debug.Log("<color=green>CHEAT: Added 100 Black Ore!</color>");
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
            }
        });

        CreateCheatButton("Delete Save", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.DeleteSave(1);
                SaveManager.saveData = new SaveData();
                Debug.Log("<color=red>CHEAT: Save File Deleted.</color>");
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

    void UpdateSaveDataText()
    {
        if (saveDataText != null && SaveManager.saveData != null)
        {
            saveDataText.text = $"White Ore: {SaveManager.saveData.whiteOre}\n" +
                                $"Purple Liquid: {SaveManager.saveData.purpleLiquid}\n" +
                                $"Black Ore: {SaveManager.saveData.blackOre}\n" +
                                $"Last Saved: {SaveManager.saveData.lastSavedTime}";
        }
    }

#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}