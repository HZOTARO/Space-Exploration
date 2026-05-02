using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    public static HintManager instance { get; private set; }

    [Header("UI References")]
    public GameObject hintPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Image hintImage;
    public ScrollRect hintScrollView;

    [Header("Pagination UI")]
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI pageText;

    private Dictionary<string, HintSO> hintDatabase = new Dictionary<string, HintSO>();

    private List<HintSO> activeHints = new List<HintSO>();
    private int currentPageIndex = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            HintSO[] loadedHints = Resources.LoadAll<HintSO>("Hints");
            foreach (HintSO hint in loadedHints)
            {
                hintDatabase[hint.hintId] = hint;
            }

            if (hintPanel != null) hintPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisplayHints(string[] hintIds)
    {
        List<HintSO> hintsToDisplay = new List<HintSO>();

        foreach (string hintId in hintIds)
        {
            if (hintDatabase.TryGetValue(hintId, out HintSO foundHint))
            {
                hintsToDisplay.Add(foundHint);
            }
            else
            {
                Debug.LogWarning($"Could not find a hint with the ID: {hintId}.");
            }
        }

        if (hintsToDisplay.Count > 0)
        {
            DisplayHints(hintsToDisplay);
        }
    }

    public void DisplayHints(List<HintSO> hints)
    {
        if (hints == null || hints.Count == 0) return;

        activeHints = hints;
        currentPageIndex = 0;
        hintPanel.SetActive(true);

        UpdatePageVisuals();
    }

    private void UpdatePageVisuals()
    {
        HintSO currentHint = activeHints[currentPageIndex];

        titleText.text = currentHint.displayName;
        descText.text = currentHint.description;

        if (currentHint.image != null)
        {
            hintImage.gameObject.SetActive(true);
            hintImage.sprite = currentHint.image;

            LayoutElement layoutElement = hintImage.GetComponent<LayoutElement>();

            if (layoutElement != null)
            {
                float originalWidth = currentHint.image.rect.width;
                float originalHeight = currentHint.image.rect.height;

                float ratio = originalWidth / originalHeight;

                layoutElement.preferredHeight = Mathf.Min(1000f / ratio, 560f);
            }
        }
        else
        {
            hintImage.gameObject.SetActive(false);
        }

        bool hasMultiplePages = activeHints.Count > 1;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(hasMultiplePages);
            if (hasMultiplePages) nextButton.interactable = (currentPageIndex < activeHints.Count - 1);
        }

        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(hasMultiplePages);
            if (hasMultiplePages) prevButton.interactable = (currentPageIndex > 0);
        }

        if (pageText != null)
        {
            pageText.gameObject.SetActive(hasMultiplePages);
            if (hasMultiplePages) pageText.text = $"{currentPageIndex + 1} / {activeHints.Count}";
        }

        if (hintScrollView != null)
        {
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