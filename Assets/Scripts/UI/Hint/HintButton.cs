using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HintButton : MonoBehaviour
{
    [Header("Hint Data")]
    public HintCollectionSO hintCollection;
    public HintSO openedHint;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnHintButtonClicked);
    }

    private void OnHintButtonClicked()
    {
        if (hintCollection == null) return;
        if (HintManager.instance != null) HintManager.instance.RequestDisplayHints(hintCollection, openedHint);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnHintButtonClicked);
    }
}