using UnityEngine;
using System.Collections.Generic;

public class HubUI : MonoBehaviour, IResourceUpdatable
{
    private Button_Id[] buttons;
    public List<HintSO> firstOpenHint;
    public List<HintSO> afterTraining1;

    private void Awake()
    {
        buttons = GetComponentsInChildren<Button_Id>(true);
    }

    void Start()
    {
        UpdateResource(SaveManager.saveData);
        if (firstOpenHint != null && HintManager.instance != null) 
        { 
            HintManager.instance.RequestDisplayHints(firstOpenHint, null, true, true);
        }
        if (afterTraining1 != null && HintManager.instance != null && SaveManager.saveData.levelCompleted.Contains("Tutorial 1"))
        {
            HintManager.instance.RequestDisplayHints(afterTraining1, null, true, true);
        }
    }

    public void UpdateResource(SaveData saveData)
    {
        foreach (Button_Id button in buttons)
        {
            button.SetupButton();
        }
    }
}