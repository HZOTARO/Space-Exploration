using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class Button_Id : MonoBehaviour
{
    [Header("Level Settings")]
    public string requiredLevelId;
    public string levelId;
    public UpgradeSO upgradeRequirement;
    public LevelType levelType;

    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        InitializeButton();
    }
    private void Start()
    {
        SetupButton();
    }
    private void InitializeButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            button.onClick.AddListener(OnClick);
        }
    }
    public void SetupButton()
    {
        InitializeButton();

        bool isUnlocked = false;

        if (string.IsNullOrEmpty(requiredLevelId))
        {
            isUnlocked = true;
        }
        else if (SaveManager.saveData != null && SaveManager.saveData.levelCompleted != null && SaveManager.saveData.levelCompleted.Contains(requiredLevelId))
        {
            isUnlocked = true;
        }

        if (upgradeRequirement != null)
        {
            isUnlocked = isUnlocked && UpgradeManager.instance.IsUpgradeUnlocked(upgradeRequirement.id);
        }

        if (isUnlocked)
        {
            button.interactable = true;
            buttonImage.color = Color.white;
        }
        else
        {
            button.interactable = false;
            buttonImage.color = new Color32(25, 25, 25, 255);
        }
    }

    private void OnClick()
    {
        PlayerPrefs.SetString("CurrentLevelId", levelId);
        LevelManager.instance.OpenScene(levelType);
    }
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
}
