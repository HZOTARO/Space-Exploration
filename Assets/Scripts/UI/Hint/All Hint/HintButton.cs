using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class HintButton : MonoBehaviour
{
    [HideInInspector] public HintSO hintData;
    [HideInInspector] public HintCollectionSO hintCollection;

    public TextMeshProUGUI buttonText;
    public Button button;
    public Image buttonImage;

    private AllHintPage pageManager;

    public void Setup(HintSO data, bool isUnlocked, HintCollectionSO collection, AllHintPage manager)
    {
        hintData = data;
        hintCollection = collection;
        pageManager = manager;

        if (buttonText != null) buttonText.text = hintData.title;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        RefreshState();
    }

    public void RefreshState()
    {
        if (hintData == null) return;

        bool isUnlocked = true;
        if (HintManager.instance != null) isUnlocked = HintManager.instance.IsHintUnlocked(hintData);

        if (isUnlocked)
        {
            if (buttonText != null) buttonText.color = Color.white;
            if (buttonImage != null) buttonImage.color = Color.white;
            button.interactable = true;
        }
        else
        {
            if (buttonText != null) buttonText.color = new Color(0.3f, 0.3f, 0.3f);
            if (buttonImage != null) buttonImage.color = new Color(0.3f, 0.3f, 0.3f);
            button.interactable = false;
        }
    }

    private void OnClick()
    {
        if (pageManager != null)
        {
            pageManager.OnHintButtonClicked(this);
        }
    }
}
