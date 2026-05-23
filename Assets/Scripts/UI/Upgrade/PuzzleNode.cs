using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleNode : MonoBehaviour
{
    public PuzzleSO puzzleData;

    [Header("UI References")]
    public Button nodeButton;
    public Image nodeBackground;
    public TextMeshProUGUI nodeLabel;

    [Header("Visual States")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
    public Color availableColor = Color.white;
    public Color completedColor = Color.green;

    private UpgradeUI upgradeUI;

    void Start()
    {
        upgradeUI = FindFirstObjectByType<UpgradeUI>();
        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(OnClick);
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (puzzleData == null) return;

        nodeLabel.text = puzzleData.puzzleName;

        bool isCompleted = SaveManager.saveData.levelCompleted.Contains(puzzleData.id);
        bool hasPrereq = UpgradeManager.instance.HasPrerequisite(puzzleData);

        if (isCompleted)
        {
            SetColor(completedColor);
        }
        else if (hasPrereq)
        {
            SetColor(availableColor);
        }
        else
        {
            SetColor(lockedColor);
        }
    }

    private void OnClick()
    {
        if (upgradeUI != null)
        {
            upgradeUI.SelectNode(this);
        }
    }

    private void SetColor(Color color)
    {
        nodeBackground.color = color;
        nodeLabel.color = color;
    }
}