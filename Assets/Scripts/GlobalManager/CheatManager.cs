using TMPro;
using UnityEngine;
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
        if (saveData == null || saveDataText == null || InventoryManager.instance == null) return;

        int whiteOre = InventoryManager.instance.GetAmount("white_ore");
        int purpleLiquid = InventoryManager.instance.GetAmount("purple_liquid");
        int blackOre = InventoryManager.instance.GetAmount("black_ore");

        saveDataText.text = $"White Ore: {whiteOre}\n" +
                            $"Purple Liquid: {purpleLiquid}\n" +
                            $"Black Ore: {blackOre}\n" +
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
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("white_ore", 100);
                Debug.Log("<color=green>CHEAT: Added 100 White Ore!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("100 Purple Liquid", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("purple_liquid", 100);
                Debug.Log("<color=green>CHEAT: Added 100 Purple Liquid!</color>");
                SaveManager.instance.UpdateAllUI();
            }
        });

        CreateCheatButton("100 Black Ore", () =>
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem("black_ore", 100);
                Debug.Log("<color=green>CHEAT: Added 100 Black Ore!</color>");
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