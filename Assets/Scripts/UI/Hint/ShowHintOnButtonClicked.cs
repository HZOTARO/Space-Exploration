using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShowHintOnButtonClicked : MonoBehaviour
{
    HintSO unlockedHint;
    void Start()
    {
        Button button = GetComponent<Button>();
        if (unlockedHint)
        {
            button.onClick.AddListener(() =>
            {
                if (HintManager.instance)
                {
                    HintManager.instance.UnlockHint(unlockedHint, true, true);
                }
            });
        }
    }
}
