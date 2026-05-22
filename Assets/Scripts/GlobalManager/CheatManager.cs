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

        CreateCheatButton("500 White Ore", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("white_ore", 500);
                Debug.Log("<color=green>CHEAT: Added 500 White Ore!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("500 Purple Liquid", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("purple_liquid", 500);
                Debug.Log("<color=green>CHEAT: Added 500 Purple Liquid!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("500 Black Ore", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("black_ore", 500);
                Debug.Log("<color=green>CHEAT: Added 500 Black Ore!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("500 Part A", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("part_a", 500);
                Debug.Log("<color=green>CHEAT: Added 500 Part A!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("500 Part B", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("part_b", 500);
                Debug.Log("<color=green>CHEAT: Added 500 Part B!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Save Data", () =>
        {
            if (SaveManager.instance != null)
            {
                if (InventoryManager.instance != null) SaveManager.saveData.inventory = InventoryManager.instance.GetInventoryForSave();
                if (UpgradeManager.instance != null) UpgradeManager.instance.SyncToSaveData();

                SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
                Debug.Log("<color=green>CHEAT: Game Saved.</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Load Save", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.LoadGame(SaveManager.saveSlotInUse);

                if (InventoryManager.instance != null) InventoryManager.instance.LoadInventory(SaveManager.saveData.inventory);
                if (UpgradeManager.instance != null) UpgradeManager.instance.LoadUpgradesFromSave();

                Debug.Log("<color=green>CHEAT: Save Loaded.</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Delete Save", () =>
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.DeleteSave(SaveManager.saveSlotInUse);
                SaveManager.instance.CreateNewSaveData();

                if (InventoryManager.instance != null) InventoryManager.instance.LoadInventory(SaveManager.saveData.inventory);
                if (UpgradeManager.instance != null) UpgradeManager.instance.LoadUpgradesFromSave();

                Debug.Log("<color=red>CHEAT: Save File Deleted.</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("Unlock All Upgrade", () =>
        {
            if (SaveManager.instance != null && UpgradeManager.instance != null)
            {
                foreach (UpgradeSO upgrade in UpgradeManager.instance.allUpgrades)
                {
                    int maxLevel = upgrade.tiers.Length;

                    if (!UpgradeManager.instance.playerUpgrades.ContainsKey(upgrade.id))
                    {
                        UpgradeManager.instance.playerUpgrades[upgrade.id] = new UpgradeSaveState { id = upgrade.id, currentLevel = maxLevel };
                    }
                    else
                    {
                        UpgradeManager.instance.playerUpgrades[upgrade.id].currentLevel = maxLevel;
                    }
                }

                SaveManager.instance.UpdateAllUI();

                Debug.Log("<color=magenta>Cheat Activated: All Upgrades Maxed Out!</color>");
            }
        });

        CreateCheatButton("Unlock All Hints", () =>
        {
            if (SaveManager.instance != null && HintManager.instance != null)
            {
                foreach (string hintId in HintManager.instance.hintDatabase.Keys)
                {
                    HintSaveState newHint = new HintSaveState { id = hintId, isUnlocked = true, hasAppeared = true };
                    if (!SaveManager.saveData.hints.Contains(newHint))
                    {
                        SaveManager.saveData.hints.Add(newHint);
                    }
                }
                SaveManager.instance.UpdateAllUI();
                Debug.Log("<color=magenta>Cheat Activated: All Hints Unlocked!</color>");
            }
        });

        CreateCheatButton("Unlock Tutorials", () =>
        {
            if (SaveManager.instance != null)
            {
                for (int i = 1; i < 21; i++)
                {
                    SaveManager.saveData.levelCompleted.Add($"Tutorial {i}");
                }
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