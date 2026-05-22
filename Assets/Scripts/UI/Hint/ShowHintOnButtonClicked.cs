using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShowHintOnButtonClicked : MonoBehaviour
{
    public HintSO unlockedHint;
    void Start()
    {
        Button button = GetComponent<Button>();
        if (unlockedHint)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        Debug.Log("Button clicked, unlocking hint: " + unlockedHint.hintId);
        if (HintManager.instance) HintManager.instance.RequestDisplayHints(new List<HintSO> { unlockedHint }, null, true, true);
    }
}
