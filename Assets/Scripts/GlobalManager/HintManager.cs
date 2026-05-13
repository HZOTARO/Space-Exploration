using System;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager instance;

    public static event Action<List<HintSO>> OnDisplayHintsRequested;
    public static event Action OnCloseHintsRequested;

    private Dictionary<string, HintSO> hintDatabase = new Dictionary<string, HintSO>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadDatabase()
    {
        HintSO[] loadedHints = Resources.LoadAll<HintSO>("Hints");
        foreach (HintSO hint in loadedHints)
        {
            if (!hintDatabase.ContainsKey(hint.hintId))
            {
                hintDatabase.Add(hint.hintId, hint);
            }
            else
            {
                Debug.LogWarning($"Duplicate Hint ID found in Resources: {hint.hintId}. Skipping...");
            }
        }
    }

    public void RequestDisplayHints(List<string> hintIds)
    {
        List<HintSO> hintsToDisplay = new List<HintSO>();

        foreach (string id in hintIds)
        {
            if (hintDatabase.TryGetValue(id, out HintSO foundHint))
            {
                hintsToDisplay.Add(foundHint);
            }
        }

        if (hintsToDisplay.Count > 0)
        {
            OnDisplayHintsRequested?.Invoke(hintsToDisplay);
        }
    }

    public void RequestDisplayHints(List <HintSO> hintsToDisplay)
    {
        if (hintsToDisplay != null && hintsToDisplay.Count > 0)
        {
            OnDisplayHintsRequested?.Invoke(hintsToDisplay);
        }
    }

    public void RequestDisplayHints(HintCollectionSO hintCollection)
    {
        if (hintCollection != null && hintCollection.hints != null && hintCollection.hints.Count > 0)
        {
            OnDisplayHintsRequested?.Invoke(hintCollection.hints);
        }
    }

    public void RequestCloseHints()
    {
        OnCloseHintsRequested?.Invoke();
    }
}