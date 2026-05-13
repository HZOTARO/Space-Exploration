using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hintPanel;
    public Transform contentContainer;
    public HintContentUI blockPrefab;

    [Header("Main Content References")]
    public TextMeshProUGUI mainTitleText;
    public ScrollRect hintScrollView;

    [Header("Pagination UI")]
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI pageText;

    private List<HintSO> activeHints = new List<HintSO>();
    private int currentPageIndex = 0;

    private void OnEnable()
    {
        HintManager.OnDisplayHintsRequested += OpenUIWithHints;
        HintManager.OnCloseHintsRequested += CloseHint;
    }

    private void OnDisable()
    {
        HintManager.OnDisplayHintsRequested -= OpenUIWithHints;
        HintManager.OnCloseHintsRequested -= CloseHint;
    }

    private void Start()
    {
        hintPanel.SetActive(false);
    }

    private void OpenUIWithHints(List<HintSO> hints)
    {
        activeHints = new List<HintSO>(hints);
        currentPageIndex = 0;
        hintPanel.SetActive(true);

        UpdatePageVisuals();
    }

    private void UpdatePageVisuals()
    {
        HintSO currentHint = activeHints[currentPageIndex];

        if (mainTitleText != null)
        {
            mainTitleText.text = currentHint.title;
        }

        foreach (Transform child in contentContainer)
        {
            if (child != mainTitleText.transform)
                Destroy(child.gameObject);
        }

        if (currentHint.hintBlocks != null)
        {
            foreach (HintBlock block in currentHint.hintBlocks)
            {
                HintContentUI newBlockUI = Instantiate(blockPrefab, contentContainer);
                newBlockUI.Setup(block);
            }
        }

        bool hasMultiplePages = activeHints.Count > 1;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(hasMultiplePages);
            nextButton.interactable = (currentPageIndex < activeHints.Count - 1);
        }

        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(hasMultiplePages);
            prevButton.interactable = (currentPageIndex > 0);
        }

        if (pageText != null)
        {
            pageText.gameObject.SetActive(hasMultiplePages);
            pageText.text = $"{currentPageIndex + 1} / {activeHints.Count}";
        }

        if (hintScrollView != null)
        {
            Canvas.ForceUpdateCanvases();
            hintScrollView.normalizedPosition = new Vector2(0f, 1f);
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < activeHints.Count - 1)
        {
            currentPageIndex++;
            UpdatePageVisuals();
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisuals();
        }
    }

    public void CloseHint()
    {
        hintPanel.SetActive(false);
        activeHints.Clear();
    }
}
