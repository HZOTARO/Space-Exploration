using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HintContentUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI subtitleText;
    public Image hintImage;
    public TextMeshProUGUI descriptionText;

    public void Setup(HintBlock blockData)
    {
        if (!string.IsNullOrEmpty(blockData.subtitle) && subtitleText != null)
        {
            subtitleText.text = blockData.subtitle;
            subtitleText.gameObject.SetActive(true);
        }
        else if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }

        if (blockData.image != null && hintImage != null)
        {
            hintImage.sprite = blockData.image;
            hintImage.gameObject.SetActive(true);

            LayoutElement layout = hintImage.GetComponent<LayoutElement>();
            if (layout != null && blockData.image.rect.height > 0)
            {
                float ratio = blockData.image.rect.width / blockData.image.rect.height;
                layout.preferredHeight = Mathf.Min(1000f / ratio, 500f);
            }
        }
        else if (hintImage != null)
        {
            hintImage.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(blockData.description) && descriptionText != null)
        {
            descriptionText.text = blockData.description;
            descriptionText.gameObject.SetActive(true);
        }
        else if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
    }
}