using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintUINew : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject hintPanel;

    [Header("Content References")]
    public TextMeshProUGUI mainTitleText;
    public Image hintImage;
    public TextMeshProUGUI hintDescriptionText;

    [Header("Pagination Controls")]
    public GameObject paginationPanel;
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI pageText;

    private struct HintPage
    {
        public HintSO hint;
        public bool showImageOnly;
        public bool showTextOnly;
    }

    private List<HintPage> activePages = new List<HintPage>();
    private int currentPageIndex = 0;

    private void Awake()
    {
        HintManager.OnDisplayHintsRequested += OpenUIWithHints;
        HintManager.OnCloseHintsRequested += CloseHint;
    }

    private void OnDestroy()
    {
        HintManager.OnDisplayHintsRequested -= OpenUIWithHints;
        HintManager.OnCloseHintsRequested -= CloseHint;
    }

    private void Start()
    {
        hintPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PreviousPage);
    }

    private void OpenUIWithHints(List<HintSO> hints, int openedHintIndex)
    {
        if (hints == null || hints.Count == 0) return;

        activePages.Clear();
        int targetPageIndex = 0;

        for (int i = 0; i < hints.Count; i++)
        {
            HintSO hint = hints[i];

            if (i == openedHintIndex)
            {
                targetPageIndex = activePages.Count;
            }

            if (hint.isImageBig && hint.image != null)
            {
                activePages.Add(new HintPage { hint = hint, showImageOnly = true, showTextOnly = false });
                activePages.Add(new HintPage { hint = hint, showImageOnly = false, showTextOnly = true });
            }
            else
            {
                activePages.Add(new HintPage { hint = hint, showImageOnly = false, showTextOnly = false });
            }
        }

        currentPageIndex = targetPageIndex;
        hintPanel.SetActive(true);
        paginationPanel.SetActive(activePages.Count > 1);

        UpdatePageVisuals();
    }

    private void UpdatePageVisuals()
    {
        HintPage currentPage = activePages[currentPageIndex];
        HintSO currentHint = currentPage.hint;

        if (mainTitleText != null)
        {
            mainTitleText.text = currentHint.title;
        }

        if (hintImage != null)
        {
            if (currentHint.image != null && !currentPage.showTextOnly)
            {
                hintImage.sprite = currentHint.image;
                hintImage.gameObject.SetActive(true);
            }
            else
            {
                hintImage.gameObject.SetActive(false);
            }
        }

        if (hintDescriptionText != null)
        {
            if (!string.IsNullOrEmpty(currentHint.description) && !currentPage.showImageOnly)
            {
                hintDescriptionText.text = currentHint.description;
                hintDescriptionText.gameObject.SetActive(true);
            }
            else
            {
                hintDescriptionText.gameObject.SetActive(false);
            }
        }

        bool hasMultiplePages = activePages.Count > 1;

        if (pageText != null)
        {
            pageText.gameObject.SetActive(hasMultiplePages);
            if (hasMultiplePages) pageText.text = $"{currentPageIndex + 1} / {activePages.Count}";
        }

        if (nextButton != null)
        {
            nextButton.enabled = hasMultiplePages && currentPageIndex < activePages.Count - 1;
            nextButton.GetComponent<Image>().color = nextButton.enabled ? Color.white : Color.gray;
        }

        if (prevButton != null)
        {
            prevButton.enabled = hasMultiplePages && currentPageIndex > 0;
            prevButton.GetComponent<Image>().color = prevButton.enabled ? Color.white : Color.gray;
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < activePages.Count - 1)
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
        activePages.Clear();
    }
}