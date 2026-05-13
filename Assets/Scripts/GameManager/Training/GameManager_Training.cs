using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager_Training : GameManager
{
    [Header("UI References")]
    public Button levelCompletePopup;

    private bool hasFinished = false;

    protected override void Start()
    {
        SetLevelObjectives();
        base.Start();
        ObjectiveManager.instance.InitiateAllTask();
        ObjectiveManager.instance.OnAllObjectiveComplete += OnLevelComplete;

        if (levelCompletePopup != null)
        {
            levelCompletePopup.gameObject.SetActive(false);
            levelCompletePopup.onClick.AddListener(FinishAndLeaveLevel);
        }
    }
    protected virtual void SetLevelObjectives()
    {
        if (ObjectiveManager.instance == null) return;
    }
    protected virtual void OnLevelComplete()
    {
        if (levelCompletePopup != null)
        {
            levelCompletePopup.gameObject.SetActive(true);
        }

        StartCoroutine(AutoFinishTimer());
    }
    private IEnumerator AutoFinishTimer()
    {
        yield return new WaitForSeconds(5f);
        FinishAndLeaveLevel();
    }

    private void FinishAndLeaveLevel()
    {
        if (hasFinished) return;
        hasFinished = true;

        LevelComplete();
    }

    public override void LevelComplete()
    {
        if (!SaveManager.saveData.levelCompleted.Contains(PlayerPrefs.GetString("CurrentLevelId")))
        {
            SaveManager.saveData.levelCompleted.Add(PlayerPrefs.GetString("CurrentLevelId"));
            SaveManager.instance.SaveGame(SaveManager.saveSlotInUse);
        }

        Debug.Log("<color=green>Training Completed!</color>");
        LevelManager.instance.OpenScene(LevelType.Hub);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (ObjectiveManager.instance != null) ObjectiveManager.instance.OnAllObjectiveComplete -= OnLevelComplete;
        if (levelCompletePopup != null) levelCompletePopup.onClick.RemoveListener(FinishAndLeaveLevel);
    }
}