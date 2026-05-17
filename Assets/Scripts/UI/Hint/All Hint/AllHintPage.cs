using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AllHintPage : MonoBehaviour
{
    [Header ("Data")]
    public List<HintCollectionSO> hintCollections;

    [Header("Scroll View References")]
    public Transform contentContainer;
    public GameObject collectionPrefab;
    public GameObject hintButtonPrefab;

    [Header("References")]
    public TextMeshProUGUI otherText;
    public Transform infoPanel;

    [Header("Info Panel References")]
    public TextMeshProUGUI titleText;
    public Image hintImage;
    public Button seeDetail;

    [Header("Button Sprite")]
    public Sprite normalButtonSprite;
    public Sprite selectedButtonSprite;
    private HintButton currentlySelectedHint;

    private List<HintCollectionButtonList> spawnedCollections = new List<HintCollectionButtonList>();

    void Start()
    {
        Setup();
        ClearInfoPanel();
    }
    private void OnEnable()
    {
        RefreshAllHints();
    }
    private void OnDisable()
    {
        ClearInfoPanel();
    }

    void Setup()
    {
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        spawnedCollections.Clear();

        foreach (HintCollectionSO collection in hintCollections)
        {
            GameObject collectionObj = Instantiate(collectionPrefab, contentContainer);
            HintCollectionButtonList collectionUI = collectionObj.GetComponent<HintCollectionButtonList>();

            if (collectionUI != null)
            {
                collectionUI.Setup(collection.name, collection, this);
                spawnedCollections.Add(collectionUI);
            }
        }
    }
    public void RefreshAllHints()
    {
        foreach (HintCollectionButtonList collection in spawnedCollections)
        {
            if (collection != null) collection.RefreshCollection();
        }
    }

    public void OnSeeDetailClicked()
    {
        if (currentlySelectedHint != null && HintManager.instance != null)
        {
            HintManager.instance.RequestDisplayHints(currentlySelectedHint.hintCollection, currentlySelectedHint.hintData, false, false);
        }
    }

    public void OnHintButtonClicked(HintButton hintButton)
    {
        if (currentlySelectedHint != null && currentlySelectedHint.buttonImage != null)
        {
            currentlySelectedHint.buttonImage.sprite = normalButtonSprite;
        }

        currentlySelectedHint = hintButton;
        HintSO hintData = hintButton.hintData;

        if (currentlySelectedHint != null && currentlySelectedHint.buttonImage != null)
        {
            currentlySelectedHint.buttonImage.sprite = selectedButtonSprite;
        }

        otherText.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(true);

        if (titleText != null) titleText.text = hintData.title;

        if (hintImage != null)
        {
            Sprite image = null;
            foreach (HintBlock hintBlock in hintData.hintBlocks)
            {
                if (hintBlock.image != null) 
                {
                    image = hintBlock.image;
                    break;
                }
            }
            if (image)
            {
                hintImage.gameObject.SetActive(true);
                hintImage.sprite = image;
            }
            else
            {
                hintImage.gameObject.SetActive(false);
                hintImage.sprite = null;
            }
        }

        seeDetail.onClick.RemoveAllListeners();
        seeDetail.onClick.AddListener(OnSeeDetailClicked);
    }

    public void ClearInfoPanel()
    {
        if (currentlySelectedHint != null && currentlySelectedHint.buttonImage != null)
        {
            currentlySelectedHint.buttonImage.sprite = normalButtonSprite;
        }

        currentlySelectedHint = null;

        if (otherText != null) otherText.gameObject.SetActive(true);
        if (infoPanel != null) infoPanel.gameObject.SetActive(false);
    }
}
