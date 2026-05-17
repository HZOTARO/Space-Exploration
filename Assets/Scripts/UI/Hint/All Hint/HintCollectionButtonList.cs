using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintCollectionButtonList : MonoBehaviour
{
    public TextMeshProUGUI collectionNameText;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    private List<HintButton> spawnedButtons = new List<HintButton>();

    public void Setup(string title, HintCollectionSO collection, AllHintPage allHint)
    {
        if (collectionNameText != null) collectionNameText.text = title;

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        foreach (HintSO hint in collection.hints)
        {
            bool isUnlocked = true;
            if (HintManager.instance != null) isUnlocked = HintManager.instance.IsHintUnlocked(hint);

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            HintButton hintBtnScript = buttonObj.GetComponent<HintButton>();

            if (hintBtnScript != null)
            {
                hintBtnScript.Setup(hint, isUnlocked, collection, allHint);
                spawnedButtons.Add(hintBtnScript);
            }
        }
    }

    public void RefreshCollection()
    {
        foreach (HintButton btn in spawnedButtons)
        {
            if (btn != null) btn.RefreshState();
        }
    }
}
