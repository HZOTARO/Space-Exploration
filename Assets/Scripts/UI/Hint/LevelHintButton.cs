using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class LevelHintButton : MonoBehaviour
{
    [Header("Hint Data")]
    bool useHintCollection = true;
    public HintCollectionSO hintCollection;
    public List<HintSO> hintList;
    public HintSO openedHint;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnHintButtonClicked);
    }

    private void OnHintButtonClicked()
    {
        if (HintManager.instance == null) return;
        if (useHintCollection && hintCollection != null)
        {
            HintManager.instance.RequestDisplayHints(hintCollection, openedHint, false);
        }
        else if (hintList != null && hintList.Count > 0)
        {
            HintManager.instance.RequestDisplayHints(hintList, openedHint, false);
        }
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnHintButtonClicked);
    }
}