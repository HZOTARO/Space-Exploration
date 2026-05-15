using System;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager instance;

    public static event Action<List<HintSO>, int> OnDisplayHintsRequested;
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

    //public void RequestDisplayHints(List<string> hintIds, string openedHintId = "", bool setHasAppeared = true)
    //{
    //    List<HintSO> hintsToDisplay = new List<HintSO>();
    //    HintSO openedHint = null;

    //    foreach (string id in hintIds)
    //    {
    //        if (hintDatabase.TryGetValue(id, out HintSO foundHint))
    //        {
    //            hintsToDisplay.Add(foundHint);
    //            if (foundHint.hintId == openedHintId)
    //            {
    //                openedHint = foundHint;
    //            }
    //        }
    //    }

    //    RequestDisplayHints(hintsToDisplay, openedHint);
    //}

    public void RequestDisplayHints(List <HintSO> hintsToDisplay, HintSO openedHint = null, bool setHasAppeared = true, bool onlyShowNewOne = false)
    {
        List<HintSO> validHintsToDisplay = new List<HintSO>();
        bool dataWasChanged = false;

        if (hintsToDisplay != null)
        {
            for (int i = 0; i < hintsToDisplay.Count; i++)
            {
                HintSO hint = hintsToDisplay[i];
                HintSaveState saveState = GetHintSaveState(hint);

                if (!IsHintUnlocked(hint)) continue;

                if (setHasAppeared && !saveState.hasAppeared)
                {
                    saveState.hasAppeared = true;
                    dataWasChanged = true;
                }

                if (setHasAppeared) saveState.hasAppeared = true;
                validHintsToDisplay.Add(hint);
            }

            if (dataWasChanged)
            {
                SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
            }
        }
        if (validHintsToDisplay.Count > 0)
        {
            int startIndex = Mathf.Max(0, validHintsToDisplay.IndexOf(openedHint));
            OnDisplayHintsRequested?.Invoke(validHintsToDisplay, startIndex);
        }
    }

    public void RequestDisplayHints(HintCollectionSO hintCollection, HintSO openedHint = null, bool setHasAppeared = true, bool onlyShowNewOne = false)
    {
        RequestDisplayHints(hintCollection.hints, openedHint, setHasAppeared, onlyShowNewOne);
    }

    public void RequestCloseHints()
    {
        OnCloseHintsRequested?.Invoke();
    }

    private HintSaveState GetHintSaveState(HintSO hint)
    {
        if (SaveManager.saveData == null) return null;

        foreach (HintSaveState saveState in SaveManager.saveData.hints)
        {
            if (saveState.id == hint.hintId) return saveState;
        }

        HintSaveState newState = new HintSaveState();
        newState.id = hint.hintId;
        SaveManager.saveData.hints.Add(newState);
        return newState;
    }

    private bool IsHintUnlocked(HintSO hint)
    {
        if (hint.isUnlockedByDefault) return true;

        if (SaveManager.saveData != null)
        {
            foreach (HintSaveState saveState in SaveManager.saveData.hints)
            {
                if (saveState.id == hint.hintId && saveState.isUnlocked)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void UnlockHint(HintSO hint, bool showHint, bool setHasAppeared)
    {
        HintSaveState state = GetHintSaveState(hint);

        state.isUnlocked = true;

        if (showHint)
        {
            if (setHasAppeared) state.hasAppeared = true;

            RequestDisplayHints(new List<HintSO> { hint }, hint);
        }

        SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
    }
}