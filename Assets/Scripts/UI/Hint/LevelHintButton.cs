using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
public class LevelHintButton : MonoBehaviour
{
    [Header("Hint Data")]
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

        List<HintSO> list = new List<HintSO>();
        if (hintCollection != null) list.AddRange(hintCollection.hints);
        if (hintList != null) list.AddRange(hintList);

        HintManager.instance.RequestDisplayHints(list, openedHint, true, false);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnHintButtonClicked);
    }
}